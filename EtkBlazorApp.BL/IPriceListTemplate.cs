using System.Collections.Generic;
using System.Threading;

namespace EtkBlazorApp.BL
{
    public interface IPriceListTemplate
    {
        List<PriceLine> ReadPriceLines(CancellationToken? token = null);
        string FileName { get; set; }
    }
}
