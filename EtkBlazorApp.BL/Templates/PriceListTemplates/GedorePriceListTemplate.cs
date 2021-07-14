using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("8AD201D0-C12F-4B07-BAA0-AA6624AD01CC")]
    public class GedorePriceListTemplate : ExcelPriceListTemplateBase
    {
        public GedorePriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 3));                

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Manufacturer = "Gedore",
                    Model = sku,
                    Sku = sku,
                    Quantity = quantity,
                    Stock = StockName.GedoreTools
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

}
