namespace EtkBlazorApp.Core.Data.Integration1C;

public class StoreStockData
{
    public string store { get; set; }
    public StoreStockSku[] Sku { get; set; }
}

public class StoreStockSku
{
    public string classSku { get; set; }
    public string article { get; set; }
    public string name { get; set; }
    public string unitMeasure { get; set; }
    public string quantity { get; set; }
}