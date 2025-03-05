namespace EtkBlazorApp.WildberriesApi.Data;

public class WBPriceUpdatePayload
{
    public List<WBProductPriceUpdate> data { get; set; } = new();
}

public class WBProductPriceUpdate
{
    public int nmID { get; set; }
    public int price { get; set; }
    //public int discount { get; set; }
}
