using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA")]
    public class MegeonPriceListTemplate : ExcelPriceListTemplateBase
    {
        const string MODEL_REGEX = @"МЕГЕОН \S+";

        public MegeonPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                var cell = ((dynamic)tab.Cells.Value);
                string nameWithoutAddSpace = Regex.Replace(cell[row, 1], " {2,}", " ").Trim();

                if (!Regex.IsMatch(nameWithoutAddSpace, MODEL_REGEX)) { continue; }
                
                string skuNumber = Regex.Match(nameWithoutAddSpace, MODEL_REGEX).Value;
                string priceString = cell[row, 4].ToString().Replace(" ", string.Empty);
                string quantityString = cell[row, 3].ToString();

                int parsedQuantity = 0;
                if (!int.TryParse(quantityString, out parsedQuantity))
                {
                    parsedQuantity = 10;
                }

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.RUB,
                    Manufacturer = "Мегеон",
                    Model = skuNumber,
                    Sku = skuNumber,
                    Price = ParsePrice(priceString),
                    Quantity = parsedQuantity
                };
                list.Add(priceLine);              
            }

            return list;
        }
    }
}
