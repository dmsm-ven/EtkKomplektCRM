using EtkBlazorApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateDescription("2CA8AB25-8AA1-4008-977C-4253378E1BA1")]
    public class WellerPriceListTemplate : ExcelPriceListTemplateBase
    {
        const int START_ROW_NUMBER = 15;

        public WellerPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int i = START_ROW_NUMBER; i < tab.Dimension.Rows; i++)
            {
                string skuNumber = tab.GetValue<string>(i, 0);
                if (Regex.IsMatch(skuNumber, @"^(\d){6,}N$"))
                {
                    skuNumber = skuNumber.TrimEnd('N');
                }

                string partNumber = tab.GetValue<string>(i, 1);
                if (Regex.IsMatch(partNumber, @"^T00(\d+)$"))
                {
                    partNumber = partNumber.Substring(3);
                }

                string priceString = tab.GetValue<string>(i, 6);

                if (decimal.TryParse(priceString, out var priceInEuro))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Sku = skuNumber,
                        Model = partNumber,
                        Price = priceInEuro,
                        Currency = CurrencyType.EUR
                    };

                    list.Add(priceLine);
                }            
            }

            return list;
        }
    }

    [PriceListTemplateDescription("56CF16C1-CD99-41C1-909F-B3031695C0C5")]
    public class WellerStockPriceListTemplate : ExcelPriceListTemplateBase
    {
        const int START_ROW_NUMBER = 9;

        public WellerStockPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int i = START_ROW_NUMBER; i < tab.Dimension.Rows; i++)
            {
                string skuNumber = tab.GetValue<string>(i, 1);
                string name = tab.GetValue<string>(i, 2);
                string quantityString = tab.GetValue<string>(i, 5);

                if (string.IsNullOrWhiteSpace(skuNumber)) { continue; }

                if (Regex.IsMatch(skuNumber, @"^(\d){6,}N$"))
                {
                    skuNumber = skuNumber.TrimEnd('N');
                }
                if (Regex.IsMatch(skuNumber, @"^T00(\d+)$"))
                {
                    skuNumber = skuNumber.Substring(3);
                }

                if (decimal.TryParse(quantityString, out var quantity))
                {
                    var line = new PriceLine(this)
                    {
                        Name = name,
                        Sku = skuNumber,
                        Model = skuNumber,
                        Quantity = (int)quantity
                    };
                    list.Add(line);
                }
            }

            return list;
        }
    }
}
