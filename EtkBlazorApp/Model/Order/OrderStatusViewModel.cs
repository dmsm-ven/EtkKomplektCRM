namespace EtkBlazorApp.Model.Order;

public class OrderStatusViewModel
{
    public OrderStatusType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
}

public enum OrderStatusType
{
    None = 0,
    Created = 1,
    Complectation = 2,
    Canceled = 9,
    Delivery = 17,
    WaitingToPickup = 18,
    Completed = 19,

}
