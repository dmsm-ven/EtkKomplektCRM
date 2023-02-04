using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface IEtkUpdatesNotifier
{
    Task NotifyPriceListProductPriceChanged(PriceListProductPriceChangeHistory data);
    Task NotifyPriceListLoadingError(string taskName);
    Task NotifOrderStatusChanged(int order_id, string statusName);
}
