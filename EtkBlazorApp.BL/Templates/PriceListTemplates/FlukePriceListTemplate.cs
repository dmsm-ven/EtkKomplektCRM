using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("0239BAB4-6675-4839-B8B2-4671B5636F7E")]
    public class FlukeQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public FlukeQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 3));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Fluke",
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
