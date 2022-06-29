using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    // Хардкод пересчет USD в рубли по курсу 84 руб., дата изменния 12.06.2022
    [PriceListTemplateGuid("C594AA8D-6D76-42E2-A35C-B19AB4B0E780")]
    public class CemPriceListTemplate : ExcelPriceListTemplateBase
    {
        readonly string SKU_PREFIX = "CEM-";
        readonly decimal RUB_FOR_USD = 84;

        public CemPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 21; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 2);
                string ean = tab.GetValue<string>(row, 3);
                string sku = SKU_PREFIX + tab.GetValue<string>(row, 4);
                decimal price = RUB_FOR_USD * tab.GetValue<decimal>(row, 10);
                int quantity = tab.GetValue<int>(row, 11);

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    //Stock = StockName.CEM,
                    Manufacturer = "CEM",
                    Model = sku,
                    Ean = ean,
                    Sku = sku,
                    Price = price,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
