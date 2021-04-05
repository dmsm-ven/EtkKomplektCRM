using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("58AAB6CD-EED9-4B73-8FF1-E7B99F0C81F8")]
    public class EinhellQuantityTemplate : ExcelPriceListTemplateBase
    {
        public EinhellQuantityTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = "EIN-" + tab.Cells[row, 0].ToString();
                string name = tab.Cells[row, 1].ToString();
                string quantityString = tab.Cells[row, 3].ToString();

                int? quantity = quantityString == "в наличии" ? 5 : (quantityString == "нет в наличии" ? (int?)0 : null);

                if (!string.IsNullOrWhiteSpace(skuNumber) && quantity.HasValue)
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Einhell",
                        Sku = skuNumber,
                        Name = name,
                        Model = skuNumber,
                        Quantity = quantity
                    };

                    list.Add(priceLine);
                }

            }

            return list;
        }
    }

    [PriceListTemplateGuid("003F2DB1-34AB-4B98-9742-1708E3C6C0A7")]
    public class EinhellQuantity2Template : ExcelPriceListTemplateBase
    {
        public EinhellQuantity2Template(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for(int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = "EIN-" + tab.Cells[row, 1].ToString();
                string name = tab.Cells[row, 2].ToString();
                string quantityString = tab.Cells[row, 3].ToString();

                if (!string.IsNullOrWhiteSpace(skuNumber) && decimal.TryParse(quantityString, out var quantity))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Einhell",
                        Sku = skuNumber,
                        Name = name,
                        Model = skuNumber,
                        Quantity = (int)quantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
}
