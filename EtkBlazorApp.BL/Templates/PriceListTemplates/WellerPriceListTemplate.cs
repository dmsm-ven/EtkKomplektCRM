using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("2CA8AB25-8AA1-4008-977C-4253378E1BA1")]
    public class WellerPriceListTemplate : ExcelPriceListTemplateBase
    {
        public WellerPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int i = 18; i < tab.Dimension.Rows; i++)
            {
                string skuNumber = tab.GetValue<string>(i, 1);
                if (Regex.IsMatch(skuNumber, @"^(\d){6,}N$"))
                {
                    skuNumber = skuNumber.TrimEnd('N');
                }

                string partNumber = tab.GetValue<string>(i, 2);

                string name = tab.GetValue<string>(i, 4);
                decimal? priceInEuro = ParsePrice(tab.GetValue<string>(i, 7));

                var priceLine = new PriceLine(this)
                {
                    Sku = skuNumber,
                    Name = name,
                    Model = partNumber,
                    Price = priceInEuro,
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Weller"
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("56CF16C1-CD99-41C1-909F-B3031695C0C5")]
    public class WellerStockPriceListTemplate : ExcelPriceListTemplateBase
    {
        public WellerStockPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 12; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);
                string ean = tab.GetValue<string>(row, 4);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 6));

                if (string.IsNullOrWhiteSpace(skuNumber)) { continue; }

                if (Regex.IsMatch(skuNumber, @"^(\d){6,}N$"))
                {
                    skuNumber = skuNumber.TrimEnd('N');
                }
                if (Regex.IsMatch(skuNumber, @"^T00(\d+)$"))
                {
                    skuNumber = skuNumber.Substring(3);
                }

                var line = new PriceLine(this)
                {
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity,
                    Ean = ean,
                    Manufacturer = "Weller",
                    Stock = StockName.SpringE
                };
                list.Add(line);
            }

            return list;
        }
    }
}
