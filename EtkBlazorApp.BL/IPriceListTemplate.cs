using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    public interface IPriceListTemplate
    {
        List<PriceLine> ReadPriceLines();
        string FileName { get; set; }
    }
}
