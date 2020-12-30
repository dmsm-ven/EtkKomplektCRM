using Dapper;
using EtkBlazorApp.DataAccess.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IDatabaseAccess
    {        
        Task<List<T>> LoadData<T, U>(string sql, U parameters);
        Task<T> GetScalar<T, U>(string sql, U parameters);
        Task SaveData<T>(string sql, T parameters);
    }

    public class EtkDatabaseDapperAccess : IDatabaseAccess
    {
        private readonly IConfiguration configuration;
#if DEBUG
        string ConnectionString => configuration.GetConnectionString("local_server_db");
#else
        string ConnectionString => configuration.GetConnectionString("server_db");
#endif

        public EtkDatabaseDapperAccess(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                var rows = await connection.QueryAsync<T>(sql, parameters);

                return rows.ToList();
            }
        }

        public async Task<T> GetScalar<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                T value = await connection.ExecuteScalarAsync<T>(sql, parameters);

                return value;
            }
        }

        public Task SaveData<T>(string sql, T parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                return connection.ExecuteAsync(sql, parameters);
            }
        }
    }


}