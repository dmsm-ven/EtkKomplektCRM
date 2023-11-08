using EtkBlazorApp.Core.Data;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("F014FD2E-F71D-41B1-960D-B2A2B2DABFDE")]
    public class ErsaFelderPriceListTemplate : ExcelPriceListTemplateBase, IPriceListTemplate
    {
        public ErsaFelderPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                string productName = tab.GetValue<string>(row, 2);
                string model = tab.GetValue<string>(row, 3);
                var price = ParsePrice(tab.GetValue<string>(row, 5));

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Ersa",
                    Model = model,
                    Sku = skuNumber,
                    Name = productName,
                    Price = price
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
