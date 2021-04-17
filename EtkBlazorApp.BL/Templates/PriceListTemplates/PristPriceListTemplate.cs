using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("438B5182-62DD-42C4-846F-4901C3B38B14")]
    public class PristPriceListTemplate : IPriceListTemplate
    {
        public string FileName { get; private set; }

        readonly string[] INVALID_BRANDS = new[] { "ERSA", "TDK-Lambda", "Weller", "ProsKit", "Bernstein"  };

        readonly Dictionary<string, string> BrandMap = new Dictionary<string, string>()
        {
            ["Teledyne LeCroy"] = "LeCroy",
            ["Keysight Technologies"] = "Keysight"
        };

        public PristPriceListTemplate(string uri)
        {
            FileName = uri;
        }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var loader = new PristXmlReader();
            var offers = await loader.LoadPristProducts(FileName);

            var list = new List<PriceLine>();

            foreach (var offer in offers.Where(offer => !INVALID_BRANDS.Contains(offer.Vendor)))
            {
                var priceLine = new PriceLine(this)
                {
                    Name = offer.Name,
                    Currency = offer.Currency,
                    Manufacturer = BrandMap.ContainsKey(offer.Vendor) ? BrandMap[offer.Vendor] : offer.Vendor,
                    Model = offer.Model,
                    Sku = offer.Model,
                    Price = offer.Price,
                    Quantity = offer.Amount
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
