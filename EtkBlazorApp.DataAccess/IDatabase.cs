using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IDatabase
    {
        Task SaveManufacturer(ManufacturerModel manufacturer);
        Task<List<ManufacturerModel>> GetManufacturers();
        Task<string> GetUserPremission(string login, string password);
    }
}