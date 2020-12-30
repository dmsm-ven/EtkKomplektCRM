using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IAuthStateProcessor
    {
        Task<int> GetUserBadPasswordTryCounter(string login);
        Task<string> GetUserPermission(string login, string password);
        Task SetUserBadPasswordTryCounter(string login, int tryCount);
    }

    public class MyAuthStateProcessor : IAuthStateProcessor
    {
        private readonly IDatabaseAccess database;

        public MyAuthStateProcessor(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<int> GetUserBadPasswordTryCounter(string login)
        {
            dynamic param = new { login };

            var sb = new StringBuilder()
                .AppendLine(" SELECT IF( EXISTS(SELECT * FROM etk_app_ban_list WHERE login = @login),")
                .AppendLine("(SELECT current_try_counter FROM etk_app_ban_list WHERE login = @login), ")
                .AppendLine("-1)");

            string sql = sb.ToString().Trim();
            int tryCount = await database.GetScalar<int, dynamic>(sql, param);

            if (tryCount == -1)
            {
                await database.SaveData<dynamic>("INSERT INTO etk_app_ban_list (login) VALUES (@login)", param);
                tryCount = 0;
            }

            return tryCount;


        }

        public async Task<string> GetUserPermission(string login, string password)
        {
            var sb = new StringBuilder()
                .AppendLine("SELECT permission")
                .AppendLine("FROM etk_app_user u")
                .AppendLine("LEFT JOIN etk_app_user_group g ON u.user_group_id = g.user_group_id")
                .AppendLine("WHERE u.status = 1 AND login = @login AND password = @password");

            var sql = sb.ToString().Trim();

            string passwordMd5 = CreateFromStringMD5(password);

            var permission = await database.GetScalar<string, dynamic>(sql, new { login, password = passwordMd5 });

            return permission;
        }

        public async Task SetUserBadPasswordTryCounter(string login, int tryCount)
        {
            string sql = "UPDATE etk_app_ban_list SET current_try_counter = @tryCount, last_access = NOW() WHERE login = @login";

            await database.SaveData<dynamic>(sql, new { tryCount, login });
        }

        private string CreateFromStringMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
