namespace EtkBlazorApp.Model.Order;

public class OrderStatusDisplayData
{
    public string Icon { get; set; }
    public string Background { get; set; }

    public OrderStatusDisplayData(string icon, string background)
    {
        Icon = icon;
        Background = background;
    }
}
