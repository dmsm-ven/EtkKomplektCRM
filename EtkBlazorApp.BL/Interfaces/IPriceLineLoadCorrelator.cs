using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    public interface IPriceLineLoadCorrelator
    {
        PriceLine FindCorrelation(PriceLine line, List<PriceLine> priceLines);
    }


}
