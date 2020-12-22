using EtkBlazorApp.BL.Data;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public interface IPriceLineLoadCorrelator
    {
        PriceLine FindCorrelation(PriceLine line, List<PriceLine> priceLines);
    }


}
