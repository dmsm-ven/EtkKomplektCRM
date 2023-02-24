namespace EtkBlazorApp.Core.Interfaces;

public interface ICustomerOrderNotificator
{
    Task NotifyCustomer(long order_id, string customerEmail);
}