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
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.Cells[row, 0].ToString();
                string quantityString = tab.Cells[row, 2].ToString();

                int? parsedQuantity = null;
                if (decimal.TryParse(quantityString, out var quantity))
                {
                    parsedQuantity = Math.Max((int)quantity, 0);
                }

                if (parsedQuantity.HasValue)
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Fluke",
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
