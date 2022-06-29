using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("0D58935B-3D84-48F3-98E3-35E99EBAC96C")]
    public class BergerPriceListTemplate : ExcelPriceListTemplateBase
    {
        public BergerPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            var tab = Excel.Workbook.Worksheets.OfType<ExcelWorksheet>().FirstOrDefault(t => t.Name.Contains("TM BERGER"));
            if (tab == null) { return list; }
           
            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                int? quantity = null;
                try
                {
                    quantity = ParseQuantity(tab.GetValue<string>(row, 7));
                }
                catch
                {
                    quantity = 0;
                }

                string manufacturer = tab.GetValue<string>(row, 1);
                string sku = tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);
                var rrcPrice = ParsePrice(tab.GetValue<string>(row, 5));

                if (string.IsNullOrWhiteSpace(sku) || ManufacturerSkipCheck(manufacturer)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    Currency = CurrencyType.RUB,
                    //Stock = StockName.Berger,
                    Manufacturer = manufacturer,
                    Model = sku,
                    Ean = sku,
                    Sku = sku,
                    Price = rrcPrice,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }
            

            return list;
        }
    }

    [PriceListTemplateGuid("CDC4B7DE-BC2C-4249-89E1-B6BC4897FA46")]
    public class BergerQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public BergerQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 3; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 2);
                string name = tab.GetValue<string>(row, 3);
                var quantity = ParseQuantity(tab.GetValue<string>(row, 4));
                string ean = tab.GetValue<string>(row, 5);

                if (string.IsNullOrWhiteSpace(sku)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Name = name,
                    //Stock = StockName.Berger,
                    Manufacturer = "Berger",
                    Model = sku,
                    Sku = sku,
                    Ean = ean,
                    Quantity = quantity
                };

                list.Add(priceLine);
            }

            return list;
        }
    }
}
