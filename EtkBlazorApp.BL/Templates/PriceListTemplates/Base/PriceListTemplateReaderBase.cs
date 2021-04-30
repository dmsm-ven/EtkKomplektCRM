using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public abstract class PriceListTemplateReaderBase
    {
        protected Dictionary<string, string> ManufacturerNameMap { get; private set; }
        protected Dictionary<string, int> QuantityMap { get; private set; }
        protected List<string> ValidManufacturerNames { get; private set; }
        protected List<string> SkipManufacturerNames { get; private set; }

        public PriceListTemplateReaderBase()
        {
            ManufacturerNameMap = new Dictionary<string, string>();
            QuantityMap = new Dictionary<string, int>();
            ValidManufacturerNames = new List<string>();
            SkipManufacturerNames = new List<string>();
        }

        protected string MapManufacturerName(string manufacturerName)
        {
            if (!string.IsNullOrWhiteSpace(manufacturerName) && ManufacturerNameMap.Any() && ManufacturerNameMap.ContainsKey(manufacturerName))
            {
                return ManufacturerNameMap[manufacturerName];
            }
            return manufacturerName;
        }

        protected virtual decimal? ParsePrice(string str, bool canBeNull = false, int? roundDigits = null)
        {
            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str.Replace(",", ".").Replace(" ", string.Empty), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = Math.Max(parsedPrice, 0);
                    if (roundDigits.HasValue)
                    {
                        price = Math.Round(price.Value, roundDigits.Value);
                    }
                }
            }

            return canBeNull ? price : (price ?? 0);
        }

        protected virtual int? ParseQuantity(string str, bool canBeNull = false)
        {
            int? quantity = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedQuantity))
                {
                    quantity = Math.Max((int)parsedQuantity, 0);
                }
                else if(QuantityMap.ContainsKey(str))
                {
                    quantity = QuantityMap[str];
                }
            }

            return canBeNull ? quantity : (quantity ?? 0);
        }
    }
}
