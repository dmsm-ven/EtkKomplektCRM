using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.DataAccess.Entity;

public class ProductToStockEntity
{
    public int product_id { get; set; }
    public int stock_partner_id { get; set; }
    public int? quantity { get; set; }
    public decimal? original_price { get; set; }
    public decimal? price { get; set; }
    public CurrencyType? currency_code { get; set; }

    public string stock_name { get; set; }
}
