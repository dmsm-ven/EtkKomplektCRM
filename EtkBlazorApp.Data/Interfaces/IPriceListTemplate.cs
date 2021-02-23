using System.Collections.Generic;
using System.Threading;

namespace EtkBlazorApp.Data
{
    public interface IPriceListTemplate
    {
        List<PriceLine> ReadPriceLines(CancellationToken? token = null);
        string FileName { get; set; }
    }
}
