using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BamboCard.EF.Model;
using Microsoft.EntityFrameworkCore;

namespace BamboCard.EF
{
    
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }
    }

}
