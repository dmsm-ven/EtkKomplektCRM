using Dapper;
using EtkBlazorApp.DataAccess.Model;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class MonobrandConnection
    {
        private readonly ShopAccountEntity connectionInfo;

        string ConnectionString
        {
            get
            {
                return $"Server=127.0.0.1;Database={connectionInfo.db_login};Port=3006;User Id={connectionInfo.db_login};Password={connectionInfo.db_password};";
            }
        }

        public MonobrandConnection(ShopAccountEntity connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public async Task UpdateProducts (List<ProductUpdateData> updateData, bool clearStock)
        {
            string sql = "";

            await ExecuteAsync(sql, null);
        }

        public async Task<List<ProductEntity>> ReadProducts()
        {
            string sql = "SELECT * FROM oc_product";

            var data = await QueryAsync<ProductEntity>(sql, null);

            return data;
        }

        private async Task<List<T>> QueryAsync<T>(string sql, object parameters)
        {
            var list = new List<T>();

            using (var client = new SshClient(connectionInfo.ftp_host, connectionInfo.ftp_login, connectionInfo.ftp_password))
            {
                client.Connect();

                var port = new ForwardedPortLocal("127.0.0.1", 3006, connectionInfo.uri, 3306);
                client.AddForwardedPort(port);

                port.Start();

                using (IDbConnection conn = new MySqlConnection(ConnectionString))
                {
                    var data = await conn.QueryAsync<T>(sql, parameters);
                    list.AddRange(data);
                }

                port.Stop();
                client.Disconnect();
            }

            return list;
        }

        private async Task ExecuteAsync(string sql, object parameters)
        {
            using (var client = new SshClient(connectionInfo.ftp_host, connectionInfo.ftp_login, connectionInfo.ftp_password))
            {
                client.Connect();

                var port = new ForwardedPortLocal("127.0.0.1", 3006, connectionInfo.uri, 3306);
                client.AddForwardedPort(port);

                port.Start();

                using (IDbConnection conn = new MySqlConnection(ConnectionString))
                {
                    await conn.ExecuteAsync(sql, parameters);
                }

                port.Stop();
                client.Disconnect();
            }
        }
    }
}
