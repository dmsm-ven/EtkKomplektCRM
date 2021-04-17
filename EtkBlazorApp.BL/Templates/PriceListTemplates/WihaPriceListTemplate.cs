using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("DE1CBA89-1780-4FF5-A196-CF14D4258503")]
    public class WihaPriceListTemplate : ExcelPriceListTemplateBase
    {
        public WihaPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1);
                string priceString = tab.GetValue<string>(row, 3);

                if (decimal.TryParse(priceString, out var price))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Manufacturer = "Wiha",
                        Sku = skuNumber,
                        Model = skuNumber,
                        Price = price
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("A89C5911-12BE-4AD5-8A66-0621C4714360")]
    public class WihaQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public WihaQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0).ToString();
                string quantityString = tab.GetValue<string>(row, 6);

                if (decimal.TryParse(quantityString, out var quantity))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "Wiha",
                        Sku = skuNumber,
                        Model = skuNumber,
                        Quantity = (int)quantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("FFA35661-230F-431F-AEA0-BC57F4A7C8AE")]
    public class WihaQuantity2Template : ExcelPriceListTemplateBase
    {
        static readonly string[] VALID_MANUFACTURERS = new[] { "Wiha", "Schut" };

        public WihaQuantity2Template(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets.FirstOrDefault(t => t.Name.Contains("Остатки"));
            if(tab == null)
            {
                throw new FormatException("Вкладка 'Остатки' не найдена");
            }

            for (int row = 3; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string manufacturer = tab.GetValue<string>(row, 1);
                string quantityString = tab.GetValue<string>(row, 4);

                if (!VALID_MANUFACTURERS.Contains(manufacturer, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (decimal.TryParse(quantityString, out var quantity))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = manufacturer,
                        Sku = skuNumber,
                        Model = skuNumber,
                        Quantity = (int)quantity
                    };

                    list.Add(priceLine);
                }

            }

            return list;
        }
    }
}
