using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public class SimplePriceLineLoadCorrelator : IPriceLineLoadCorrelator
    {
        public PriceLine FindCorrelation(PriceLine line, List<PriceLine> priceLines)
        {
            if(string.IsNullOrWhiteSpace(line.Model) || string.IsNullOrWhiteSpace(line.Sku))
            {
                return null;
            }

            return priceLines.FirstOrDefault(l => 
                line.Model.Equals(line.Model, System.StringComparison.OrdinalIgnoreCase) ||
                line.Sku.Equals(l.Sku, System.StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}
