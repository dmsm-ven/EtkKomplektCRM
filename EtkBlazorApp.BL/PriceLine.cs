namespace EtkBlazorApp.BL
{
    public class PriceLine
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public Currency Currency { get; set; }
    }
}
