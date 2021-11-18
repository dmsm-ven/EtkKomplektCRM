using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("8BBB5D89-BA3F-4C48-AE2C-B023A08AEF55")]
    public class DinoLitePriceListTemplate : ExcelPriceListTemplateBase
    {
        public DinoLitePriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 2);
                var price = ParsePrice(tab.GetValue<string>(row, 4), canBeNull: true, 4);

                if (!price.HasValue) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Dino-Lite",
                    Model = sku,
                    Sku = sku,
                    Price = price
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
