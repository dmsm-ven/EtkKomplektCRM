using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("3853B988-DB37-4B6E-861F-3000B643FAC4")]
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        public SymmetronPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                var priceLine = new PriceLine(this)
                {
                    Name = tab.Cells[row, 1].ToString(),
                    Sku = tab.Cells[row, 3].ToString(),
                    Model = tab.Cells[row, 27].ToString(),
                    Manufacturer = tab.Cells[row, 26].ToString(),
                    Price = ParsePrice(tab.Cells[row, 13].ToString()),
                    Currency = CurrencyType.RUB
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
