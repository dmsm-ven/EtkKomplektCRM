using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    [PriceListTemplateGuid("3D41DDC2-BB5C-4D6A-8129-C486BD953A3D")]
    public class MeanWellSilverPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MeanWellSilverPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 3; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 1);
                string skuNumber = tab.GetValue<string>(row, 2);
                int quantityString = tab.GetValue<int>(row, 6);

                decimal regularPriceString = tab.GetValue<decimal>(row, 7);
                decimal discountPriceString = tab.GetValue<decimal>(row, 8);

                decimal priceString = new decimal[] { regularPriceString, discountPriceString }.Max();

                if (manufacturer.Equals("MeanWell", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Quantity = quantityString,
                        Currency = CurrencyType.USD,
                        Price = priceString,
                        Manufacturer = "Mean Well",
                        Sku = skuNumber,
                        Model = skuNumber
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("2231E716-3643-4EDE-B6D0-764DB3B4DF68")]
    public class MeanWellPartnerPriceListTemplate : ExcelPriceListTemplateBase
    {
        public MeanWellPartnerPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string sku = tab.GetValue<string>(row, 1);
                string model = tab.GetValue<string>(row, 2);
                int partSize = tab.GetValue<int>(row, 6);
                var currency = Enum.Parse<CurrencyType>(tab.GetValue<string>(row, 7));
                decimal priceBezNds = tab.GetValue<decimal>(row, 8);
                
                if(partSize != 1) { continue; }
                
                var priceLine = new PriceLine(this)
                {
                    Currency = currency,
                    Price = priceBezNds,
                    Manufacturer = "Mean Well",
                    Sku = sku,
                    Model = model                   
                };
                list.Add(priceLine);
            }

            return list;
        }
    }
}
