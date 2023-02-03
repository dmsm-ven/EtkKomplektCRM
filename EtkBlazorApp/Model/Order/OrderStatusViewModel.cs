

using EtkBlazorApp.Core.Data.Order;

namespace EtkBlazorApp.Model.Order;


public class OrderStatusViewModel
{
    public OrderStatusCode Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
}