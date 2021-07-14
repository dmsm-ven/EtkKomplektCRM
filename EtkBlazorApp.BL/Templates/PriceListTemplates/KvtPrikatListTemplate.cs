using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("0E5D0616-0FC0-422E-96FA-1A512D9A675A")]
    public class TexElektroPriceListTemplate : ExcelPriceListTemplateBase
    {
        public TexElektroPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);

                if (!name.Contains("КВТ")) { continue; }

                string code = "KV-" + tab.GetValue<string>(row, 2);
                string ean = tab.GetValue<string>(row, 3);

                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 5));
                var price = ParsePrice(tab.GetValue<string>(row, 6));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "КВТ",
                    Sku = code,
                    Model = code,
                    Ean = ean,
                    Price = price,
                    Quantity = quantitySpb,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.TexElektro
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

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
                string ean =  tab.GetValue<string>(row, 3);

                var quantityKaluga = ParseQuantity(tab.GetValue<string>(row, 4));
                var quantitySpb =  ParseQuantity(tab.GetValue<string>(row, 5));
                var price =  ParsePrice(tab.GetValue<string>(row, 6));

                

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
}