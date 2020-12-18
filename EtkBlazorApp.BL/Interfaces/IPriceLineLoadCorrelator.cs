using EtkBlazorApp.BL.Data;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public interface IPriceLineLoadCorrelator
    {
        PriceLine FindCorrelation(PriceLine line, List<PriceLine> priceLines);
    }

    public class SimplePriceLineLoadCorrelator : IPriceLineLoadCorrelator
    {
        public PriceLine FindCorrelation(PriceLine line, List<PriceLine> priceLines)
        {
            return priceLines
                .FirstOrDefault(l => line.Model.Equals(line.Model, System.StringComparison.OrdinalIgnoreCase) ||
                                     line.Sku.Equals(l.Sku, System.StringComparison.OrdinalIgnoreCase)
                                     );
        }
    }
}
