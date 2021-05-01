using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
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
        public Task<List<CronTaskTypeEntity>> GetCronTaskTypes();
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
            string sql = "SELECT ta.*, tp.name as task_type_name " +
                         "FROM etk_app_cron_task ta " +
                         "LEFT JOIN etk_app_cron_task_type tp ON (ta.task_type_id = tp.task_type_id)";

            var data = await database.LoadData<CronTaskEntity, dynamic>(sql, new { });

            return data;
        }

        public async Task<CronTaskEntity> GetCronTaskById(int id)
        {
            string sql = "SELECT ta.*, tp.name as task_type_name " +
                         "FROM etk_app_cron_task ta " +
                         "LEFT JOIN etk_app_cron_task_type tp ON (ta.task_type_id = tp.task_type_id) " +
                         "WHERE task_id = @id";

            var data = await database.LoadData<CronTaskEntity, dynamic>(sql, new { id });

            return data.FirstOrDefault();
        }

        public async Task UpdateCronTask(CronTaskEntity task)
        {
            string sql = "UPDATE etk_app_cron_task SET " +
                                                "enabled = @enabled, " +
                                                "exec_time = @exec_time, " +
                                                "last_exec_date_time = @last_exec_date_time, " +
                                                "last_exec_result = @last_exec_result " +
                                                "WHERE task_id = @task_id";

            await database.SaveData(sql, task);
        }

        public async Task CreateCronTask(CronTaskEntity task)
        {
            string sql = "INSERT INTO etk_app_cron_task (task_type_id, linked_price_list_guid, enabled, exec_time, name, description) VALUES " +
                                                       "(@task_type_id, @linked_price_list_guid, @enabled, @exec_time, @name, @description)";

            await database.SaveData(sql, task);

        }

        public async Task<List<CronTaskTypeEntity>> GetCronTaskTypes()
        {
            var data = await database.LoadData<CronTaskTypeEntity, dynamic>("SELECT * FROM etk_app_cron_task_type", new { });
            return data;
        }

        public async Task DeleteCronTask(int task_id)
        {
            await database.SaveData<dynamic>("DELETE FROM etk_app_cron_task WHERE task_id = @task_id", new { task_id });
        }
    }
}
