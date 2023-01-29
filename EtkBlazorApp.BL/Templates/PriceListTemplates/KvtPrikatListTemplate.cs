﻿using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("F2272C31-48B7-4350-9C14-9CA44F542E1B")]
    public class KvtSuPriceListTemplate : ExcelPriceListTemplateBase
    {
        public KvtSuPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string sku = "KV-" + tab.GetValue<string>(row, 2);
                string model = "KVT-" + tab.GetValue<string>(row, 2);
                string ean = tab.GetValue<string>(row, 3);

                var quantityKaluga = ParseQuantity(tab.GetValue<string>(row, 4));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 5));
                var price = ParsePrice(tab.GetValue<string>(row, 6));

                var priceLine = new MultistockPriceLine(this)
                {
                    Currency = CurrencyType.RUB,
                    Price = price,
                    Quantity = quantitySpb,
                    Ean = ean,
                    Sku = sku,
                    Model = model,
                    Name = name,
                    Manufacturer = "КВТ",
                    Stock = StockName.KvtSu_Spb
                };

                priceLine.AdditionalStockQuantity[StockName.KvtSu_Kaluga] = quantityKaluga.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("040BBB29-51D7-44AD-876C-B1ECC65936AA")]
    public class TexElektroPriceListTemplate : ExcelPriceListTemplateBase
    {
        public TexElektroPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string sku = "KV-" + tab.GetValue<string>(row, 2);

                var quantityKaluga = ParseQuantity(tab.GetValue<string>(row, 4));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 5));

                var price = ParsePrice(tab.GetValue<string>(row, 6));

                var priceLine = new MultistockPriceLine(this)
                {
                    Quantity = quantitySpb,
                    Stock = StockName.TexElektro,
                    Sku = sku,
                    Model = sku,
                    Name = name,
                    Price = price,
                    Manufacturer = "КВТ",
                };

                priceLine.AdditionalStockQuantity[StockName.TexElektro_Kaluga] = quantityKaluga.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }
}