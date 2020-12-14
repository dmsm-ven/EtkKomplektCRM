using EtkBlazorApp.DataAccess.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IDatabase
    {
        Task SaveManufacturer(ManufacturerEntity manufacturer);
        Task<List<ManufacturerEntity>> GetManufacturers();
        
        Task<List<OrderEntity>> GetLastOrders(int limit, string city = null);
        Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId);
        Task<OrderEntity> GetOrderById(int orderId);

        Task SaveShopAccount(ShopAccountEntity shopAccount);
        Task<List<ShopAccountEntity>> GetShopAccounts();
        Task DeleteShopAccounts(int id);

        Task<List<ProductEntity>> GetLastAddedProducts(int count);

        Task<string> GetUserPermission(string login, string password);
    }
}