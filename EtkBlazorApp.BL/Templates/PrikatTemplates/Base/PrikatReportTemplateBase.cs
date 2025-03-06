using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates.Base
{
    public abstract class PrikatReportTemplateBase
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
        protected readonly PricatFormatterBase formatter;

        public decimal CurrencyRatio { get; set; }
        public decimal Discount { get; set; }
        public string GLN { get; set; }
        public Dictionary<int, decimal> ProductIdToDiscount { get; set; } = new();
        public int Precission { get; }
        public string Manufacturer { get; }
        public CurrencyType Currency { get; }

        public PrikatReportTemplateBase(string manufacturer, CurrencyType currency, PricatFormatterBase formatter)
        {
            Manufacturer = manufacturer;
            Currency = currency;
            Precission = Currency == CurrencyType.RUB ? 0 : 2;

            this.formatter = formatter;
        }

        protected decimal GetCustomDiscountOrDefaultForProductId(int product_id)
        {
            if (ProductIdToDiscount != null && ProductIdToDiscount.ContainsKey(product_id))
            {
                return ProductIdToDiscount[product_id];
            }
            return Discount;
        }

        public void WriteProductLines(IEnumerable<ProductEntity> products, StreamWriter sw)
        {
            foreach (var product in products)
            {
                WriteProductLine(product, sw);
            }
        }

        protected virtual void WriteProductLine(ProductEntity product, StreamWriter sw)
        {
            decimal sellPrice = (int)product.price;
            if (Currency != CurrencyType.RUB)
            {
                sellPrice = product.base_currency_code == Currency.ToString() && product.base_price != decimal.Zero ?
                    product.base_price :
                    Math.Round(product.price / CurrencyRatio, 2);
            }

            decimal currentDiscount = GetCustomDiscountOrDefaultForProductId(product.product_id);

            decimal rrcPrice = Math.Round(sellPrice * ((100m + currentDiscount) / 100m), Precission);

            //Важно, эта проверка с изменение должна быть после расчет [decimal rrcPrice = ...]
            if (currentDiscount != Discount)
            {
                decimal discountDiff = (Discount - currentDiscount);
                if (discountDiff != decimal.Zero)
                {
                    decimal discountRatio = 1.00m - (discountDiff / 100m);
                    sellPrice = Math.Round(sellPrice * discountRatio, Precission);
                }
            }

            formatter.WriteProductEntry(product, rrcPrice, sellPrice);
        }
    }
}
