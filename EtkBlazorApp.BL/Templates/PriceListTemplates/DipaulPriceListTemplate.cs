using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("C53B8C85-3115-421F-A579-0B5BFFF6EF48")]
    public class DipaulPriceListTemplate : ExcelPriceListTemplateBase
    {
        public DipaulPriceListTemplate(string fileName) : base(fileName) 
        {
            ValidManufacturerNames.AddRange(new[] { "Hakko", "Keysight", "ITECH" });
            ManufacturerNameMap["ITECH ВЭД"] = "ITECH";
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.Cells[row, 2].ToString();

                manufacturer = ManufacturerNameMap.ContainsKey(manufacturer) ? ManufacturerNameMap[manufacturer] : manufacturer;

                if (!ValidManufacturerNames.Contains(manufacturer)) { continue; }

                string skuNumber = tab.GetValue<string>(row, 1);
                string productName = tab.GetValue<string>(row, 2);
                string quantityString = tab.GetValue<string>(row, 4);
                string priceString = tab.GetValue<string>(row, 5);
                string model = Regex.Match(productName, "^(.*?), ").Groups[1].Value;
                string currencyTypeString = tab.Cells[row, 5]?.ToString()?.Replace("руб.", "RUB");

                CurrencyType? priceCurreny = null;
                if (!string.IsNullOrEmpty(currencyTypeString))
                {
                    priceCurreny = Enum.Parse<CurrencyType>(currencyTypeString);
                }

                decimal? parsedPrice = null;
                if (priceCurreny.HasValue && decimal.TryParse(priceString, out var price))
                {
                    parsedPrice = price;
                }

                int? parsedQuantity = null;
                if (int.TryParse(quantityString, out var quantity))
                {
                    parsedQuantity = Math.Max(quantity, 0);
                }

                if (parsedPrice.HasValue || parsedQuantity.HasValue)
                {
                    var priceLine = new PriceLine(this)
                    {
                        Name = productName,
                        Manufacturer = manufacturer,
                        Model = model,
                        Sku = skuNumber,
                        Price = parsedPrice,
                        Currency = priceCurreny.Value,
                        Quantity = parsedQuantity
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("5CFDD5BD-816C-44DC-8AF3-9418F4052BF2")]
    public class HakkoPriceListTemplate : ExcelPriceListTemplateBase
    {
        public HakkoPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string priceString = tab.GetValue<string>(row, 3);

                if (decimal.TryParse(priceString, out var price))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Name = name,
                        Currency = CurrencyType.RUB,
                        Manufacturer = "Hakko",
                        Sku = skuNumber,
                        Price = price
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
        
    }
}
