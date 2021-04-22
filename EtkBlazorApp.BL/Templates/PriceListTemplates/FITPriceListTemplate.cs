using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("91EE5CFF-4752-4D6F-8E36-E5557149225B")]
    public class FITQuantityTemplate : ExcelPriceListTemplateBase
    {
        public FITQuantityTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = "FIT-" + tab.Cells[row, 0].ToString();
                string name = tab.Cells[row, 1].ToString();
                string ean = tab.Cells[row, 3].ToString();
                string quantityString = tab.Cells[row, 6].ToString().Trim(' ', '>', '<');
                string priceString = tab.Cells[row, 5].ToString().Replace(" ", string.Empty).Trim();

                int? parsedQuantity = null;
                if (int.TryParse(quantityString, out var quantity))
                {
                    parsedQuantity = quantity;
                }

                decimal? parsedPrice = null;
                if (decimal.TryParse(priceString, out var price))
                {
                    parsedPrice = price;
                }

                if (parsedQuantity.HasValue || parsedPrice.HasValue)
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "FIT",
                        Price = parsedPrice,
                        Currency = CurrencyType.RUB,
                        Sku = skuNumber,
                        Name = name,
                        Model = ean,
                        Quantity = parsedQuantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
}
