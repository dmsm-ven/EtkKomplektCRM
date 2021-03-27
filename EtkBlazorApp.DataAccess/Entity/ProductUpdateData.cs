namespace EtkBlazorApp.DataAccess
{
    public class ProductUpdateData
    {
        public int product_id { get; set; }      
        public decimal? price { get; set; }
        public int? quantity { get; set; }
        public string currency_code { get; set; }
    }
}
