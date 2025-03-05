﻿using EtkBlazorApp.BL.Templates.PriceListTemplates.Base;
using EtkBlazorApp.Core.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("438B5182-62DD-42C4-846F-4901C3B38B14")]
    public class PristPriceListTemplate : PriceListTemplateReaderBase, IPriceListTemplate
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        public string FileName { get; private set; }

        public PristPriceListTemplate(string uri)
        {
            FileName = uri;

        }

        public async Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null)
        {
            var loader = new PristXmlReader();
            var offers = await Task.Run(() => loader.LoadPristProducts(FileName));

            FixOffers(offers);

            var list = new List<PriceLine>();

            foreach (var offer in offers)
            {
                string manufacturer = MapManufacturerName(offer.Vendor);
                if (SkipThisBrand(manufacturer)) { continue; }

                decimal? price = offer.OldPrice ?? offer.Price;

                var priceLine = new PriceLine(this)
                {
                    Name = offer.Name,
                    Currency = offer.Currency,
                    Manufacturer = manufacturer,
                    Model = offer.Model.Trim(),
                    Sku = offer.Model.Trim(),
                    Price = price,
                    Quantity = offer.Amount,
                    //Stock = StockName.Prist
                };

                list.Add(priceLine);
            }

            return list;
        }

        private void FixOffers(List<PristOffer> offers)
        {
            var offerWithError = offers
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.StartsWith("Источник питания GPP-"))
                .ToList();

            if (offerWithError.Count == 0)
            {
                return;
            }

            foreach (var offer in offerWithError)
            {
                if (offer.Name.EndsWith(" (GPIB)") && !offer.Model.EndsWith(" (GPIB)"))
                {
                    offer.Model = $"{offer.Model} (GPIB)";
                }
            }

            var dups = offers
                .GroupBy(o => o.Model)
                .Where(g => g.Count() > 1);

            if (dups.Count() > 0)
            {
                string dupModels = string.Join(";", dups.Select(g => g.Key ?? "<empty model>"));
                nlog.Warn("Обнаружены дубли по моделям в прайс-листе. Следующие модели имеются в 2х или более офферах: {dupModels}", dupModels);
            }

        }

        private class PristXmlReader
        {
            public List<PristOffer> LoadPristProducts(string fileUri)
            {
                var file = new XmlDocument();
                file.Load(fileUri);

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
                        if (node["oldprice"] != null)
                        {
                            offer.OldPrice = decimal.Parse(node["oldprice"].InnerText, new CultureInfo("en-EN"));
                        }
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

                RenameInvalidModels(list);

                return list;
            }

            private void RenameInvalidModels(List<PristOffer> list)
            {
                KeyValuePair<string, string> invalidProductData = new("003B", "0003B");

                var invalidProduct = list.FirstOrDefault(p => p.Model == invalidProductData.Key);

                if (invalidProduct != null)
                {
                    invalidProduct.Model = invalidProductData.Value;
                }
            }
        }

        private class PristOffer
        {
            public int OfferId { get; set; }
            public bool IsAvailable { get; set; }
            public int Amount { get; set; }
            public string Url { get; set; }
            public decimal Price { get; set; }
            public decimal? OldPrice { get; set; }
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
