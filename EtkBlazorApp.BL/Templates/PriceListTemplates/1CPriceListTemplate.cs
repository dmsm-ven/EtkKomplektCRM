using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("4EA34EEA-5407-4807-8E33-D8A8FA71ECBA")]
    public class _1CPriceListTemplate : ExcelPriceListTemplateBase
    {
        const int START_ROW_NUMBER = 3;

        public _1CPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = START_ROW_NUMBER; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 1).Trim('.', ' ');
                string name = tab.GetValue<string>(row, 4);
                int quantity = tab.GetValue<int>(row, 11);
                
                if (string.IsNullOrWhiteSpace(skuNumber)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    IsSpecialLine = true,
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            // Для Testo удаляем пробелы а моделях
            list.Where(row => row.Name.Contains("testo")).ToList().ForEach(p => p.Model = p.Model.Replace(" ", string.Empty));

            return list;
        }
    }
}
