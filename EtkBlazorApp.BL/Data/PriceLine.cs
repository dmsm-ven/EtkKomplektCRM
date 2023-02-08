using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    public class PriceLine
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public string Ean { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public CurrencyType Currency { get; set; }
        public StockName Stock { get; set; }

        public PriceLine(IPriceListTemplate template)
        {
            Template = template;
        }

        public IPriceListTemplate Template { get; }
    }

    public class MultistockPriceLine : PriceLine
    {
        public MultistockPriceLine(IPriceListTemplate template) : base(template) { }

        public readonly Dictionary<StockName, int> AdditionalStockQuantity = new Dictionary<StockName, int>();
    }

    public class PriceLineWithNextDeliveryDate : PriceLine
    {
        public PriceLineWithNextDeliveryDate(IPriceListTemplate template) : base(template) { }

        public NextStockDelivery NextStockDelivery { get; set; }
    }
}
