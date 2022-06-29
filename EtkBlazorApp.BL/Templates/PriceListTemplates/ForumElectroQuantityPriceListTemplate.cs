using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("310652CB-96D0-4AA2-A168-ED13F6354C1D")]
    public class ForumElectroQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public ForumElectroQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 1);
                string name = tab.GetValue<string>(row, 2);
                string sku = tab.GetValue<string>(row, 3);

                var quantityMoscov = ParseQuantity(tab.GetValue<string>(row, 6));
                var quantitySpb = ParseQuantity(tab.GetValue<string>(row, 7));

                var priceLine = new MultistockPriceLine(this)
                {
                    Quantity = quantitySpb,
                    Sku = sku,
                    Model = sku,
                    Name = name,
                    Manufacturer = manufacturer,
                    Stock = StockName.ForumElectro_Spb
                };

                priceLine.AdditionalStockQuantity[StockName.ForumElectro_Moscow] = quantityMoscov.Value;

                list.Add(priceLine);
            }

            return list;
        }
    }
}
