using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using NLog;
using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates.Base
{
    public abstract class PrikatReportTemplateBase
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        public decimal CurrencyRatio { get; }
        public decimal Discount { get; }
        public VseInstrumentiReportOptions Options { get; }
        public int Precission { get; }
        public string Manufacturer { get; }
        public CurrencyType Currency { get; }

        private readonly Dictionary<int, decimal> productIdToDiscount = new();
        protected IReadOnlyDictionary<int, decimal> ProductIdToDiscount => productIdToDiscount;

        public PrikatReportTemplateBase(string manufacturer,
            CurrencyType currency,
            decimal discount,
            decimal currentCurrencyRate,
            VseInstrumentiReportOptions options)
        {
            this.Manufacturer = manufacturer;
            this.Currency = currency;
            this.Precission = Currency == CurrencyType.RUB ? 0 : 2;
            this.Discount = discount;
            this.CurrencyRatio = currentCurrencyRate;
            this.Options = options;
        }

        public void AddDiscountMapItems(IEnumerable<KeyValuePair<int, decimal>> items)
        {
            productIdToDiscount.Clear();
            foreach (var item in items)
            {
                productIdToDiscount[item.Key] = item.Value;
            }
        }

        protected decimal GetCustomDiscountOrDefaultForProductId(int product_id)
        {
            if (ProductIdToDiscount != null && ProductIdToDiscount.ContainsKey(product_id))
            {
                return ProductIdToDiscount[product_id];
            }
            return Discount;
        }

        public void WriteProductLines(IEnumerable<ProductEntity> products, PricatFormatterBase formatter)
        {
            foreach (var product in products)
            {
                WriteProductLine(product, formatter);
            }
        }

        protected virtual void WriteProductLine(ProductEntity product, PricatFormatterBase formatter)
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
