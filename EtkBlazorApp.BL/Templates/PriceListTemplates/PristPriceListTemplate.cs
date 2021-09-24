using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EtkBlazorApp.BL
{
    //TODO тут нужно разбить класс, не должно быть что шаблон считывает данные
    [PriceListTemplateGuid("438B5182-62DD-42C4-846F-4901C3B38B14")]
    public class PristPriceListTemplate : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get; private set; }

        readonly string[] ONLY_QUANTITY_BRANDS = new[] { "ERSA" };

        public PristPriceListTemplate(string uri)
        {
            FileName = uri;
            ManufacturerNameMap["Teledyne LeCroy"] = "LeCroy";
            ManufacturerNameMap["Keysight Technologies"] = "Keysight";

            SkipManufacturerNames.AddRange(new[] { "TDK-Lambda", "Weller", "ProsKit", "Bernstein", "Testo", "Viking" });
        }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var loader = new PristXmlReader();
            var offers = await loader.LoadPristProducts(FileName);

            var list = new List<PriceLine>();

            foreach (var offer in offers)
            {
                string manufacturer = MapManufacturerName(offer.Vendor);

                if (SkipManufacturerNames.Contains(manufacturer)) { continue; }

                decimal? price = ONLY_QUANTITY_BRANDS.Contains(manufacturer, StringComparer.OrdinalIgnoreCase) ? null : offer.Price;

                var priceLine = new PriceLine(this)
                {
                    Name = offer.Name,
                    Currency = offer.Currency,
                    Manufacturer = manufacturer,
                    Model = offer.Model,
                    Sku = offer.Model,
                    Price = price,
                    Quantity = offer.Amount,
                    Stock = StockName.Prist
                };

                list.Add(priceLine);
            }

            return list;
        }

        private class PristXmlReader
        {
            public async Task<List<PristOffer>> LoadPristProducts(string fileUri)
            {
                var file = new XmlDocument();

                await Task.Run(() => file.Load(fileUri));

                var list = new List<PristOffer>();

                var offers = file
                    .SelectNodes("yml_catalog/shop/offers/offer")
                    .OfType<XmlNode>()
                    .ToList();

                foreach (var node in offers)
                {
                    var offer = new PristOffer();
                    string currentString = node["currencyId"].InnerText;

                    try
                    {
                        offer.OfferId = Convert.ToInt32(node.Attributes["id"].Value);
                        offer.IsAvailable = Convert.ToBoolean(node.Attributes["available"].Value);
                        offer.Model = node["name"].InnerText;
                        offer.Price = decimal.Parse(node["price"].InnerText, new CultureInfo("en-EN"));
                        offer.Name = node["description"].InnerText;
                        offer.Url = node["url"].InnerText;
                        offer.Vendor = node["vendor"].InnerText;

                        if (node.SelectSingleNode("./param[@name='Доступные остатки']") != null)
                        {
                            offer.Amount = (int)decimal.Parse(node.SelectSingleNode("./param[@name='Доступные остатки']").InnerText, new CultureInfo("en-EN"));
                        }
                     
                        if (!string.IsNullOrEmpty(currentString))
                        {
                            offer.Currency = (CurrencyType)Enum.Parse(typeof(CurrencyType), currentString);
                        }
                        else if (offer.Price == 0)
                        {
                            offer.Currency = CurrencyType.RUB;
                        }
                        else
                        {
                            continue;
                        }

                    }
                    catch
                    {
                        throw;
                    }

                    list.Add(offer);
                }


                return list;
            }
        }

        private class PristOffer
        {
            public int OfferId { get; set; }
            public bool IsAvailable { get; set; }
            public int Amount { get; set; }
            public string Url { get; set; }
            public decimal Price { get; set; }
            public CurrencyType Currency { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; } // поле name
            public string Name { get; set; } // поле description

            //<offer id = "209376" available="true">
            //<amount>0</amount>
            //<url>https://prist.ru/catalog/izmeriteli_rlc_laboratornye/e7_28/?r1=yandext&amp;r2=</url>
            //<price>185850</price>
            //<currencyId>RUB</currencyId>
            //<categoryId>3179</categoryId>
            //<picture>https://prist.ru/upload/iblock/f46/f469998a8fd6eb1c4d7cc56dc8962cdb.jpg</picture>
            //<vendor>МНИПИ</vendor>
            //<name>E7-28</name>
            //<description>Переходник АКИП-21.135</description>
        }
    }
}
