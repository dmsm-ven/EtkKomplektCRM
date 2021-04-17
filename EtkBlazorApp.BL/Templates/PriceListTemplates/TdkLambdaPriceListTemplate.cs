using EtkBlazorApp.BL.Templates;
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

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0).ToString();
                decimal price = tab.GetValue<decimal>(row, 1);

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.USD,
                    Manufacturer = PrikatReportTemplateBase.PRIKAT_ONLY_PREFIX + "TDK-Lambda",
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
