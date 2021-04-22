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

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.Cells[row, 1].ToString();
                string quantityString = tab.Cells[row, 4].ToString();
                string manufacturer = tab.Cells[row, 0].ToString();

                int? parsedQuantity = null;
                if (decimal.TryParse(quantityString, out var quantity))
                {
                    parsedQuantity = (int)quantity;
                }

                if (parsedQuantity.HasValue)
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = manufacturer,
                        Sku = skuNumber,
                        Model = skuNumber,
                        Quantity = parsedQuantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
}
