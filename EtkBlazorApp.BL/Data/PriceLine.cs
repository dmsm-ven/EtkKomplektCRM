namespace EtkBlazorApp.BL
{
    public class PriceLine
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public string Ean { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public CurrencyType Currency { get; set; }
        public bool IsSpecialLine { get; set; }
        public StockPartner? StockPartner { get; set; }

        public PriceLine(IPriceListTemplate template)
        {
            Template = template;
        }

        public IPriceListTemplate Template { get; }
    }

    public enum StockPartner
    {        
        MarsComponent = 1,
        _1C = 2,
        Eltech = 3,
    }
}
