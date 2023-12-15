namespace EtkBlazorApp.Core.Interfaces;

public interface ICustomerOrderNotificator
{
    Task<bool> NotifyCustomer(long order_id, string customerEmail);
}