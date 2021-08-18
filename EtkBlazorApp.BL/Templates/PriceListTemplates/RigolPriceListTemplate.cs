using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("8C4A3CF3-AF22-48F4-8EBD-9C232273F507")]
    public class RigolPriceListTemplate : CsvPriceListTemplateBase
    {
        public RigolPriceListTemplate(string fileName) : base(fileName) { }

        public override async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var list = new List<PriceLine>();

            var lines = await ReadCsvLines(';', Encoding.UTF8);

            foreach (var row in lines.Skip(1))
            {
                string sku = row[0];
                string name = row[3];
                var quantity = ParseQuantity(row[4]);
                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Rigol",
                    Stock = StockName.UnitServis,
                    Name = name,
                    Sku = sku,
                    Model = sku,
                    Quantity = quantity
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
