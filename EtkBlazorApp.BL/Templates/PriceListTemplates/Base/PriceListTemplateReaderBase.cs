using System;
using System.Globalization;

namespace EtkBlazorApp.BL
{
    public abstract class PriceListTemplateReaderBase
    {
        protected virtual decimal? ParsePrice(string str)
        {
            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str.Replace(",", ".").Replace(" ", string.Empty), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = parsedPrice;
                }
            }

            return price;
        }

        protected virtual int? ParseQuantity(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var quantity))
                {
                    return Math.Max((int)quantity, 0);
                }
            }
            return null;
        }
    }
}
