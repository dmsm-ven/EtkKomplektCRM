using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("2D93CEB9-FD28-44E1-8382-5A485770DD57")]
    public class ZubrPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ZubrPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);               
                string manufacturer = tab.GetValue<string>(row, 9).Trim();
                decimal price = tab.GetValue<decimal>(row, 4);

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Sku = skuNumber,
                    Model = skuNumber
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("C1412CC4-79E5-467F-A8E9-ACF18E320B92")]
    [Obsolete]
    public class ZubrQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ZubrQuantityPriceListTemplate(string fileName) : base(fileName) 
        {
            ValidManufacturerNames.Add("Зубр");
            ValidManufacturerNames.Add("Kraftool");
            ValidManufacturerNames.Add("Grinda");
            ValidManufacturerNames.Add("Stayer");
            QuantityMap["Есть"] = 5;
            QuantityMap["Нет"] = 0;
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 4).Trim();
                
                if (!ValidManufacturerNames.Contains(manufacturer, StringComparer.OrdinalIgnoreCase)) { continue; }
                string sku = tab.GetValue<string>(row, 1);
                string model = tab.GetValue<string>(row, 2);                           
                string name = tab.GetValue<string>(row, 5);
                
                var quantity = ParseQuantity(tab.GetValue<string>(row, 6));
                var price = ParsePrice(tab.GetValue<string>(row, 8));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Sku = sku,
                    Model = model,
                    Name = name,
                    Quantity = quantity,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.Eridan,
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    //Прайс-лист из личного кабинета сайта mks.master.pro
    [PriceListTemplateGuid("2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6")]
    public class MksMasterPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MksMasterPriceListTemplate(string fileName) : base(fileName) 
        {
            ValidManufacturerNames.Add("Зубр");
            ValidManufacturerNames.Add("Kraftool");
            ValidManufacturerNames.Add("Grinda");
            ValidManufacturerNames.Add("Stayer");
            QuantityMap["Да"] = 5;
            QuantityMap["Нет"] = 0;
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 6).Trim();

                if (!ValidManufacturerNames.Contains(manufacturer, StringComparer.OrdinalIgnoreCase)) { continue; }

                string sku = tab.GetValue<string>(row, 4);
                string name = tab.GetValue<string>(row, 5);
                string ean = tab.GetValue<string>(row, 16);
                
                var quantity = ParseQuantity(tab.GetValue<string>(row, 11));
                var price = ParsePrice(tab.GetValue<string>(row, 14));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = manufacturer,
                    Sku = sku,
                    Model = sku,
                    Ean = ean,
                    Name = name,
                    Quantity = quantity,
                    Price = price,
                    Currency = CurrencyType.RUB,
                    Stock = StockName.Eridan,
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
