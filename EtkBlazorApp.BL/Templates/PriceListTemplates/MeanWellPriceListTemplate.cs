using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("3D41DDC2-BB5C-4D6A-8129-C486BD953A3D")]
    public class MeanWellPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MeanWellPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.Cells[row, 0].ToString();
                string skuNumber = tab.Cells[row, 1]?.ToString();
                string quantityString = tab.Cells[row, 5]?.ToString();

                string regularPriceString = tab.Cells[row, 6]?.ToString();
                string discountPriceString = tab.Cells[row, 7]?.ToString();

                string priceString = new string[] { regularPriceString, discountPriceString }
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

                if (manufacturer.Equals("MeanWell", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Quantity = ParseQuantity(quantityString),
                        Currency = CurrencyType.USD,
                        Price = ParsePrice(priceString),
                        Manufacturer = "Mean Well",
                        Sku = skuNumber,
                        Model = skuNumber
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("2231E716-3643-4EDE-B6D0-764DB3B4DF68")]
    public class MeanWellPartnerPriceListTemplate : CsvPriceListTemplateBase
    {
        public MeanWellPartnerPriceListTemplate(string fileName) : base(fileName) { }

        public override async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var data = (await ReadCsvLines())
                .Select(row => new
                {
                    Sku = row[0].ToString(),
                    Model = row[1].ToString(),
                    Price = row[7].ToString(),
                    PartSize = row[5].ToString()
                })
                .GroupBy(row => row.Sku)
                .Select(g => g.OrderBy(a => a.PartSize).First())
                .ToList();

            foreach (var row in data)
            {
                if (row.PartSize != "1") { continue; }            
                
                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.USD,
                    Price = ParsePrice(row.Price),
                    Manufacturer = "Mean Well",
                    Sku = row.Sku,
                    Model = row.Model
                };
                list.Add(priceLine);

            }

            return list;
        }
    }
}
