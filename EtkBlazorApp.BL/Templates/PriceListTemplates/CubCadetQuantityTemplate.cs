using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("9EEB7A82-1029-4C1F-A282-196C0907160B")]
    public class CubCadetPriceListTemplate : ExcelPriceListTemplateBase
    {
        public CubCadetPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 7; row < tab.Dimension.Rows; row++)
            {
                string model = tab.GetValue<string>(row, 1);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 4));

                if (!string.IsNullOrWhiteSpace(model))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Cub Cadet",
                        Model = model,
                        Sku = model,
                        Quantity = quantity
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }    
    }
}
