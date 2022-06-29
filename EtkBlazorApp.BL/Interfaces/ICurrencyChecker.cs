using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface ICurrencyChecker
    {
        DateTime LastUpdate { get; }
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }
}
