using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IUserService
    {
        Task UpdateUserLastLoginDate(string login);

        Task<List<AppUserEntity>> GetUsers();
        Task<AppUserEntity> GetUser(string login, string password);
        Task UpdateUser(AppUserEntity user);
        Task AddUser(AppUserEntity user);
        Task DeleteUser(int user_id);
        Task<List<AppUserGroupEntity>> GetUserGroups();

    }
}
