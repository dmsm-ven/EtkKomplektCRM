using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("7F3DE005-28A6-4E02-9148-EF068B40C4E8")]
    public class ElectrolubePriceListTemplate : ExcelPriceListTemplateBase
    {
        public ElectrolubePriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for(int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 2));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Electrolube",
                    //Stock = StockName.Borel,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
