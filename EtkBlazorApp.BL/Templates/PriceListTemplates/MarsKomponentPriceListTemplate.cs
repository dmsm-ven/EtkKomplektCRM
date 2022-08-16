using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("E11729F8-244B-420A-801C-110FC81BE61B")]
    public class MarsKomponentPriceListTemplate : ExcelPriceListTemplateBase
    {              
        public MarsKomponentPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 11; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 10));
                
                if (ManufacturerSkipCheck(manufacturer)) { continue; }           

                //string prefix = $"{manufacturer} ";
                //string sku = prefix + tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                int quantity = (int)tab.GetValue<decimal>(row, 5);
                int? nextDeliveryQuantity = ParseQuantity(tab.GetValue<string>(row, 6));
                decimal? price = tab.GetValue<decimal>(row, 8); 
                string model = tab.GetValue<string>(row, 9);

                var priceLine = new PriceLineWithNextDeliveryDate(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Manufacturer = manufacturer,
                    Model = model,
                    //Sku = sku,
                    Price = price,
                    Quantity = quantity,
                };

                priceLine.NextStockDelivery = new DataAccess.NextStockDelivery()
                {
                    Date = DateTime.Now.Date.AddDays(7),
                    Quantity = nextDeliveryQuantity.Value
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
