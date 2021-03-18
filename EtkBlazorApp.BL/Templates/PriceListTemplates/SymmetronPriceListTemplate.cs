using EtkBlazorApp.Data;
using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateDescription("3853B988-DB37-4B6E-861F-3000B643FAC4")]
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int i = 2; i < tab.Dimension.Rows; i++)
            {
                var priceLine = new PriceLine(this)
                {
                    Name = tab.GetValue<string>(i, 1),
                    Sku = tab.GetValue<string>(i, 3),
                    Model = tab.GetValue<string>(i, 27),
                    Manufacturer = tab.GetValue<string>(i, 26),
                    Price = ParsePrice(tab.GetValue<string>(i, 13)),
                    Currency = CurrencyType.RUB
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
