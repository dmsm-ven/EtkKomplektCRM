using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("E11729F8-244B-420A-801C-110FC81BE61B")]
    public class MarsKomponentPriceListTemplate : ExcelPriceListTemplateBase
    {
        readonly string[] STOCK_PARTNER_MANUFACTURERS = new[] { "Pro'sKit" };
        readonly string[] VALID_MANUFACTURERS = new[] { "PROSKIT", "Mastech", "UNI-T" };
        readonly Dictionary<string, string> ManufacturerMap = new Dictionary<string, string>()
        {
            ["PROSKIT"] = "Pro'sKit"
        };
              
        const int START_ROW_NUMBER = 9;

        public MarsKomponentPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {                
                string manufacturer = tab.Cells[row, 8].ToString();

                //Если не из списка то пропускаем
                if (!VALID_MANUFACTURERS.Any(valid => valid.Equals(manufacturer, StringComparison.OrdinalIgnoreCase))) { continue; }

                if (ManufacturerMap.ContainsKey(manufacturer))
                {
                    manufacturer = ManufacturerMap[manufacturer];
                }

                string model = tab.Cells[row, 7].ToString();
                string sku = manufacturer + " " + tab.Cells[row, 0].ToString();
                string priceString = tab.Cells[row, 6].ToString();
                string quantityString = tab.Cells[row, 4].ToString();
                string name = tab.Cells[row, 1].ToString();

                if (decimal.TryParse(priceString.Trim(), out decimal price)) { }
                if (decimal.TryParse(quantityString, out decimal quantity)) { quantity = Math.Floor(quantity); }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    Manufacturer = manufacturer,
                    Model = model,
                    Sku = sku,
                    Price = price,
                    Quantity = (int)quantity,
                    //Если производитель входит в список то его количество добавляем в склад Марс Компонент
                    StockPartner = (STOCK_PARTNER_MANUFACTURERS.Contains(manufacturer) ? StockPartner.MarsComponent : null)
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
