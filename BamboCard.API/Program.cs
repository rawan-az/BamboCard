using BamboCard.Application.Contract;
using BamboCard.Application; 
using BamboCard.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using AspNetCoreRateLimit;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Polly.Extensions.Http;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 

builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
 
builder.Services.AddHttpClient<FrankfurterCurrencyProvider>();
builder.Services.AddHttpClient<OpenExchangeRatesProvider>();
// Register Factory
builder.Services.AddSingleton<CurrencyProviderFactory>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-auth-provider.com";
        options.Audience = "CurrencyAPI";
    });

builder.Services.AddAuthorization();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt")
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation() 
        .AddConsoleExporter());


builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});


// Define retry policy with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        5,  // Retry up to 5 times
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2^1, 2^2, etc.
        (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds} seconds due to {outcome.Exception?.Message}");
        });

// Define Circuit Breaker Policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        3, // Break the circuit after 3 consecutive failures
        TimeSpan.FromMinutes(1), // Wait 1 min before allowing requests again
        (exception, timespan) =>
        {
            Console.WriteLine($"Circuit Breaker triggered! Pausing requests for {timespan.TotalSeconds} seconds.");
        },
        () =>
        {
            Console.WriteLine("Circuit Breaker reset - API calls allowed again.");
        });

// Combine Policies: Retry first, then Circuit Breaker
var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
