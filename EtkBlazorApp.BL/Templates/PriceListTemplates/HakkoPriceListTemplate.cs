using EtkBlazorApp.Core.Data;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("5CFDD5BD-816C-44DC-8AF3-9418F4052BF2")]
    public class HakkoPriceListTemplate : ExcelPriceListTemplateBase
    {
        public HakkoPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string priceString = tab.GetValue<string>(row, 3);

                if (decimal.TryParse(priceString, out var price))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Name = name,
                        Currency = CurrencyType.RUB,
                        Manufacturer = "Hakko",
                        Sku = skuNumber,
                        Price = price
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }

    }
}
