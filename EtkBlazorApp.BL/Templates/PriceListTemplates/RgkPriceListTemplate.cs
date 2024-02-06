using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("58F658D6-483C-4EE6-9E23-9932514624CF")]
    public class RgkPriceListTemplate : ExcelPriceListTemplateBase
    {
        private const int HEADER_ROW_INDEX = 7;
        private const int START_PRODUCT_ROW_INDEX = 9;

        public RgkPriceListTemplate(string fileName) : base(fileName) { }

        private Dictionary<StockName, int> GetHeaderIndexes()
        {
            Dictionary<StockName, int> columnIndexes = new()
            {
                [StockName.RGK_Moscow] = 11,
                [StockName.RGK_Spb] = 12,
                [StockName.RGK_Rostov] = 13,
                [StockName.RGK_Novosibirsk] = 14,
                [StockName.RGK_Ekaterinburg] = 15,
            };

            /*for (int i = 0; i < tab.Columns.Count(); i++)
            {
                var headerColumnText = tab.GetValue<string>(HEADER_ROW_INDEX, i).Trim();
                if (string.IsNullOrWhiteSpace(headerColumnText)) { continue; }

                StockName stockColumn = headerColumnText switch
                {
                    "МСК склад" => StockName.RGK_Moscow,
                    "СПб склад офис" => StockName.RGK_Spb,
                    "РСТ склад" => StockName.RGK_Rostov,
                    "ЕКБ склад" => StockName.RGK_Ekaterinburg,
                    "НВСБ склад" => StockName.RGK_Novosibirsk,
                    _ => StockName.None
                };

                if (stockColumn != StockName.None)
                {
                    columnIndexes[stockColumn] = i;
                }
            }

            if (columnIndexes.Any((kvp) => kvp.Value == -1))
            {
                throw new Exception("Шаблон прайс-листа был изменен. Необходима перепроверка данных");
            }*/

            return columnIndexes;
        }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            Dictionary<StockName, int> columnIndexes = GetHeaderIndexes();

            var list = new List<PriceLine>();

            for (int row = START_PRODUCT_ROW_INDEX; row < tab.Dimension.Rows; row++)
            {
                string name = tab.GetValue<string>(row, 1);
                string manufacturer = tab.GetValue<string>(row, 4);
                string sku = tab.GetValue<string>(row, 6);

                var price = ParsePrice(tab.GetValue<string>(row, 8));

                int? quantitySpb = ParseQuantity(tab.GetValue<string>(row, columnIndexes[StockName.RGK_Spb]), canBeNull: false);

                var priceLine = new MultistockPriceLine(this)
                {
                    Name = name,
                    Manufacturer = manufacturer,
                    Model = sku,
                    Sku = sku,
                    Price = price,
                    Quantity = quantitySpb,
                    Stock = StockName.RGK_Spb
                };

                foreach (var kvp in columnIndexes)
                {
                    //СПБ добавляется напрямую как главный
                    if (kvp.Key == StockName.RGK_Spb)
                    {
                        continue;
                    }
                    priceLine.AdditionalStockQuantity[kvp.Key] = (int)ParseQuantity(tab.GetValue<string>(row, kvp.Value), canBeNull: false);
                }

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
