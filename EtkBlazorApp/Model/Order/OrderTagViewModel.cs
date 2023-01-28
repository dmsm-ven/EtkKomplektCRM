namespace EtkBlazorApp.Model.Order;

public class OrderTagViewModel
{
    public OrderTagType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public enum OrderTagType
{
    None,
    ShippingTimeExceed,
}
