using EtkBlazorApp.Data;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ProductUpdateData
    {
        public int product_id { get; set; }      
        public decimal? price { get; set; }
        public int? quantity { get; set; }
        public CurrencyType currency_code { get; set; }
    }
}
