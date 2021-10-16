using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("003F2DB1-34AB-4B98-9742-1708E3C6C0A7")]
    public class EinhellPriceListTemplate : ExcelPriceListTemplateBase
    {
        public EinhellPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for(int row = 3; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = "EIN-" + tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 4));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Einhell",
                    //Stock = StockName.Einhell,
                    Sku = skuNumber,
                    Name = name,
                    Model = skuNumber,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
