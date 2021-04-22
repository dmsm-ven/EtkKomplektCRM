using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                string skuNumber = tab.Cells[row, 0].ToString();
                string productName = tab.Cells[row, 1].ToString();
                string model = tab.Cells[row, 2].ToString();
                string priceString = tab.Cells[row, 4].ToString();

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Ersa",
                    Model = model,
                    Sku = skuNumber,
                    Name = productName,
                    Price = ParsePrice(priceString)
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
