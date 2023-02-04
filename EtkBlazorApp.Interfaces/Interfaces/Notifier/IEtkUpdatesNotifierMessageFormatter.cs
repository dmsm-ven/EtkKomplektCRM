using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface IEtkUpdatesNotifierMessageFormatter
{
    string GetPriceListChangedMessage(string priceListName, double percent, int totalProducts);
    string GetTaskLoadErrorMessage(string taskName);
    string GetOrderStatusChangedMessage(int order_id, string statusName);
}
