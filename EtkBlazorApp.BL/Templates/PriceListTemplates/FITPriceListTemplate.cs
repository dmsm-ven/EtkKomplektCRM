using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("91EE5CFF-4752-4D6F-8E36-E5557149225B")]
    public class FITQuantityTemplate : ExcelPriceListTemplateBase
    {
        public FITQuantityTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 3);
                if (SkipThisBrand(manufacturer)) { continue; }

                string skuNumber = "FIT-" + tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string ean = tab.GetValue<string>(row, 4);
                decimal price = tab.GetValue<decimal>(row, 6);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 7).Trim(' ', '>', '<'));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    //Stock = StockName.FIT,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Name = name,
                    Ean = ean,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
