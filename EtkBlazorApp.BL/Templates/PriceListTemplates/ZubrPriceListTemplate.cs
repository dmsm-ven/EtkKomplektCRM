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

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);               
                string manufacturer = tab.GetValue<string>(row, 8).Trim();
                decimal price = tab.GetValue<decimal>(row, 3);

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
    public class ZubrQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ZubrQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string quantityString = tab.GetValue<string>(row, 12);
                string manufacturer = tab.GetValue<string>(row, 8).Trim();

                if (!manufacturer.Equals("Зубр", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                };

                int parsedQuantity = (quantityString == "Есть" ? 10 : 0);

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Зубр",
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = parsedQuantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
