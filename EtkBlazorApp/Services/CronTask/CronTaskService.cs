using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.Services
{
    public class CronTaskService
    {
        public event Action<CronTaskBase> OnTaskComplete;
        public event Action<CronTaskBase> OnTaskError;

        internal readonly ISettingStorage settings;
        internal readonly ITemplateStorage templates;
        internal readonly SystemEventsLogger logger;
        internal readonly UpdateManager updateManager;
        internal readonly PriceListManager priceListManager;

        private DateTime lastCheckDate;
        private readonly Timer checkTimer;
        private readonly List<CronTaskBase> taskList;

        public CronTaskService(
            ISettingStorage settings,
            ITemplateStorage templates,
            SystemEventsLogger logger,
            UpdateManager updateManager, 
            PriceListManager priceListManager)
        {
            this.settings = settings;
            this.templates = templates;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;

            taskList = Assembly.GetAssembly(typeof(CronTaskBase)).GetTypes()
                .Where(tt => tt.IsClass && !tt.IsAbstract && tt.IsSubclassOf(typeof(CronTaskBase)))
                .Select(tt => (CronTaskBase)Activator.CreateInstance(tt))
                .ToList();

            taskList.ForEach(t => t.SetService(this));

            checkTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();
        }

        public async Task ExecuteImmediately(int task_id)
        {
            var task = taskList.FirstOrDefault(t => t.Prefix == (CronTaskPrefix)task_id);
            if(task != null)
            {
                await ExecuteTask(task, DateTime.Now.TimeOfDay, forceRun: true);
            }
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetIfNewDay();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            foreach(var task in taskList)
            {
                if (task.IsDoneToday) { continue; }

                await ExecuteTask(task, currentTime);           
            }

        }

        private async Task ExecuteTask(CronTaskBase task, TimeSpan startTime, bool forceRun = false)
        {
            var taskDatabaseEntity = await settings.GetCronTaskById((int)task.Prefix);

            if(taskDatabaseEntity.enabled == false && forceRun == false) { return; }

            if (IsTimeToRun(taskDatabaseEntity, startTime) || forceRun)
            {
                var sw = Stopwatch.StartNew();

                try
                {
                    taskDatabaseEntity.last_exec_date_time = DateTime.Now;
                    await task.Execute();
                    OnTaskComplete?.Invoke(task);
                    await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Успех", $"Задание {task.Prefix} выполнено");
                    
                }
                catch (Exception ex)
                {                    
                    OnTaskError?.Invoke(task);
                    await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{task.Prefix}'. {ex.Message}");
                }                 

                await settings.UpdateCronTask(taskDatabaseEntity);                                            
            }
        }

        private void ResetIfNewDay()
        {
            if (lastCheckDate > DateTime.Now.Date)
            {
                taskList.ForEach(t => t.Reset());
            }
            lastCheckDate = DateTime.Now.Date;
        }

        private bool IsTimeToRun(CronTaskEntity task, TimeSpan now)
        {
            if(Math.Abs((task.exec_time - now).TotalMilliseconds) <= checkTimer.Interval)
            {
                return true;
            }
            return false;
        }
    }
}
