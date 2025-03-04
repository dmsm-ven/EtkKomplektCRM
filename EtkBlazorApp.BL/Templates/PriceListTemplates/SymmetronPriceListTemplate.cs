using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("3853B988-DB37-4B6E-861F-3000B643FAC4")]
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SymmetronPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 26));
                if (SkipThisBrand(manufacturer)) { continue; }

                string sku = tab.GetValue<string>(row, 3);
                string name = tab.GetValue<string>(row, 4);
                int quantity = tab.GetValue<int>(row, 10);
                decimal? priceInCurrency = ParsePrice(tab.GetValue<string>(row, 11));
                decimal? priceInRub = ParsePrice(tab.GetValue<string>(row, 13));

                string model = tab.GetValue<string>(row, 27);

                CurrencyType priceCurreny = CurrencyType.RUB;
                if (Enum.TryParse<CurrencyType>(tab.GetValue<string>(row, 12), out var parsedCurrency))
                {
                    priceCurreny = parsedCurrency;
                }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Sku = sku,
                    Model = model,
                    Manufacturer = manufacturer,
                    Price = priceInCurrency.HasValue && priceCurreny != CurrencyType.RUB ? priceInCurrency : priceInRub,
                    Currency = priceCurreny,
                    Quantity = quantity,
                    //Stock = StockName.Symmetron
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("51188220-54E6-4973-BCE9-7DE618829C5C")]
    public class SymmetronProskitPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SymmetronProskitPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string model = tab.GetValue<string>(row, 3);
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 4));

                //TODO: тут хак, стоит доработать parsequantity (вместо использования ParsePrice) метод что бы он корректно обрабатывал
                int quantity = (int)Math.Ceiling(ParsePrice(tab.GetValue<string>(row, 5)) ?? 0);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 9));

                if (SkipThisBrand(manufacturer))
                {
                    continue;
                }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Sku = sku,
                    Model = model,
                    Manufacturer = manufacturer,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Quantity = quantity,
                };

                list.Add(priceLine);
            }

            FixSkuDuplicate(list);

            return list;
        }

        //TODO: доделать в функционал, убрать костыль
        //Есть товары которые у разных поставщиков находятся по одним и темже кодом (их личный код, разный у каждого поставщика)
        //Надо подумать как сделать функционал проверки копий, скорее всего после чтения шаблона нужно проверять в отдельной таблице,
        private void FixSkuDuplicate(List<PriceLine> list)
        {
            var bugProgduct = list.FirstOrDefault(i => i.Sku == "00030823");
            if (bugProgduct != null)
            {
                bugProgduct.Sku = "";
                bugProgduct.Model = "0212WD";
            }
        }
    }
}
