using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("934C755F-55DC-4BBD-AE7D-C79A0364E060")]
    public class ProxonPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ProxonPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string fullName = tab.GetValue<string>(row, 0).Trim();
        
                var match = Regex.Match(fullName, @"(Proxxon \d+)\. (.*?)$");

                if (!match.Success) { continue; }

                string model = match.Groups[1].Value;
                decimal rrcPrice = tab.GetValue<decimal>(row, 3);

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Proxxon",
                    Model = model,
                    Name = fullName,
                    Sku = model,
                    Price = rrcPrice
                };
                list.Add(priceLine);
            }

            return list;
        }

    }

    [PriceListTemplateGuid("641779CC-C6F8-4CFB-9ABA-2C8136BF19FB")]
    public class ProxonDealerPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ProxonDealerPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 7; row < tab.Dimension.Rows; row++)
            {
                string sku = "PX " + tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);
                string ean = tab.GetValue<string>(row, 4);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 11), false, 4);

                var priceLine = new PriceLine(this)
                {
                    Currency = CurrencyType.EUR,
                    Manufacturer = "Proxxon",
                    Model = sku,
                    Sku = sku,
                    Ean = ean,
                    Name = name,
                    Price = price
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
