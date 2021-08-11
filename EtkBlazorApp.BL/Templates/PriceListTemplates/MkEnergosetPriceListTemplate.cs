using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("9EEB7A82-1029-4C1F-A282-196C0907160B")]
    public class MkEnergosetPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MkEnergosetPriceListTemplate(string fileName) : base(fileName) 
        {
            ValidManufacturerNames.AddRange(new[] { "MTD", "Cub Cadet", "Wolf Garten" });
            QuantityMap["Достаточное количество"] = 50;
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 8; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string model = tab.GetValue<string>(row, 2);
                var price = ParsePrice(tab.GetValue<string>(row, 3));
                var quantity = ParseQuantity(tab.GetValue<string>(row, 5));
                       
                string manufacturer = ValidManufacturerNames.FirstOrDefault(vm => name.Contains(vm, StringComparison.OrdinalIgnoreCase));
                if(manufacturer == null) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Manufacturer = manufacturer,
                    Model = model,
                    Sku = model,
                    Price = price,
                    Quantity = quantity,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.MkEnergoset
                };
                list.Add(priceLine);
            }

            return list;
        }    
    }
}
