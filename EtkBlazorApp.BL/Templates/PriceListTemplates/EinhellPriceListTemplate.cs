using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("003F2DB1-34AB-4B98-9742-1708E3C6C0A7")]
    public class EinhellStockListTemplate : ExcelPriceListTemplateBase
    {
        public EinhellStockListTemplate(string fileName) : base(fileName) { }

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

    [PriceListTemplateGuid("879C6323-F186-45AD-90A4-2699011A5F5E")]
    public class EinhellPriceListTemplate : ExcelPriceListTemplateBase
    {
        public EinhellPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 4; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 2);
                string skuNumber = "EIN-" + sku;
                string name = tab.GetValue<string>(row, 4);
                var rrcPrice = ParsePrice(tab.GetValue<string>(row, 10));

                if (string.IsNullOrWhiteSpace(sku)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Einhell",
                    //Stock = StockName.Einhell,
                    Sku = skuNumber,
                    Name = name,
                    Model = skuNumber,
                    Currency = CurrencyType.RUB,
                    Price = rrcPrice
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
