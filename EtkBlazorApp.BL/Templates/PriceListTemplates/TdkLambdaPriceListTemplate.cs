using EtkBlazorApp.BL.Templates;
using EtkBlazorApp.Core.Data;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("6137CA73-3748-47F9-BC53-CF6B3C4F296A")]
    public class TdkLambdaPriceListTemplate : ExcelPriceListTemplateBase
    {
        public TdkLambdaPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets.First(t => t.Name.Contains("DIN"));

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                decimal price = tab.GetValue<decimal>(row, 6); // под заказ

                if (string.IsNullOrWhiteSpace(skuNumber)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.USD,
                    Manufacturer = "TDK-Lambda",
                    Model = skuNumber,
                    Sku = skuNumber,
                    Price = price
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
