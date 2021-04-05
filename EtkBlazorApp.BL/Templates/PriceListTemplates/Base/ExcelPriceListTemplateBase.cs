using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{

    public abstract class ExcelPriceListTemplateBase : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get;  }

        protected CancellationToken? CancelToken { get; set; }
        protected ExcelPackage Excel { get; set; }

        public ExcelPriceListTemplateBase(string fileName) { FileName = fileName; }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<PriceLine> lines = new List<PriceLine>();

            await Task.Run(() =>
            {
                using (Excel = new ExcelPackage(new FileInfo(FileName)))
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Отменено пользователем");
                    }

                    var readedLines = ReadDataFromExcel();
                    lines.AddRange(readedLines.Where(line => line.Price.HasValue || line.Quantity.HasValue));
                }
            });

            return lines;
        }

        protected abstract List<PriceLine> ReadDataFromExcel();
    }
}
