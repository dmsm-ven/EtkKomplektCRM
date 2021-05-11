using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("4EA34EEA-5407-4807-8E33-D8A8FA71ECBA")]
    public class _1CPriceListTemplate : ExcelPriceListTemplateBase
    {
        const int START_ROW_NUMBER = 3;

        public _1CPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1).Trim('.', ' ');
                string name = tab.GetValue<string>(row, 4);
                int quantity = tab.GetValue<int>(row, 11);

                if (string.IsNullOrWhiteSpace(skuNumber)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    IsSpecialLine = true,
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            // Для Testo удаляем пробелы а моделях
            list.Where(row => row.Name.Contains("testo")).ToList().ForEach(p => p.Model = p.Model.Replace(" ", string.Empty));

            return list;
        }
    }

    [PriceListTemplateGuid("B048D3E6-D8D1-4867-944B-6D5D3A6D4396")]
    public class _1CHtmlPriceListTemplate : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get; }

        public _1CHtmlPriceListTemplate(string fileName)
        {
            FileName = fileName;
            ManufacturerNameMap["Proskit"] = "Pro'sKit";
            ManufacturerNameMap["АКТАКОМ"] = "Aktakom";
        }

        public Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(FileName));

            var table = doc.DocumentNode.SelectNodes(".//table").LastOrDefault();

            if(table != null)
            {
                var data = table
                    .SelectNodes(".//tr")
                    .Skip(4)
                    .Select(tr => tr.SelectNodes("./td").Select(td => HttpUtility.HtmlDecode(td.InnerText.Trim())).ToArray())
                    .Select(cells => new PriceLine(this)
                    {
                        Sku = cells[0],
                        Manufacturer = MapManufacturerName(cells[1]),
                        Name = cells[2],
                        Quantity = ParseQuantity(cells[4].Replace(",000", string.Empty)),
                        StockPartner = StockPartner._1C
                    })
                    .Where(pl => !string.IsNullOrWhiteSpace(pl.Sku))
                    .ToList();

                list.AddRange(data);
            }


            return Task.FromResult(list);
        }
    }
}
