using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA")]
    public class MegeonPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MegeonPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 3));
                if (SkipThisBrand(manufacturer)) { continue; }

                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 4));
                decimal price = tab.GetValue<decimal>(row, 5);
                CurrencyType currency = CurrencyType.RUB;
                if(Enum.TryParse(tab.GetValue<string>(row, 7)?.Replace("руб.", "RUB"), true, out currency)) { }

                var match1 = Regex.Match(name, @"^(.*?) (МЕГЕОН \S+)");
                var match2 = Regex.Match(name, @"МЕГЕОН \S+");
                string model = match1.Success ? match1.Groups[2].Value : (match2.Success ? match2.Value : null);

                var priceLine = new PriceLine(this)
                {
                    Currency = currency,
                    Manufacturer = manufacturer,
                    Model = model,
                    Sku = sku,
                    Price = price,
                    Quantity = quantity,
                    Name = name,
                    //Stock = StockName.Megeon
                };
                list.Add(priceLine);              
            }

            return list;
        }
    }

    [PriceListTemplateGuid("A9BF1E82-09C6-4A28-A7BB-99BF1D6FF695")]
    public class MegeonXmlPriceListTemplate : PriceListTemplateReaderBase, IPriceListTemplate
    {
        private readonly string fileName;

        public MegeonXmlPriceListTemplate(string fileName)
        {
            this.fileName = fileName;
        }

        public string FileName => fileName;

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var doc = XDocument.Load(fileName);
            var offers = doc.Descendants("shop").Elements("offers").Elements("offer");

            var list = new List<PriceLine>();
            foreach (var offer in offers)
            {
                if (offer.Element("vendor").Value != "МЕГЕОН") { continue; }

                int? quantity = ParseQuantity(offer.Element("outlets").Element("outlet")?.Attribute("instock").Value ?? "0");

                list.Add(new PriceLine(this)
                {
                    Price = ParsePrice(offer.Element("price").Value),
                    Currency = CurrencyType.RUB,
                    Name = offer.Element("name").Value,
                    Model = offer.Element("model").Value,
                    Sku = offer.Attribute("id").Value,
                    Manufacturer = "Мегеон",
                    Quantity = quantity
                });
                
            }
            return list;
        }
    }
}
