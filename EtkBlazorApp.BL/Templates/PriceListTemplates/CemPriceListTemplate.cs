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
                decimal price = tab.GetValue<decimal>(row, 7);

                var quantityMsk = ParseQuantity(tab.GetValue<string>(row, 8));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 11));

                var priceLine = new MultistockPriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Quantity = quantitySpb,
                    Stock = StockName.CEM_Spb,
                    Manufacturer = "CEM",
                    Model = sku,
                    Ean = ean,
                    Sku = sku,
                    Price = price
                };

                priceLine.AdditionalStockQuantity[StockName.CEM_Msk] = quantityMsk.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }


}
