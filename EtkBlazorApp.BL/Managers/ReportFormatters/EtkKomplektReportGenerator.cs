using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL
{
    public class EtkKomplektReportGenerator
    {
        public class EtkKomplektPriceListExportOptions
        {
            public bool ExcludeEmptyPrice { get; init; }
            public bool ExcludeEmptyQuantity { get; init; }
            public List<int> AllowedManufacturers { get; init; }
        }
    
        private readonly IProductStorage productStorage;

        public EtkKomplektReportGenerator(IProductStorage productStorage)
        {
            this.productStorage = productStorage;
        }

        public async Task<string> Create(EtkKomplektPriceListExportOptions options = null)
        {
            var products = (await productStorage.ReadProducts(options?.AllowedManufacturers))
                .OrderBy(p => p.manufacturer)
                .ThenBy(p => p.name)
                .ToList();

            string fileName = Path.GetTempPath() + $"etk-komplekt_{DateTime.Now.ToShortDateString().Replace(".", "_")}.xlsx";

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var workSheet = package.Workbook.Worksheets.Add("ЕТК-Комплект");

                AppendHeader(workSheet);
                AppendBody(products, workSheet);

                await package.SaveAsync();
            }

            return fileName;
        }

        private static void AppendBody(IEnumerable<ProductEntity> products, ExcelWorksheet workSheet)
        {
            int r = 2;
            foreach (var product in products)
            {
                workSheet.Cells[r, 1].Value = product.manufacturer;
                workSheet.Cells[r, 2].Value = HttpUtility.HtmlDecode(product.model);
                workSheet.Cells[r, 3].Value = HttpUtility.HtmlDecode(product.sku);
                workSheet.Cells[r, 4].Value = HttpUtility.HtmlDecode(product.name);
                workSheet.Cells[r, 5].Value = product.quantity;
                workSheet.Cells[r, 6].Value = product.price;

                if (product.base_price != decimal.Zero && product.base_currency_code != "RUB")
                {
                    workSheet.Cells[r, 7].Value = product.base_price.ToString("F2");
                    workSheet.Cells[r, 8].Value = product.base_currency_code;
                }

                r++;
            }
        }

        private static void AppendHeader(ExcelWorksheet workSheet)
        {
            workSheet.Cells[1, 1].Value = "Производитель";
            workSheet.Cells[1, 2].Value = "Модель";
            workSheet.Cells[1, 3].Value = "Артикул";
            workSheet.Cells[1, 4].Value = "Наименование";
            workSheet.Cells[1, 5].Value = "Количество";
            workSheet.Cells[1, 6].Value = "Цена";
            workSheet.Cells[1, 7].Value = "Цена в валюте";
            workSheet.Cells[1, 8].Value = "Валюта";
            var headerStyle = workSheet.Cells["A1:H1"].Style;
            headerStyle.Font.Size = 12;
            headerStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            headerStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            headerStyle.Font.Bold = true;

            workSheet.Column(1).AutoFit();
            workSheet.Column(2).Width = 15;
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).Width = 60;
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();
        }
    }
}
