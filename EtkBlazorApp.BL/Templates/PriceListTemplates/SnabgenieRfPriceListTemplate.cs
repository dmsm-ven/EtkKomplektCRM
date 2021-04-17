using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("1B4BCB27-245B-469A-92EC-43695288E161")]
    public class SnabgenieRfPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SnabgenieRfPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string manufacturer = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string quantityString = tab.GetValue<string>(row, 4);

                if (decimal.TryParse(quantityString, out var quantity))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = manufacturer,
                        Sku = skuNumber,
                        Model = skuNumber,
                        Name = name,
                        Quantity = (int)quantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
}
