using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA")]
    public class MegeonPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MegeonPriceListTemplate(string fileName) : base(fileName) 
        {
            QuantityMap["Более 10"] = 10;
            ManufacturerNameMap["МЕГЕОН"] = "Мегеон";
            ValidManufacturerNames.Add("Мегеон");
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 3));
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 4));
                decimal price = tab.GetValue<decimal>(row, 5);
                CurrencyType currency = CurrencyType.RUB;
                if(Enum.TryParse(tab.GetValue<string>(row, 7)?.Replace("руб.", "RUB"), true, out currency)) { }

                if (!ValidManufacturerNames.Contains(manufacturer)) { continue; }

                var match1 = Regex.Match(name, @"^(.*?) (МЕГЕОН \S+)");
                var match2 = Regex.Match(name, @"МЕГЕОН \S+");
                string model = match1.Success ? match1.Groups[2].Value : (match2.Success ? match2.Value : null);

                var priceLine = new PriceLine(this)
                {
                    Currency = currency,
                    Manufacturer = manufacturer,
                    Model = model,
                    Sku = sku,
                    Price = price,
                    Quantity = quantity,
                    Name = name
                };
                list.Add(priceLine);              
            }

            return list;
        }
    }
}
