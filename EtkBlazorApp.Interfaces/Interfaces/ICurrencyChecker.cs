using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface ICurrencyChecker
{
    DateTime LastUpdate { get; }
    Task<decimal> GetCurrencyRate(CurrencyType type);
}
