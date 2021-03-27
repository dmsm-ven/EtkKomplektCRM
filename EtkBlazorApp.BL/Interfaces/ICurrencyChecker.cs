using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface ICurrencyChecker
    {
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }
}
