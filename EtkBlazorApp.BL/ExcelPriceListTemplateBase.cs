using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace EtkBlazorApp.BL
{
    public abstract class ExcelPriceListTemplateBase : IPriceListTemplate
    {
        public string FileName { get; set; }

        protected virtual ExcelWorkbook GetWorkbook()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorkbook book;
            using (var package = new ExcelPackage(new FileInfo(FileName)))
            {
                book = package.Workbook;
            }

            return book;
        }

        public abstract List<PriceLine> ReadPriceLines();
    
        protected decimal? ParsePrice(string str)
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
