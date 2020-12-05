using EtkBlazorApp.DataAccess.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IDatabase
    {
        Task SaveManufacturer(ManufacturerModel manufacturer);
        Task<List<ManufacturerModel>> GetManufacturers();
        Task<List<OrderModel>> GetLastOrders(int count);
        Task<bool> GetUserPremission(string login, string password);
    }
}