using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class MD5UserService : IUserService
    {
        private readonly IDatabaseAccess database;

        public MD5UserService(IDatabaseAccess database)
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

        public async Task<AppUserEntity> GetUser(string login, string password)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUser(AppUserEntity user)
        {
            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_user")
                .AppendLine("SET user_group_id = @user_group_id,")
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
            string sql = @"INSERT INTO etk_app_user (login, password, ip, user_group_id, status) VALUES 
                                                    (@login, MD5(@password), @ip, @user_group_id, '1')";
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
