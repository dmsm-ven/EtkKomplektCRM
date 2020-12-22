using EtkBlazorApp.BL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Interfaces
{
    public interface ICurrencyChecker
    {
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }
}
