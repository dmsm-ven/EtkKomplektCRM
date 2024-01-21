using EtkBlazorApp.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("58F658D6-483C-4EE6-9E23-9932514624CF")]
    public class RgkPriceListTemplate : ExcelPriceListTemplateBase
    {
        public RgkPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 8; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string manufacturer = tab.GetValue<string>(row, 4);
                string sku = tab.GetValue<string>(row, 6);
                int? quantitySpb = ParseQuantity(tab.GetValue<string>(row, 8));

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Manufacturer = manufacturer,
                    Model = sku,
                    Sku = sku,
                    Quantity = quantitySpb,
                };

                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("3DC6BA41-0B2A-45B2-8C50-6B0166060191")]
    public class RgkKIPPriceListTemplate : ExcelPriceListTemplateBase
    {
        public RgkKIPPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            const string brand = "HIKMICRO";

            var hikmicroTab = this.Excel.Workbook.Worksheets.FirstOrDefault(t => t.Name.Contains(brand));

            if (hikmicroTab != null)
            {
                for (int row = 2; row < hikmicroTab.Dimension.Rows; row++)
                {
                    var sku = hikmicroTab.GetValue<string>(row, 3);
                    var model = hikmicroTab.GetValue<string>(row, 4);
                    var name = hikmicroTab.GetValue<string>(row, 5);
                    var rrcPrice = ParsePrice(hikmicroTab.GetValue<string>(row, 6));

                    var line = new PriceLine(this)
                    {
                        Manufacturer = brand,
                        Currency = CurrencyType.RUB,
                        Model = model,
                        Sku = sku,
                        Name = name,
                        Price = rrcPrice
                    };

                    list.Add(line);
                }
            }

            return list;
        }
    }

}
