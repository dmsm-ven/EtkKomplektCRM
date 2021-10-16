using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("58F658D6-483C-4EE6-9E23-9932514624CF")]
    public class RgkPriceListTemplate : ExcelPriceListTemplateBase
    {
        public RgkPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 8; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 4));

                if(ManufacturerSkipCheck(manufacturer)) { continue; }

                string sku = tab.GetValue<string>(row, 6);

                decimal? priceRrc = ParsePrice(tab.GetValue<string>(row, 8));
                decimal? priceOpt = ParsePrice(tab.GetValue<string>(row, 9));
                //TODO: тут игнорируется стандартный механизм добавления скидок
                decimal price = manufacturer.Equals("Fluke") ? Math.Floor(priceOpt.Value * 1.2m) : priceRrc.Value;

                int? quantity = ParseQuantity(tab.GetValue<string>(row, 16));

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Manufacturer = manufacturer,
                    Model = sku,
                    Sku = sku,
                    Price = price,
                    Quantity = quantity,
                    //Stock = StockName.RGK
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

}
