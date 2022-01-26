using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class AuthenticationHMACSHA256DataStorage : IAuthenticationDataStorage
    {
        private readonly IDatabaseAccess database;

        public AuthenticationHMACSHA256DataStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<string> GetUserPermission(string login, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<AppUserEntity> GetUser(string login, string password)
        {                        
            string sql = @"SELECT u.*, g.name as group_name, g.permission
                           FROM etk_app_user u
                           JOIN etk_app_user_group g ON u.user_group_id = g.user_group_id
                           WHERE login = @login";

            var dbUserData = await database.GetFirstOrDefault<AppUserEntity, dynamic>(sql, new { login, password });

            if (CheckHash(password, dbUserData.password, dbUserData.salt))
            {
                return dbUserData;
            }

            return null;
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
            string newPasswordSalt = null;

            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_user")
                .AppendLine("SET user_group_id = @user_group_id,")
                .AppendLine("ip = @ip,")
                .AppendLine("status = @status");

            if (user.password != null)
            {
                newPasswordSalt = Convert.ToBase64String(GetRandomSalt());
                newPassword = GetPasswordHash(user.password, newPasswordSalt);               
                sb.AppendLine(", password = @password");
                sb.AppendLine(", salt = @salt");
            }

            sb.Append("WHERE user_id = @user_id");

            string sql = sb.ToString();

            await database.ExecuteQuery<dynamic>(sql, new
            {
                user_id = user.user_id,
                user_group_id = user.user_group_id,
                salt = newPasswordSalt,
                password = newPassword,
                ip = user.ip,
                status = user.status
            });
        }

        public async Task AddUser(AppUserEntity user)
        {
            var salt = Convert.ToBase64String(GetRandomSalt());
            var password = GetPasswordHash(user.password, salt);

            string sql = @"INSERT INTO etk_app_user (login, password, salt, ip, user_group_id, status) VALUES 
                                                    (@login, @password, @salt, @ip, @user_group_id, '1')";
            await database.ExecuteQuery<dynamic>(sql, new
            {
                login = user.login,
                password,
                salt,
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

        private bool CheckHash(string clientPassword, string storedHash, string salt)
        {
            string calculatedHash = GetPasswordHash(clientPassword, salt);
            return storedHash == calculatedHash;
        }

        private string GetPasswordHash(string password, string salt)
        {
            string hash = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Convert.FromBase64String(salt),
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 50000,
                    numBytesRequested: 8));

            return hash;
        }

        private byte[] GetRandomSalt()
        {
            byte[] saltBytes = new byte[8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(saltBytes);
            }

            return saltBytes;
        }
    }
}
