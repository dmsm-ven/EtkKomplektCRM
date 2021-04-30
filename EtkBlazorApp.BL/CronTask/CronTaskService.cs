using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL.CronTask
{
    public class CronTaskService
    {
        public event Action<CronTaskBase> OnTaskComplete;
        public event Action<CronTaskBase> OnTaskExecutionError;

        private readonly ICronTaskStorage cronTaskStorage;
        internal readonly ITemplateStorage templates;
        internal readonly SystemEventsLogger logger;
        internal readonly UpdateManager updateManager;
        internal readonly PriceListManager priceListManager;
        internal readonly RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory;

        private DateTime lastCheckDate;
        private readonly Timer checkTimer;
        private readonly List<CronTaskBase> tasks;
        private readonly Dictionary<CronTaskBase, bool> isDoneToday;

        public CronTaskService(
            ICronTaskStorage cronTaskStorage,
            ITemplateStorage templates,
            SystemEventsLogger logger,
            UpdateManager updateManager, 
            PriceListManager priceListManager,
            RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory)
        {
            this.cronTaskStorage = cronTaskStorage;
            this.templates = templates;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;
            this.remoteTemplateLoaderFactory = remoteTemplateLoaderFactory;

            tasks = new List<CronTaskBase>();

            isDoneToday = tasks.ToDictionary(t => t, state => false);

            checkTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();
        }

        public async Task ExecuteImmediately(int task_id)
        {
            var task = tasks.FirstOrDefault(t => t.TaskId == task_id);
            if(task != null)
            {
                await ExecuteTask(task, DateTime.Now.TimeOfDay, forceRun: true);
            }
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetIfNewDay();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            foreach(var task in tasks)
            {
                if (isDoneToday[task]) { continue; }

                await ExecuteTask(task, currentTime);           
            }
        }

        private async Task ExecuteTask(CronTaskBase task, TimeSpan startTime, bool forceRun = false)
        {
            var taskDatabaseEntity = await cronTaskStorage.GetCronTaskById(task.TaskId);

            if(taskDatabaseEntity == null || (!taskDatabaseEntity.enabled && !forceRun)) { return; }

            if (IsTimeToRun(taskDatabaseEntity, startTime) || forceRun)
            {
                var sw = Stopwatch.StartNew();

                try
                {
                    taskDatabaseEntity.last_exec_date_time = DateTime.Now;
                    isDoneToday[task] = true;

                    await task.Run();

                    sw.Stop();
                    OnTaskComplete?.Invoke(task);
                    await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Выполнено", $"Задание {taskDatabaseEntity.name} выполнено. Длительность выполнения {(int)sw.Elapsed.TotalSeconds} сек.");
                    
                }
                catch (Exception ex)
                {                    
                    OnTaskExecutionError?.Invoke(task);
                    await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{taskDatabaseEntity.name}'. {ex.Message}");
                }                 

                await cronTaskStorage.UpdateCronTask(taskDatabaseEntity);                                            
            }
        }

        private void ResetIfNewDay()
        {
            if (lastCheckDate != DateTime.Now.Date)
            {
                foreach(var task in tasks)
                {
                    isDoneToday[task] = false;
                }
            }
            lastCheckDate = DateTime.Now.Date;
        }

        private bool IsTimeToRun(CronTaskEntity task, TimeSpan now)
        {
            if(now >= task.exec_time && Math.Abs((task.exec_time - now).TotalMilliseconds) <= checkTimer.Interval)
            {
                return true;
            }
            return false;
        }
    }
}
