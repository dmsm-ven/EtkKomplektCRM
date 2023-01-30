using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ILogStorage
    {
        Task Write(LogEntryEntity logEntry);
        Task<List<LogEntryEntity>> GetLogItems(int count, int maxDaysOld, string selectedUser, string selectedGroup);

        Task<List<AppUpdateHistoryEntity>> GetAppUpdateInfo(int count);
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

        public async Task<List<LogEntryEntity>> GetLogItems(int count, int maxDaysOld, string selectedUser = null, string selectedGroup = null)
        {
            var sb = new StringBuilder("SELECT * FROM etk_app_log\n");

            bool whereAdded = false;

            if (maxDaysOld > 0)
            {
                maxDaysOld *= -1;
                sb.AppendLine("WHERE DATE(date_time) >= ADDDATE(NOW(), INTERVAL @maxDaysOld DAY)");
                whereAdded = true;
            }
            else if (maxDaysOld < 0)
            {
                sb.AppendLine("WHERE DATE(date_time) = DATE(ADDDATE(NOW(), INTERVAL @maxDaysOld DAY))");
                whereAdded = true;
            }

            if (selectedUser != null)
            {
                if (!whereAdded)
                {
                    sb.AppendLine("WHERE user = @user");
                    whereAdded = true;
                }
                else
                {
                    sb.AppendLine(" AND user = @user");
                }

            }

            if (selectedGroup != null)
            {
                if (!whereAdded)
                {
                    sb.AppendLine("WHERE group_name = @group");
                    whereAdded = true;
                }
                else
                {
                    sb.AppendLine(" AND group_name = @group");
                }

            }

            sb.Append("ORDER BY date_time DESC LIMIT @limit");

            string sql = sb.ToString();
            var parameters = new
            {
                limit = count,
                maxDaysOld,
                user = selectedUser,
                group = selectedGroup
            };

            var data = await database.GetList<LogEntryEntity, dynamic>(sql, parameters);

            return data;
        }

        public async Task<List<AppUpdateHistoryEntity>> GetAppUpdateInfo(int limit)
        {
            string sql = "SELECT * FROM etk_app_update_history ORDER BY id DESC LIMIT @limit";

            var items = await database.GetList<AppUpdateHistoryEntity, dynamic>(sql, new { limit });

            return items;
        }
    }

}
