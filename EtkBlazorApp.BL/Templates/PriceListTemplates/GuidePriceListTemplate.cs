using EtkBlazorApp.BL.Data;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("675483A2-3D98-42CC-8C65-A0096F9054F1")]
    public class GuidePriceListTemplate : ExcelPriceListTemplateBase
    {
        public GuidePriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 6; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);

                if (string.IsNullOrWhiteSpace(sku))
                {
                    continue;
                }

                //Хардкод значение
                if (sku.Contains("MobIR Air(Grey)"))
                {
                    sku = "MobIR Air";
                }

                string name = tab.GetValue<string>(row, 3);

                var quantityMsk = ParseQuantity(tab.GetValue<string>(row, 6));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 7));

                var priceLine = new MultistockPriceLine(this)
                {
                    Name = name,
                    Quantity = quantitySpb,
                    Stock = StockName.Guide_Spb,
                    Manufacturer = "Guide",
                    Model = sku,
                    Sku = sku,
                };

                priceLine.AdditionalStockQuantity[StockName.Guide_Msk] = quantityMsk.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }


}
