using EtkBlazorApp.Core.Data;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{
    public abstract class DipaulQuantityTemplateBase : ExcelPriceListTemplateBase
    {

        public DipaulQuantityTemplateBase(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();
            var tab = Excel.Workbook.Worksheets[0];

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 3));

                if (SkipThisBrand(manufacturer))
                {
                    continue;
                }

                string skuNumber = tab.GetValue<string>(row, 1);
                string productName = tab.GetValue<string>(row, 2);
                int? quantity = ParseQuantity(tab.GetValue<string>(row, 4));
                decimal? price = ParsePrice(tab.GetValue<string>(row, 5));

                string separator = productName.Contains(",") ? "," : " ";

                string model = Regex.Match(productName, "^(.*?)" + separator).Groups[1].Value.Trim();
                string currencyTypeString = tab.GetValue<string>(row, 6);

                CurrencyType priceCurreny = CurrencyType.RUB;
                if (!Enum.TryParse(currencyTypeString, out priceCurreny)) { }

                var priceLine = new PriceLine(this)
                {
                    Name = productName,
                    Manufacturer = manufacturer,
                    Model = model,
                    Sku = skuNumber,
                    Price = price,
                    Currency = priceCurreny,
                    Quantity = quantity,
                    //Stock = StockName.Dipaul
                };
                list.Add(priceLine);
            }

            return list;
        }
    }

    //Т.к. нельзя создать больше одного шаблона для одного и того же [PriceListTemplateGuid()]
    //Делаем 4 отдельных
    //TODO: Возможно, стоить сделать возможность создания больше одного шаблона для одного и того же GUID
    //(тогда отдельные классы не понадобятся, но могут быть другие проблемы)

    //Hakko
    [PriceListTemplateGuid("A53B8C85-3115-421F-A579-0B5BFFF6EF49")]
    public class DipaulQuantityHakkoPriceListTemplate : DipaulQuantityTemplateBase
    {
        public DipaulQuantityHakkoPriceListTemplate(string fileName) : base(fileName) { }
    }

    //ITECH
    [PriceListTemplateGuid("B53B8C85-3115-421F-A579-0B5BFFF6EF49")]
    public class DipaulQuantityItechPriceListTemplate : DipaulQuantityTemplateBase
    {
        public DipaulQuantityItechPriceListTemplate(string fileName) : base(fileName) { }
    }

    //Keysight
    [PriceListTemplateGuid("C53B8C85-3115-421F-A579-0B5BFFF6EF49")]
    public class DipaulQuantityKeysightPriceListTemplate : DipaulQuantityTemplateBase
    {
        public DipaulQuantityKeysightPriceListTemplate(string fileName) : base(fileName) { }
    }

    //Agilent
    [PriceListTemplateGuid("D53B8C85-3115-421F-A579-0B5BFFF6EF49")]
    public class DipaulQuantityAgilentPriceListTemplate : DipaulQuantityTemplateBase
    {
        public DipaulQuantityAgilentPriceListTemplate(string fileName) : base(fileName) { }
    }
}
