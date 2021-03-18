using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Data
{
    public interface IPriceListTemplate
    {
        Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null);
        string FileName { get; }
    }
}
