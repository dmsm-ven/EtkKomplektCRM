using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("5785C822-A57D-4DD2-9B68-E0301DDF135B")]
    public class BoschPriceListTemplate : ExcelPriceListTemplateBase
    {
        public BoschPriceListTemplate(string fileName): base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1).ToString()
                    .Insert(1, " ").Insert(5, " ").Insert(9, " ");

                string stockStatusCode = tab.GetValue<string>(row, 2);
                decimal price = tab.GetValue<decimal>(row, 3);

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.RUB,
                    Model = sku,
                    Sku = sku,
                    Price = price,
                    Quantity = QuantityMap[stockStatusCode],
                    Manufacturer = "Bosch",
                    Stock = StockName.BoschB2B
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
