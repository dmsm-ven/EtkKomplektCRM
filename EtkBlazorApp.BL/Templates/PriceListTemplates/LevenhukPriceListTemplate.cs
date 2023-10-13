using EtkBlazorApp.BL.Data;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("BD241653-AE4F-4173-9172-988144A52CB6")]
    public class LevenhukPriceListTemplate : ExcelPriceListTemplateBase
    {
        public LevenhukPriceListTemplate(string fileName) : base(fileName) { }
        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 17; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);

                if (string.IsNullOrWhiteSpace(sku))
                {
                    continue;
                }

                string name = tab.GetValue<string>(row, 2);
                var rrcPrice = ParsePrice(tab.GetValue<string>(row, 3));

                var quantityMsk = ParseQuantity(tab.GetValue<string>(row, 6));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 8));

                var ean = tab.GetValue<string>(row, 11);

                var priceLine = new MultistockPriceLine(this)
                {
                    Quantity = quantitySpb,
                    Stock = StockName.Levenhuk_Spb,
                    Sku = sku,
                    Model = sku,
                    Name = name,
                    Currency = Core.Data.CurrencyType.RUB,
                    Price = rrcPrice,
                    Ean = ean,
                    Manufacturer = "Ermenrich",
                };

                priceLine.AdditionalStockQuantity[StockName.Levenhuk_Msk] = quantityMsk.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }
}
