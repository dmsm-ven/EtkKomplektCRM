namespace EtkBlazorApp.DataAccess.Entity.PriceList;

public class PriceListUpdateProductHistoryEntity
{
    public int update_id { get; set; }
    public int product_id { get; set; }
    public decimal price { get; set; }
}
