using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface IEtkUpdatesNotifierMessageFormatter
{
    string GetPriceListChangedMessage(string priceListName, double percent, int totalProducts, bool isMaxItems);
    string GetTaskLoadErrorMessage(string taskName);
    string GetOrderStatusChangedMessage(int? etkOrderId, string cdekOrderId, string statusName);
}
