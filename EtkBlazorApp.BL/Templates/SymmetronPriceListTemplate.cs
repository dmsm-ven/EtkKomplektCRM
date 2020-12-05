using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EtkBlazorApp.BL
{
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int i = 2; i < tab.Dimension.Rows; i++)
            {
                if (CancelToken.HasValue && (i % 100 == 0) && CancelToken.Value.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Отменено пользователем");
                }

                var priceLine = new PriceLine()
                {
                    Name = tab.GetValue<string>(i, 1),
                    Sku = tab.GetValue<string>(i, 3),
                    Model = tab.GetValue<string>(i, 27),
                    Manufacturer = tab.GetValue<string>(i, 26),
                    Price = ParsePrice(tab.GetValue<string>(i, 13)),
                    Currency = Currency.RUB
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
