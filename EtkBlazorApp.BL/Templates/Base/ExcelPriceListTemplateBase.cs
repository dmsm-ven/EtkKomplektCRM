using EtkBlazorApp.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class ExcelPriceListTemplateBase : IPriceListTemplate
    {
        public string FileName { get; set; }

        protected CancellationToken? CancelToken { get; set; }
        protected ExcelPackage Excel { get; set; }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<PriceLine> lines = null;

            await Task.Run(() =>
            {
                using (Excel = new ExcelPackage(new FileInfo(FileName)))
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Отменено пользователем");
                    }

                    lines = ReadDataFromExcel();
                }
            });

            return lines;
        }

        protected abstract List<PriceLine> ReadDataFromExcel();
    
        protected virtual decimal? ParsePrice(string str)
        {
            if(!string.IsNullOrWhiteSpace(str))
            {
                if( decimal.TryParse(str.Replace(",", ".").Replace(" ", string.Empty), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
                {
                    return price;
                }
            }

            return null;
        }
    }
}
