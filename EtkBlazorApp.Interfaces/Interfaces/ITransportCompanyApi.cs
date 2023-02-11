using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Data.Cdek;

namespace EtkBlazorApp.Core.Interfaces;

public interface ITransportCompanyApi
{
    Task<CdekOrderInfo> GetOrderInfo(string orderId);
    string GetOrderDetailsLink(string orderId);
    TransportDeliveryCompany Prefix { get; }
}
