using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("641779CC-C6F8-4CFB-9ABA-2C8136BF19FB")]
    public class SnabgenieRfProxxonPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SnabgenieRfProxxonPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 9; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 2)?.Replace("PR- ", "PX ");
                string name = tab.GetValue<string>(row, 3);
                string ean = tab.GetValue<string>(row, 4);
                decimal? price = 0;
                try
                {
                    price = ParsePrice(tab.GetValue<string>(row, 11), false, 4);
                }
                catch
                {

                }

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Proxxon",
                    Model = sku,
                    Sku = sku,
                    Ean = ean,
                    Name = name,
                    Price = price
                };
                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("1B4BCB27-245B-469A-92EC-43695288E161")]
    public class SnabgenieRfPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SnabgenieRfPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                string manufacturer = tab.GetValue<string>(row, 2);

                if(SkipThisBrand(manufacturer)) { continue; }

                string name = tab.GetValue<string>(row, 3);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 5));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Name = name,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
