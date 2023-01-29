using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
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
        const int START_ROW = 3;

        protected override List<PriceLine> ReadDataFromExcel()
        {
            var list = new List<PriceLine>();

            PriceLine previousPriceLine = null;
            for (int row = START_ROW; row < tab.Dimension.Rows; row++)
            {
                string manufacturer = MapManufacturerName(tab.GetValue<string>(row, 1));
                string skuNumber = tab.GetValue<string>(row, 2);

                if (string.IsNullOrWhiteSpace(skuNumber) || SkipThisBrand(manufacturer))
                {
                    continue;
                }

                int quantityString = tab.GetValue<int>(row, 6);

                if (previousPriceLine != null && previousPriceLine.Sku == skuNumber && previousPriceLine.Manufacturer == manufacturer)
                {
                    previousPriceLine.Quantity += quantityString;
                    continue;
                }

                decimal regularPriceString = tab.GetValue<decimal>(row, 7);
                decimal discountPriceString = tab.GetValue<decimal>(row, 8);

                int? stockNextShipmentQuantity = ParseQuantity(tab.GetValue<string>(row, 9), canBeNull: true);
                int? stockNextShipmentWeeks = ParseQuantity(tab.GetValue<string>(row, 10), canBeNull: true);

                decimal priceString = new decimal[] { regularPriceString, discountPriceString }.Max();

                NextStockDelivery productDeliveryInfo = null;
                if (stockNextShipmentQuantity.HasValue && stockNextShipmentWeeks.HasValue)
                {
                    productDeliveryInfo = new DataAccess.NextStockDelivery()
                    {
                        Date = DateTime.Now.AddDays(stockNextShipmentWeeks.Value * 7).Date,
                        Quantity = stockNextShipmentQuantity.Value
                    };
                }

                var priceLine = new PriceLineWithNextDeliveryDate(this)
                {
                    Quantity = quantityString,
                    Currency = CurrencyType.USD,
                    Price = priceString,
                    Manufacturer = manufacturer,
                    Sku = skuNumber,
                    Model = skuNumber,
                    //Stock = StockName.Eltech,
                    NextStockDelivery = productDeliveryInfo
                };


                list.Add(priceLine);
                previousPriceLine = priceLine;
            }

            return list;
        }
    }

    public abstract class EltechPartnersPriceListTemplate : ExcelPriceListTemplateBase
    {
        private readonly string manufacturer;

        public EltechPartnersPriceListTemplate(string fileName, string manufacturer) : base(fileName)
        {
            this.manufacturer = manufacturer;
        }

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

                if (partSize != 1) { continue; }

                var priceLine = new PriceLine(this)
                {
                    Currency = currency,
                    Price = priceBezNds,
                    Manufacturer = manufacturer,
                    Sku = sku,
                    Model = model,
                    //Stock = StockName.Eltech
                };
                list.Add(priceLine);
            }

            return list;
        }
    }

    [PriceListTemplateGuid("2231E716-3643-4EDE-B6D0-764DB3B4DF68")]
    public class EltechMeanWellPartnerPriceListTemplate : EltechPartnersPriceListTemplate
    {
        public EltechMeanWellPartnerPriceListTemplate(string fileName) : base(fileName, "Mean Well") { }
    }

    [PriceListTemplateGuid("F554CF11-AA44-484E-B047-093453B1E261")]
    public class EltechNecPartnerPriceListTemplate : EltechPartnersPriceListTemplate
    {
        public EltechNecPartnerPriceListTemplate(string fileName) : base(fileName, "NEC") { }
    }

    [PriceListTemplateGuid("2BC535B6-1443-4D59-89FA-FD2DBB936395")]
    public class EltechTianmaPartnerPriceListTemplate : EltechPartnersPriceListTemplate
    {
        public EltechTianmaPartnerPriceListTemplate(string fileName) : base(fileName, "Tianma") { }
    }
}
