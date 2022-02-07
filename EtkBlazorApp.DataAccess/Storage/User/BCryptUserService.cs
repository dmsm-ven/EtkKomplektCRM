using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class BCryptUserService : IUserService
    {
        private readonly IDatabaseAccess database;

        public BCryptUserService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<AppUserEntity> GetUser(string login, string password)
        {
            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
            {
                string sql = @"SELECT u.*, g.name as group_name, g.permission
                           FROM etk_app_user u
                           JOIN etk_app_user_group g ON u.user_group_id = g.user_group_id
                           WHERE login = @login";

                var dbUserData = await database.GetFirstOrDefault<AppUserEntity, dynamic>(sql, new { login });

                try
                {
                    if (BCryptNet.Verify(password, dbUserData.password))
                    {
                        return dbUserData;
                    }
                }
                catch
                {

                }
            }

            return new AppUserEntity();
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
            string newPassword = null;

            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_user")
                .AppendLine("SET user_group_id = @user_group_id,")
                .AppendLine("ip = @ip,")
                .AppendLine("status = @status");

            if (user.password != null)
            {
                newPassword = BCryptNet.HashPassword(user.password);
                sb.AppendLine(", password = @password");
            }

            sb.Append("WHERE user_id = @user_id");

            string sql = sb.ToString();

            await database.ExecuteQuery<dynamic>(sql, new
            {
                user_id = user.user_id,
                user_group_id = user.user_group_id,
                password = newPassword,
                ip = user.ip,
                status = user.status
            });
        }

        public async Task AddUser(AppUserEntity user)
        {
            var password = BCryptNet.HashPassword(user.password);

            string sql = @"INSERT INTO etk_app_user (login, password, ip, user_group_id, status) VALUES 
                                                    (@login, @password, @ip, @user_group_id, '1')";
            await database.ExecuteQuery<dynamic>(sql, new
            {
                login = user.login,
                password,
                ip = user.ip,
                user_group_id = user.user_group_id
            });

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
