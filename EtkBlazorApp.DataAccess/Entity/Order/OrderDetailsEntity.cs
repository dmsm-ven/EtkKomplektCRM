namespace EtkBlazorApp.DataAccess.Entity;

public class OrderDetailsEntity
{
    public int order_product_id { get; set; }
    public int order_id { get; set; }
    public int product_id { get; set; }
    public string name { get; set; }
    public string model { get; set; }
    public string sku { get; set; }
    public string manufacturer { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
    public decimal cost { get; set; }
    public decimal total { get; set; }
    public decimal tax { get; set; }
    public string reward { get; set; }
}