namespace EtkBlazorApp.DataAccess.Entity.PriceList;

public partial class PriceListUpdateProductHistory
{
    public int update_id { get; set; }
    public int product_id { get; set; }
    public decimal price { get; set; }
}
