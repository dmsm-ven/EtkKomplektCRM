using EtkBlazorApp.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("D31FFE97-53BC-41DC-9D54-43FABBC51BCD")]
    public class AktakomPriceListTemplate : CsvPriceListTemplateBase
    {
        public AktakomPriceListTemplate(string fileName) : base(fileName) { }

        public override async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var lines = await ReadCsvLines('\t', Encoding.GetEncoding("windows-1251"));

            foreach (var row in lines.Skip(1))
            {
                string name = row[1].Trim();
                string sku = row[2].Trim();
                string manufacturer = MapManufacturerName(row[3].Trim());
                var price = ParsePrice(row[21].Trim(), true, 0);
                var quantity = ParseQuantity(row[20].Trim(), true);

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Model = sku,
                    Sku = sku,
                    Price = price,
                    Quantity = quantity,
                    Manufacturer = manufacturer
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
