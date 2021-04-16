using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface ICurrencyChecker
    {
        DateTime LastUpdate { get; }
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }
}
