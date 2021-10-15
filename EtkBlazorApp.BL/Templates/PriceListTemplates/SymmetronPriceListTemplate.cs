using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL
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
                string sku = tab.GetValue<string>(row, 3);
                string name = tab.GetValue<string>(row, 4);             
                int quantity = tab.GetValue<int>(row, 10);
                decimal? priceInCurrency = ParsePrice(tab.GetValue<string>(row, 11));                
                decimal? priceInRub = ParsePrice(tab.GetValue<string>(row, 13));
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 26));
                string model = tab.GetValue<string>(row, 27);

                CurrencyType priceCurreny = CurrencyType.RUB;
                if(Enum.TryParse<CurrencyType>(tab.GetValue<string>(row, 12), out var parsedCurrency))
                {
                    priceCurreny = parsedCurrency;
                }

                if (SkipManufacturerNames.Contains(manufacturer)) { continue; }
                          
                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Sku = sku,
                    Model = model,
                    Manufacturer = manufacturer,
                    Price = (priceInCurrency.HasValue && priceCurreny != CurrencyType.RUB) ? priceInCurrency : priceInRub,                   
                    Currency = priceCurreny,
                    Quantity = quantity,
                    Stock = StockName.Symmetron
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
