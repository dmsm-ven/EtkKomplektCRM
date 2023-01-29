using EtkBlazorApp.BL.Templates;
using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("D33C8144-0EEA-4430-9F74-ED428A043E0D")]
    public class TdmElectricPriceListTemplate : ExcelPriceListTemplateBase
    {
        public TdmElectricPriceListTemplate(string fileName) : base(fileName) { }
        string NEXT_SHIPMENT_BACKGROUND_CELL_COLOR = "#FFCCCCFF";

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 9; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);

                if (string.IsNullOrWhiteSpace(skuNumber) || !skuNumber.StartsWith("SQ")) { continue; }

                var price = ParsePrice(tab.GetValue<string>(row, 7)); // базовая цена
                string quantityCellBackgroundColor = tab.Cells[row, 4].Style.Fill.BackgroundColor.LookupColor();
                string quantityString = tab.GetValue<string>(row, 4);
                var quantity = ParseQuantity(quantityString);

                var priceLine = new PriceLineWithNextDeliveryDate(this)
                {
                    Currency = CurrencyType.RUB,
                    Manufacturer = "TDM Electric",
                    Model = skuNumber,
                    Sku = skuNumber,
                    Price = price,
                    Quantity = quantity
                };

                if (NEXT_SHIPMENT_BACKGROUND_CELL_COLOR == quantityCellBackgroundColor && DateTime.TryParse(quantityString, out var nextDelDate))
                {
                    priceLine.NextStockDelivery = new DataAccess.NextStockDelivery()
                    {
                        Date = nextDelDate.Date,
                        Quantity = 1
                    };
                }

                list.Add(priceLine);
            }

            return list;
        }
    }
}
