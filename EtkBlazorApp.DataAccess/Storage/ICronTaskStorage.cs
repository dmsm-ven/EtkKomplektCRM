using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ICronTaskStorage
    {
        public Task CreateCronTask(CronTaskEntity task);
        public Task<List<CronTaskEntity>> GetCronTasks();
        public Task<CronTaskEntity> GetCronTaskById(int id);
        public Task DeleteCronTask(int id);
        public Task UpdateCronTask(CronTaskEntity task);
        public Task SaveCronTaskExecResult(CronTaskEntity task);
        public Task<List<CronTaskTypeEntity>> GetCronTaskTypes();
        public Task<List<CronTaskHistoryEntity>> GetCronTaskHistoryInfo(int month, int year);
        public Task SetTaskExecStatus(int id, CronTaskExecResult status);
    }

    public class CronTaskStorage : ICronTaskStorage
    {
        private readonly IDatabaseAccess database;

        public CronTaskStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<CronTaskEntity>> GetCronTasks()
        {
            string sql = @"SELECT ta.*, tp.name as task_type_name
                           FROM etk_app_cron_task ta
                           LEFT JOIN etk_app_cron_task_type tp ON (ta.task_type_id = tp.task_type_id)
                           WHERE archived = 0";

            var data = await database.GetList<CronTaskEntity>(sql);
            return data;
        }

        public async Task<CronTaskEntity> GetCronTaskById(int id)
        {
            string sql = @"SELECT ta.*, tp.name as task_type_name
                           FROM etk_app_cron_task ta
                           LEFT JOIN etk_app_cron_task_type tp ON (ta.task_type_id = tp.task_type_id)
                           WHERE task_id = @id";

            var task = (await database.GetList<CronTaskEntity, dynamic>(sql, new { id })).FirstOrDefault();
            return task;
        }

        public async Task UpdateCronTask(CronTaskEntity task)
        {
            string sql = @"UPDATE etk_app_cron_task 
                           SET enabled = @enabled,
                               description = @description,
                               exec_time = @exec_time,
                               linked_price_list_guid = @linked_price_list_guid,
                               additional_exec_time = @additional_exec_time
                           WHERE task_id = @task_id";

            await database.ExecuteQuery(sql, task);
        }

        public async Task SaveCronTaskExecResult(CronTaskEntity task)
        {
            string mainTableSql = @"UPDATE etk_app_cron_task
                           SET last_exec_date_time = @last_exec_date_time,
                               last_exec_result = @last_exec_result,
                               last_exec_file_size = @last_exec_file_size
                           WHERE task_id = @task_id";

            await database.ExecuteQuery(mainTableSql, task);

            string historyTableSql = @"INSERT INTO etk_app_cron_task_history (task_id, date_time, exec_result) 
                                                                     VALUES (@task_id, @last_exec_date_time, @last_exec_result)";

            await database.ExecuteQuery(historyTableSql, task);
        }

        public async Task CreateCronTask(CronTaskEntity task)
        {
            string sql = @"INSERT INTO etk_app_cron_task (task_type_id, linked_price_list_guid, enabled, exec_time, additional_exec_time, name, description) 
                           VALUES
                          (@task_type_id, @linked_price_list_guid, @enabled, @exec_time, @additional_exec_time, @name, @description)";

            await database.ExecuteQuery(sql, task);
        }

        public async Task<List<CronTaskTypeEntity>> GetCronTaskTypes()
        {
            var data = await database.GetList<CronTaskTypeEntity>("SELECT * FROM etk_app_cron_task_type ORDER BY task_type_id");
            return data;
        }

        public async Task DeleteCronTask(int task_id)
        {
            await database.ExecuteQuery<dynamic>("UPDATE etk_app_cron_task SET archived = 1 WHERE task_id = @task_id", new { task_id });
        }

        public async Task<List<CronTaskHistoryEntity>> GetCronTaskHistoryInfo(int month, int year)
        {
            string sql = @"SELECT h.*, t.name as name
                           FROM etk_app_cron_task_history h
                           JOIN etk_app_cron_task t ON h.task_id = t.task_id
                           WHERE MONTH(h.date_time) = @month AND YEAR(h.date_time) = @year";

            var data = await database.GetList<CronTaskHistoryEntity, dynamic>(sql, new { month, year });
            return data;
        }

        public async Task SetTaskExecStatus(int task_id, CronTaskExecResult exec_result)
        {
            string sql_1 = @"UPDATE etk_app_cron_task SET last_exec_result = @exec_result WHERE task_id = @task_id";
            await database.ExecuteQuery(sql_1, new { task_id, exec_result = (int)exec_result });

            string sql_2 = @"UPDATE etk_app_cron_task_history SET last_exec_result = @exec_result WHERE task_id = @task_id";
            await database.ExecuteQuery(sql_2, new { task_id, exec_result = (int)exec_result });
        }
    }
}
