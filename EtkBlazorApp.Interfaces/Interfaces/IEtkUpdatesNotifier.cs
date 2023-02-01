using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface IEtkUpdatesNotifier
{
    Task NotifyPriceListProductPriceChanged(PriceListProductPriceChangeHistory data);
}
