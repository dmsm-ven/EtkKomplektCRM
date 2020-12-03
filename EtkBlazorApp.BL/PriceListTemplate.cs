using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtkBlazorApp.BL
{
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        public override List<PriceLine> ReadPriceLines()
        {
            var list = new List<PriceLine>();

            for(int row = 0; row < Workbook.Rows; row++)
            {
                list.Add(new PriceLine()
                {
                    Name = Workbook[row, 0].Value.ToString()
                });
            }

            return list;
        }
    }

    public abstract class ExcelPriceListTemplateBase : IPriceListTemplate
    {
        public string FileName { get; set; }

        protected ExcelRange Workbook => new EPPlusExcelReader().ReadDataFromFile(FileName);

        public abstract List<PriceLine> ReadPriceLines();
    }

    internal class EPPlusExcelReader
    {
        public ExcelRange ReadDataFromFile(string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                return package.Workbook.Worksheets[0].Cells;
            }
        }
    }

    public interface IPriceListTemplate
    {
        List<PriceLine> ReadPriceLines();
        string FileName { get; set; }
    }

    public class PriceLine
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public Currency Currency { get; set; }
    }
}
