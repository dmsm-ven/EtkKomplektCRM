using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EtkBlazorApp.BL
{
    [PriceListTemplateGuid("6C238D2C-145E-4320-B4E3-DCA8B8FAECB0")]
    public class PraktikaQuantityPriceListTemplate : ExcelPriceListTemplateBase
    {
        public PraktikaQuantityPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 2; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = tab.GetValue<string>(row, 2);
                string skuNumber = tab.GetValue<string>(row, 4);
                string name = tab.GetValue<string>(row, 5);
                decimal? price = ParsePrice(tab.GetValue<string>(row, 7));
                string quantityString = tab.GetValue<string>(row, 8);
                
                int quantity = QuantityMap[quantityString];

                if (string.IsNullOrEmpty(skuNumber)) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Quantity = quantity,
                    Name = name,
                    Sku = skuNumber,
                    Model = skuNumber,
                    Currency = CurrencyType.RUB,
                    Price = price,
                    Manufacturer = manufacturer
                };
                list.Add(priceLine);
            }

            return list;
        }
    }

    #region Старые прайс-листы, похоже они не нужны (на 22.04.2021)
    /*
    [PriceListTemplateGuid("2F924D38-47F6-4AC1-B439-B3B7056BC858")]
    public class NorthArrowsPriceListTemplate : ExcelPriceListTemplateBase
    {       
        public NorthArrowsPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 1);
                string quantityString = tab.GetValue<string>(row, 4);
                decimal price = tab.GetValue<decimal>(row, 3);

                if (!string.IsNullOrEmpty(skuNumber) && !string.IsNullOrWhiteSpace(name))
                {
                    int parsedQuantity;
                    switch (quantityString)
                    {
                        case "*": parsedQuantity = 1; break;
                        case "**": parsedQuantity = 2; break;
                        case "***": parsedQuantity = 5; break;
                        case "****": parsedQuantity = 9; break;
                        default: parsedQuantity = 0; break;
                    }

                    var priceLine = new PriceLine(this)
                    {
                        Quantity = parsedQuantity,
                        Price = price,
                        Name = name,
                        Currency = CurrencyType.RUB,
                        Sku = skuNumber,
                        Model = skuNumber
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("9AC900E8-5D4D-4085-8C9F-A774A7886B7A")]
    public class NorthArrowsBrandPriceListTemplate : ExcelPriceListTemplateBase
    {
        public NorthArrowsBrandPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            string manufacturer = tab.GetValue<string>(6, 0);

            for (int row = 7; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 4);
                decimal price = tab.GetValue<decimal>(row, 14);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = manufacturer,
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("B877C47E-351C-45BE-A0B9-1FB9CB3B86B4")]
    public class QuattroElementiPriceListTemplate : ExcelPriceListTemplateBase
    {
        public QuattroElementiPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 1);
                decimal price = tab.GetValue<decimal>(row, 3);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = "Quattro Elementi",
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("9C0AFA10-ACCA-406B-841E-34733CF490BC")]
    public class DDEPriceListTemplate : ExcelPriceListTemplateBase
    {
        public DDEPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 1);
                decimal price = tab.GetValue<decimal>(row, 3);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = "DDE",
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
        
    }

    [PriceListTemplateGuid("E504F904-B933-494C-BF83-DFE7C6DC096F")]
    public class PraktikaPriceListTemplate : ExcelPriceListTemplateBase
    {
        public PraktikaPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 2);
                decimal price = tab.GetValue<decimal>(row, 4);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = "Практика",
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("40A40EA8-8644-4676-9148-B382DB09B336")]
    public class PulsarPriceListTemplate : ExcelPriceListTemplateBase
    {
        public PulsarPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 1);
                decimal price = tab.GetValue<decimal>(row, 5);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = "Пульсар",
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }

    [PriceListTemplateGuid("B9E57D90-B25B-49AC-BAD6-6C97D67907D9")]
    public class KobaltPriceListTemplate : ExcelPriceListTemplateBase
    {
        public KobaltPriceListTemplate(string fileName) : base(fileName) { }

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            for (int row = 0; row < tab.Dimension.Rows; row++)
            {
                string skuNumber = tab.GetValue<string>(row, 0);
                string name = tab.GetValue<string>(row, 1);
                decimal price = tab.GetValue<decimal>(row, 3);

                if (!string.IsNullOrEmpty(skuNumber))
                {
                    var priceLine = new PriceLine(this)
                    {
                        Currency = CurrencyType.RUB,
                        Price = Math.Floor(price),
                        Manufacturer = "Кобальт",
                        Model = skuNumber,
                        Sku = skuNumber,
                        Name = name
                    };
                    list.Add(priceLine);
                }
            }

            return list;
        }
    }
    */
    #endregion
}
