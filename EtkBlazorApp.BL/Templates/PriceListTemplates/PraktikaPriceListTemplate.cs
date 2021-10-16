using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("6C238D2C-145E-4320-B4E3-DCA8B8FAECB0")]
    public class PraktikaQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public PraktikaQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 2);
                string skuNumber = tab.GetValue<string>(row, 4);
                string name = tab.GetValue<string>(row, 5);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 7));
                string quantityString = tab.GetValue<string>(row, 8);
                
                int quantity = QuantityMap[quantityString];

                if (string.IsNullOrEmpty(skuNumber) || ManufacturerSkipCheck(manufacturer)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Quantity = quantity,
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Currency = CurrencyType.RUB,
                    Price = price,
                    Manufacturer = manufacturer
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
