using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("9EEB7A82-1029-4C1F-A282-196C0907160B")]
    public class CubCadetPriceListTemplate : ExcelPriceListTemplateBase
    {
        const int START_ROW_NUMBER = 7;
        public CubCadetPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {
                string model = tab.Cells[row, 1].ToString();
                string quantityString = tab.Cells[row, 4].ToString();

                if (!string.IsNullOrWhiteSpace(model))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Cub Cadet",
                        Model = model,
                        Sku = model,
                        Quantity = ParseQuantity(quantityString)
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }    
    }
}
