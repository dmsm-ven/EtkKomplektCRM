using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class ExcelPriceListTemplateBase : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get; }

        protected CancellationToken? CancelToken { get; set; }
        protected ExcelPackage Excel { get; set; }

        public ExcelPriceListTemplateBase(string fileName) { FileName = fileName; }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string convertedFilePath = null;
            List<PriceLine> lines = null;
            try
            {
                //Сначала пытаемся просто загрузить, т.к. даже некоторые .xls файлы можно загрузить без конвертации в .xlsx
                lines = await Read(FileName);
            }
            catch
            {
                //Если не получилось загрузить xls или в файле xls 0 вкладок - делаем конвертацию
                try
                {
                    convertedFilePath = await ConvertXlsToXlsx(FileName);
                    lines = await Read(convertedFilePath);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (File.Exists(convertedFilePath))
                    {
                        File.Delete(convertedFilePath);
                    }
                }               
            }
            return lines;
        }

        private async Task<List<PriceLine>> Read(string fileName, CancellationToken? token = null)
        {
            using (Excel = new ExcelPackage(new FileInfo(fileName)))
            {
                if(Excel.Workbook.Worksheets.Count == 0)
                {
                    throw new FormatException("Прайс-лист не содержит вкладок");
                }
                var readedLines = await Task.Run(() => ReadDataFromExcel());
                return readedLines;
            }
        }

        /// <summary>
        /// Конвертация с помощью C# много зависимостей. Делаем конвертацию с помощью Python
        /// </summary>
        protected abstract List<PriceLine> ReadDataFromExcel();

        protected ExcelWorksheet tab => Excel.Workbook.Worksheets[0];

        private async Task<string> ConvertXlsToXlsx(string inputFilePath)
        {
            string scriptPath = Path.Combine(Environment.CurrentDirectory, "wwwroot/script/xls2xlsx_convert.py");
            string arguments = $"\"{scriptPath}\" \"{inputFilePath}\"";

            ProcessStartInfo command = new ProcessStartInfo();
            command.FileName = "python3";
            command.Arguments = arguments;
            command.UseShellExecute = false;
            command.CreateNoWindow = true;

            using (var process = Process.Start(command))
            {
                await process.WaitForExitAsync();              
            }
            await Task.Delay(TimeSpan.FromSeconds(2));
            return inputFilePath + "x";
        }
    }
}
