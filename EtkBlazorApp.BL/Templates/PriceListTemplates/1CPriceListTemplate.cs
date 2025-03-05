using EtkBlazorApp.BL.Templates.PriceListTemplates.Base;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const int START_ROW_NUMBER = 7;

        public _1CPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 1));
                string skuNumber = tab.GetValue<string>(row, 3);
                string name = tab.GetValue<string>(row, 4);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 7));

                if (string.IsNullOrWhiteSpace(skuNumber) || SkipThisBrand(manufacturer))
                {
                    continue;
                }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity,
                    Manufacturer = manufacturer,
                    //Stock = StockName._1C
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("B048D3E6-D8D1-4867-944B-6D5D3A6D4396")]
    public class _1CHtmlPriceListTemplate : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get; }

        private static readonly IReadOnlyDictionary<string, string> BrandPrefixMap;

        static _1CHtmlPriceListTemplate()
        {
            BrandPrefixMap = new Dictionary<string, string>()
            {
                ["КВТ"] = KvtSuPriceListTemplate.MODEL_PREFIX
            };
        }

        public _1CHtmlPriceListTemplate(string fileName)
        {
            FileName = fileName;
        }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var doc = new HtmlDocument();
            doc.LoadHtml(await File.ReadAllTextAsync(FileName));

            var table = doc.DocumentNode.SelectNodes(".//table").LastOrDefault();

            if (table != null)
            {
                // содержит дату следующей поставки
                DateTime[] nextDeliveryDays = table.SelectSingleNode(".//tr")
                    .SelectNodes(".//td")
                    .Skip(2)
                    .Where(cell => !string.IsNullOrWhiteSpace(cell.InnerText))
                    .Select(cell => DateTime.Parse(cell.InnerText.Replace(" г.", string.Empty), new CultureInfo("ru-RU")))
                    .ToArray();

                var data = table
                    .SelectNodes(".//tr")
                    .Skip(3)
                    .Select(tr => tr.SelectNodes("./td").Select(td => HttpUtility.HtmlDecode(td.InnerText.Trim())).ToArray())
                    .Where(cells => cells.Length >= 5 && cells[0] != "Итого")
                    .Select(cells => ParseRow(cells, nextDeliveryDays))
                    .Where(pl => !string.IsNullOrWhiteSpace(pl.Sku) && !BrandsBlackList.Contains(pl.Manufacturer))
                    .ToList();

                list.AddRange(data);
            }


            return list;
        }

        private PriceLineWithNextDeliveryDate ParseRow(string[] cells, DateTime[] headerColumnsDays)
        {
            var line = new PriceLineWithNextDeliveryDate(this)
            {
                Manufacturer = MapManufacturerName(cells[0]),
                Sku = cells[1],
                Model = cells[1],
                Name = cells[2],
                Quantity = ParseQuantity(cells[4].Replace(",000", string.Empty)),
                //Stock = StockName._1C,
            };

            //Некоторые бренды имеют префиксы, иначе товар не подхватиться
            if (BrandPrefixMap.ContainsKey(line.Manufacturer))
            {
                if (!line.Sku.StartsWith(BrandPrefixMap[line.Manufacturer]))
                {
                    line.Sku = $"{BrandPrefixMap[line.Manufacturer]}{cells[1]}";
                }
            }

            for (int i = 0; i < headerColumnsDays.Length; i++)
            {
                int? nextQuantity = ParseQuantity(cells[5 + i].Replace(",000", string.Empty), canBeNull: true);
                //берем первый попавшийся столбец (ближайший), остальные отбрасываем
                if (nextQuantity.HasValue)
                {
                    line.NextStockDelivery = new DataAccess.NextStockDelivery()
                    {
                        Date = headerColumnsDays[i],
                        Quantity = nextQuantity.Value
                    };
                    break;
                }
            }

            return line;
        }
    }
}
