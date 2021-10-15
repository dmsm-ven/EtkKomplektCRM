using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace EtkBlazorApp.BL.Templates
{
    [PriceListTemplateGuid("B6B23CAF-0D0C-416F-AB96-C7FD42FD0DED")]
    public class HantekPriceListTemplate : CsvPriceListTemplateBase
    {
        public HantekPriceListTemplate(string fileName) : base(fileName) { }

        public override async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var lines = await ReadCsvLines(',', Encoding.GetEncoding("windows-1251"));

            foreach (var row in lines.Skip(1))
            {
                if(row.Length != 5) { continue; }

                string model = row[0].Trim();
                string name = row[1].Trim();
                var price = ParsePrice(row[2]);
                var quantity = ParseQuantity(row[4]);

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.Hantek,
                    Model = model,
                    Sku = model,
                    Price = price,
                    Quantity = quantity,
                    Manufacturer = "Hantek"
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
