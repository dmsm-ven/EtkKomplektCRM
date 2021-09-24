using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("624500A1-B837-4FD5-9428-6AFDB1271285")]
    public class TestoQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public TestoQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 4));

                var priceLine = new PriceLine(this)
                {
                    Manufacturer = "Testo",
                    Name = name,
                    Model = sku,
                    Sku = sku,
                    Quantity = quantity,
                    Stock = StockName.Testo
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("04DC88D7-BC69-4746-BBF5-EE600D8575C6")]
    public class TestoPriceListTemplate : ExcelPriceListTemplateBase
    {
        private readonly string[] InvalidTabNames = new string[] { "Поверка", "3-rd party", "Оглавление" };

        public TestoPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tables = Excel.Workbook.Worksheets.Where(t => InvalidTabNames.Count(it => t.Name.Contains(it)) == 0).ToList();

            foreach (var tab in tables)
            {
                for (int row = 2; row < tab.Dimension.Rows; row++)
                {
                    string model = tab.GetValue<string>(row, 1);

                    var price = ParsePrice(tab.GetValue<string>(row, 3));

                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = price,
                        Manufacturer = "Testo",
                        Model = model,
                        Sku = model
                    };


                    if (!list.Any(p => p.Model == priceLine.Model))
                    {
                        list.Add(priceLine);
                    }
                }
            }

            return list;
        }
    }
}
