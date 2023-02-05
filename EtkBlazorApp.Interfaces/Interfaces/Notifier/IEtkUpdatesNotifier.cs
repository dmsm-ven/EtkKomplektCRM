using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface IEtkUpdatesNotifier
{
    Task NotifyPriceListProductPriceChanged(PriceListProductPriceChangeHistory data);
    Task NotifyPriceListLoadingError(string taskName);
    Task NotifOrderStatusChanged(int? EtkOrderId, string cdekOrderId, string statusName);
}
