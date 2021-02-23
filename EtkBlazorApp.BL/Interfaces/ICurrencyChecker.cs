using EtkBlazorApp.Data;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Interfaces
{
    public interface ICurrencyChecker
    {
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }
}
