using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.IO;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    //В этом шаблоне, в отличии от обычных, не выгружаем РРЦ цена (всегда должна быть 0)
    //А цена расчитывается от цены поставщика SYMMETRON  
    public sealed class PrikatProskitReportTemplate : PrikatReportTemplateBase
    {
        private readonly int SYMMETRON_STOCK_ID = 4;

        public PrikatProskitReportTemplate(string manufacturer, CurrencyType currency, PricatFormatterBase formatter)
            : base(manufacturer, currency, formatter) { }

        protected override async void WriteProductLine(ProductEntity product, StreamWriter sw)
        {
            decimal priceInCurrency = product.stock_data == null ? 0 :
                product.stock_data.FirstOrDefault(s => s.stock_partner_id == SYMMETRON_STOCK_ID)?.original_price ?? 0;

            if (priceInCurrency == decimal.Zero)
            {
                return;
            }

            priceInCurrency = Math.Round(priceInCurrency / CurrencyRatio, 2);
            decimal purchasePrice = Math.Round(priceInCurrency * (100m + GetCustomDiscountOrDefaultForProductId(product.product_id)) / 100m, Precission);

            // Для Proskit исключение, у него закупочная цена специально сделана всегда = 0
            formatter.WriteProductEntry(product, 0m, purchasePrice);
        }
    }
}
