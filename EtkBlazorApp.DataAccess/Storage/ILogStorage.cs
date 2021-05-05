using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ILogStorage
    {
        Task Write(LogEntryEntity logEntry);
        Task<List<LogEntryEntity>> GetLogItems(int count, int maxDaysOld);
    }

    public class LogStorage : ILogStorage
    {
        private readonly IDatabaseAccess database;

        public LogStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task Write(LogEntryEntity entry)
        {
            string sql = @"INSERT INTO etk_app_log (user, group_name, date_time, title, message) VALUES
                          (@user, @group_name, @date_time, @title, @message)";

            await database.ExecuteQuery(sql, entry);
        }

        public async Task<List<LogEntryEntity>> GetLogItems(int count, int maxDaysOld)
        {
            string sql = "SELECT * FROM etk_app_log ORDER BY date_time DESC LIMIT @limit";

            if (maxDaysOld > 0)
            {
                maxDaysOld *= -1;
                sql = sql.Insert(sql.IndexOf("ORDER BY"), "WHERE DATE(date_time) >= ADDDATE(NOW(), INTERVAL @maxDaysOld DAY) ");
            }
            else if (maxDaysOld < 0)
            {
                sql = sql.Insert(sql.IndexOf("ORDER BY"), "WHERE DATE(date_time) = DATE(ADDDATE(NOW(), INTERVAL @maxDaysOld DAY)) ");
            }
            var data = await database.GetList<LogEntryEntity, dynamic>(sql, new { limit = count, maxDaysOld });
            return data.ToList();
        }
    }

}
