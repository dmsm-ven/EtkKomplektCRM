using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{

    [PriceListTemplateGuid("C594AA8D-6D76-42E2-A35C-B19AB4B0E780")]
    public class CemPriceListTemplate : ExcelPriceListTemplateBase
    {
        readonly string SKU_PREFIX = "CEM-";

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
                decimal priceInRUR = tab.GetValue<decimal>(row, 10);
                int quantity = tab.GetValue<int>(row, 11);

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Manufacturer = "CEM",
                    Model = sku,
                    Ean = ean,
                    Sku = sku,
                    Price = priceInRUR,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

}
