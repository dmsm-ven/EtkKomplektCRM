using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Data
{
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
