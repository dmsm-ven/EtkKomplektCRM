using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("DB9EBC46-639C-4E12-8D9E-B59179029285")]
    public class KnipexPriceListTemplate : ExcelPriceListTemplateBase
    {
        public KnipexPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 4; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 2);
                string skuNumber = tab.GetValue<string>(row, 3);
                string name = tab.GetValue<string>(row, 4);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 6));
                string ean = tab.GetValue<string>(row, 7);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 8));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.Knipex,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity,
                    Ean = ean,          
                    Name = name
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
