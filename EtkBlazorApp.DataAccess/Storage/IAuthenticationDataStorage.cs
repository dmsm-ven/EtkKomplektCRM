using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IAuthenticationDataStorage
    {
        Task<string> GetUserPermission(string login, string password);
        Task UpdateUserLastLoginDate(string login);

        Task<List<AppUserEntity>> GetUsers();
        Task UpdateUser(AppUserEntity user);
        Task AddUser(AppUserEntity user);
        Task DeleteUser(int user_id);
        Task<List<AppUserGroupEntity>> GetUserGroups();
    }

    //TODO: поменять MD5 на HMACSHA256 + salt
    //https://docs.microsoft.com/ru-ru/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-6.0
    public class AuthenticationDataStorage : IAuthenticationDataStorage
    {
        private readonly IDatabaseAccess database;

        public AuthenticationDataStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<string> GetUserPermission(string login, string password)
        {
            var sql = @"SELECT permission
                        FROM etk_app_user u
                        LEFT JOIN etk_app_user_group g ON u.user_group_id = g.user_group_id
                        WHERE u.status = 1 AND login = @login AND password = MD5(@password)";

            var permission = await database.GetScalar<string, dynamic>(sql, new { login, password });

            return permission;
        }

        public async Task UpdateUserLastLoginDate(string login)
        {
            string sql = "UPDATE etk_app_user SET last_login_date = NOW() WHERE login = @login";

            await database.ExecuteQuery<dynamic>(sql, new { login });
        }

        public async Task<List<AppUserEntity>> GetUsers()
        {
            string sql = @"SELECT u.*,  g.name as group_name
                           FROM etk_app_user u
                           JOIN etk_app_user_group g ON u.user_group_id = g.user_group_id";

            var users = await database.GetList<AppUserEntity, dynamic>(sql, new { });

            return users;
        }

        public async Task UpdateUser(AppUserEntity user)
        {
            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_user")
                .AppendLine("SET user_group_id = @user_group_id,")
                .AppendLine("salt = @salt, ")
                .AppendLine("ip = @ip,")
                .AppendLine("status = @status");

            if(user.password != null)
            {
                sb.AppendLine(", password = MD5(@password)");
            }

            sb.Append("WHERE user_id = @user_id");

            string sql = sb.ToString();

            await database.ExecuteQuery<dynamic>(sql, user);
        }

        public async Task AddUser(AppUserEntity user)
        {
            string sql = @"INSERT INTO etk_app_user (login, password, salt, ip, user_group_id, status) VALUES 
                                                    (@login, MD5(@password), @salt, @ip, @user_group_id, '1')";
            await database.ExecuteQuery<dynamic>(sql, user);

        }

        public async Task DeleteUser(int user_id)
        {
            await database.ExecuteQuery<dynamic>("DELETE FROM etk_app_user WHERE user_id = @user_id", new { user_id });
        }

        public async Task<List<AppUserGroupEntity>> GetUserGroups()
        {
            var groups = await database.GetList<AppUserGroupEntity>("SELECT * FROM etk_app_user_group");
            return groups;
        }
    }
}
