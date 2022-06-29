using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("59F2F712-24E7-4FB7-AC99-60AB139280AF")]
    public class BernsteinPriceListTemplate : ExcelPriceListTemplateBase
    {
        public BernsteinPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string model = tab.GetValue<string>(row, 3);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 4));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Bernstein",
                    Price = price,
                    Currency = CurrencyType.EUR,
                    Sku = sku,
                    Model = model,        
                    Name = name
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
