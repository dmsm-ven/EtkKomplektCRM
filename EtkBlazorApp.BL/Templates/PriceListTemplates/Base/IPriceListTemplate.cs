using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IPriceListTemplate
    {
        Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null);
        string FileName { get; }
    }
}
