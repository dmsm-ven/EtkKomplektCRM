using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface ICurrencyChecker
{
    DateTime LastUpdate { get; }
    ValueTask<decimal> GetCurrencyRate(CurrencyType type);
}
