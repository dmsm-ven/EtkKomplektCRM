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
            if (ManufacturerNameMap.Any() && ManufacturerNameMap.ContainsKey(manufacturerName))
            {
                return ManufacturerNameMap[manufacturerName];
            }
            return manufacturerName;
        }

        protected virtual decimal? ParsePrice(string str, bool canBenNull = false)
        {
            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str.Replace(",", ".").Replace(" ", string.Empty), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = parsedPrice;
                }
            }

            return canBenNull ? price : (price ?? 0);
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
