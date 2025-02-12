using System;
using System.Collections.Generic;
using BamboCard.Application.Contract;

public class CurrencyProviderFactory
{
    private readonly Dictionary<string, IExchangeRateService> _providers;

    public CurrencyProviderFactory(IEnumerable<IExchangeRateService> providers)
    {
        _providers = new Dictionary<string, IExchangeRateService>();
        foreach (var provider in providers)
        {
            _providers[provider.GetType().Name] = provider;
        }
    }

    public IExchangeRateService GetProvider(string providerName)
    {
        if (_providers.ContainsKey(providerName))
        {
            return _providers[providerName];
        }

        throw new ArgumentException($"Currency provider '{providerName}' not found.");
    }
}
