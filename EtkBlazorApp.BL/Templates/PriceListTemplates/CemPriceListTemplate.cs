using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{

    [PriceListTemplateGuid("C594AA8D-6D76-42E2-A35C-B19AB4B0E780")]
    public class CemPriceListTemplate : ExcelPriceListTemplateBase
    {
        readonly int START_ROW_NUMBER = 19;
        readonly string SKU_PREFIX = "CEM-";

        public CemPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {
                string quantityString = tab.Cells[row, 10].ToString().Trim();
                string priceInRUR = tab.Cells[row, 9].ToString().Trim();
                string name = tab.Cells[row, 0].ToString().Trim();
                string sku = SKU_PREFIX + tab.Cells[row, 3].ToString().Trim();

                if (decimal.TryParse(priceInRUR.Trim(), out decimal price)) { }
                if (int.TryParse(quantityString, out int quantity)) { }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Manufacturer = "CEM",
                    Model = sku,
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
