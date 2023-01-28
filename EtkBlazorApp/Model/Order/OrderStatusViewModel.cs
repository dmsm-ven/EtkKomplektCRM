namespace EtkBlazorApp.Model.Order;

public class OrderStatusViewModel
{
    public OrderStatusType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public enum OrderStatusType
{
    None,
    Created,
    Complectation,
    Delivery,
    WaitingToPickup,
    Completed,
    Canceled
}
