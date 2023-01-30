namespace EtkBlazorApp.Core.Interfaces;

public interface ITransportCompanyApi
{
    Task<dynamic> GetOrderInfo(string orderId);
}
