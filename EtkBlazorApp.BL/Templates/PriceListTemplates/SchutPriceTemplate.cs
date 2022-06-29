using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("F7730F08-2379-45EC-AA7F-9187C16F4E7B")]
    public class SchutPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SchutPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 2);
                decimal price = tab.GetValue<decimal>(row, 4);

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.RUB,
                    Manufacturer = "Schut",
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Price = price
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
