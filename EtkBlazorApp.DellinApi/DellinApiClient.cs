using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.Core.Interfaces;

namespace EtkBlazorApp.DellinApi;

public class DellinApiClient : ITransportCompanyApi
{
    private readonly string apiKey;
    private readonly HttpClient httpClient;

    public TransportDeliveryCompany Prefix => TransportDeliveryCompany.Dellin;

    public DellinApiClient(string apiKey, HttpClient httpClient)
    {
        this.apiKey = apiKey;
        this.httpClient = httpClient;
    }

    public string GetOrderDetailsLink(string orderId)
    {
        return $"https://www.dellin.ru/cabinet/orders/{orderId}/";
    }

    public Task<CdekOrderInfo> GetOrderInfo(string orderId)
    {
        return Task.FromResult(new CdekOrderInfo() { comment = "Не реализовано" });
    }
}
