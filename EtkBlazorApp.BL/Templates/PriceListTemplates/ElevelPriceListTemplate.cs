using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    //Т.к. в прайс-листе нет столбца с названием бренда то будут загражаются все
    [PriceListTemplateGuid("BE7AC7E1-DC6F-4419-A07E-373599222170")]
    public class ElevelPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ElevelPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            int stock_spb_self_index = -1;
            int stock_spb_producer_index = -1;

            for (int column = 1; column <= tab.Dimension.Columns; column++)
            {
                if (tab.GetValue<string>(1, column).Equals("Санкт-Петербург")) { stock_spb_self_index = column; }
                if (tab.GetValue<string>(1, column).Equals("Производитель СПБ")) { stock_spb_producer_index = column; }
            }

            for (int row = 3; row < tab.Dimension.Rows; row++)
            {
                string model = tab.GetValue<string>(row, 1).Trim();
                string sku = tab.GetValue<string>(row, 2).Trim();
                var price = ParsePrice(tab.GetValue<string>(row, 3));
                CurrencyType currency = Enum.Parse<CurrencyType>(tab.GetValue<string>(row, 4));

                var quantitySpbSelf = ParseQuantity(tab.GetValue<string>(row, stock_spb_self_index));
                var quantitySpbProducer = ParseQuantity(tab.GetValue<string>(row, stock_spb_producer_index));

                var priceLine = new MultistockPriceLine(this)
                {
                    Quantity = quantitySpbSelf,
                    Sku = model,
                    Model = sku,
                    Manufacturer = "Elevel",
                    Price = price,
                    Currency = currency,
                    Stock = StockName.ElevelSbp_self
                };

                priceLine.AdditionalStockQuantity[StockName.ElevelSbp_producer] = quantitySpbProducer.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }
}
