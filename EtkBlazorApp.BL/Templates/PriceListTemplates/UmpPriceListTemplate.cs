using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("A89C5911-12BE-4AD5-8A66-0621C4714360")]
    public class UmpPriceListTemplate : ExcelPriceListTemplateBase
    {
        public UmpPriceListTemplate(string fileName) : base(fileName) 
        {
            ValidManufacturerNames.Add("Wiha");
            ValidManufacturerNames.Add("Klauke");
            ValidManufacturerNames.Add("Weicon");
            ValidManufacturerNames.Add("Brady");
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 4));
                var rrcPrice = ParsePrice(tab.GetValue<string>(row, 8));
               
                string manufacturer = GetManufacturerBySkuPrefix(ref sku);

                if(!ValidManufacturerNames.Contains(manufacturer))
                {
                    continue;
                }

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Sku = sku,
                    Model = sku,
                    Name = name,
                    Quantity = quantity,
                    Price = rrcPrice,
                    Stock = StockName.UMP
                };

                list.Add(priceLine);
            }

            return list;
        }

        private string GetManufacturerBySkuPrefix(ref string sku)
        {
            string brandPart = sku.Substring(0, 3);

            string manufacturer = string.Empty;

            switch (brandPart)
            {
                case "wih": manufacturer = "Wiha"; break;
                case "brd": manufacturer = "Brady"; break;
                case "klk": manufacturer = "Klauke"; break;
                case "wcn": manufacturer = "Weicon"; break;
            }

            if (manufacturer == "Wiha")
            {
                sku = sku.Replace("wih", "WI-");
            }

            return manufacturer;
        }
    }
}
