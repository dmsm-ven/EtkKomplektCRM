using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("0D58935B-3D84-48F3-98E3-35E99EBAC96C")]
    public class BergerriceListTemplate : ExcelPriceListTemplateBase
    {
        public BergerriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 3; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);               
                var priceInRUR = ParsePrice(tab.GetValue<string>(row, 7));
                var quantity = ParseQuantity(tab.GetValue<string>(row, 8));

                if (string.IsNullOrWhiteSpace(sku)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.Berger,
                    Manufacturer = "Berger",
                    Model = sku,
                    Ean = sku,
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
