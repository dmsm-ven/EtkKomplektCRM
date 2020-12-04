using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EtkBlazorApp.BL
{
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        public override List<PriceLine> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(FileName)))
            {
                var tab = package.Workbook.Worksheets[0];
                for (int i = 2; i < tab.Dimension.Rows; i++)
                {
                    if(token.HasValue && (i % 100 == 0) && token.Value.IsCancellationRequested)
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
            }

            return list;
        }
    }
}
