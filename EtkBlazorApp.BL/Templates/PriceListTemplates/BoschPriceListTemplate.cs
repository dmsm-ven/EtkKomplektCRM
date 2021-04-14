using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("5785C822-A57D-4DD2-9B68-E0301DDF135B")]
    public class BoschPriceListTemplate : ExcelPriceListTemplateBase
    {
        private Dictionary<string, int> stockStatusCodeToQuantity = new Dictionary<string, int>()
        {
            ["A"] = 10, // Доступно
            ["B"] = 5, // Менее 5 штук
            ["C"] = 0, // Не доступно
        };

        public BoschPriceListTemplate(string fileName): base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int i = 2; i < tab.Dimension.Rows; i++)
            {
                string sku = tab.Cells[i, 1].GetValue<string>().ToString().Insert(1, " ").Insert(5, " ").Insert(9, " ");
                string stockStatusCode = tab.Cells[i, 2].GetValue<string>();
                decimal price = tab.Cells[i, 3].GetValue<decimal>();

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.RUB,
                    Model = sku,
                    Sku = sku,
                    Price = price,
                    Quantity = stockStatusCodeToQuantity[stockStatusCode],
                    Manufacturer = "Bosch"
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
