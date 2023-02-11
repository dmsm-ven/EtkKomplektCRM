namespace EtkBlazorApp;

public class OrderDetails
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Model { get; set; }
    public string Sku { get; set; }
    public string Manufacturer { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Sum => Quantity * Price;
}

