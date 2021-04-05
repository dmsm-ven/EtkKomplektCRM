using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("50B4B307-08B5-4498-B9D6-B2DA86D97F29")]
    public class KvtPriceListTemplate : ExcelPriceListTemplateBase
    {
        public KvtPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            
           foreach(var tab in Excel.Workbook.Worksheets.Skip(1))
           {
                for (int row = 1; row < tab.Dimension.Rows; row++)
                {
                    string code = "KV-" + tab.Cells[row, 0].ToString();
                    string model = Regex.Match(tab.Cells[row, 1].ToString(), @"^(.*?) \((.*?)\)$").Groups[1].Value.Trim();
                    string opt = tab.Cells[row, 4].ToString();

                    if (Regex.IsMatch(code, @"KV-\d+") && decimal.TryParse(opt, out var price))
                    {
                        var priceLine = new PriceLine(this)
                        {
                            Manufacturer = "КВТ",
                            Sku = code,
                            Model = model,
                            Price = price,
                            Currency = CurrencyType.RUB
                        };

                        list.Add(priceLine);
                    }
                }
           }

            return list;
        }
    }

    [PriceListTemplateGuid("CD4A4607-4623-4522-978D-D153F3650192")]
    public class KvtQuantityListTemplate : ExcelPriceListTemplateBase
    {
        public KvtQuantityListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 1; row < tab.Dimension.Rows; row++)
            {
                string name = tab.Cells[row, 0].ToString();
                string code = "KV-" + tab.Cells[row, 1].ToString();
                string ean = tab.Cells[row, 2].ToString();               
                string priceString = tab.Cells[row, 4].ToString();

                if (int.TryParse(priceString, out var quantity))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Manufacturer = "КВТ",
                        Name = name,
                        Sku = code,
                        Model = ean,
                        Quantity = quantity
                    };

                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
}
