using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp
{
    public class ProductToStockDataModel
    {
        public int ProductId { get; set; }
        public int StockId { get; set; }
        public string StockName { get; set; }
        public int? Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? Price { get; set; }
        public CurrencyType? PriceCurrency { get; set; }


    }
}
