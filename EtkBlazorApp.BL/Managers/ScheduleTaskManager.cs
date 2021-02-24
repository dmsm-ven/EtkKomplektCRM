using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL.Managers
{
    public class ScheduleTaskManager
    {
        public event Action<Tuple<ScheduleTaskBase, bool>> TaskProcessed;

        internal readonly ISettingStorage settings;
        internal readonly ILogStorage logger;
        internal readonly UpdateManager updateManager;
        internal readonly PriceListManager priceListManager;

        private DateTime lastCheckDate;
        private readonly Timer checkTimer;
        private readonly List<ScheduleTaskBase> taskList;

        public ScheduleTaskManager(
            ISettingStorage settings, 
            ILogStorage logger,
            UpdateManager updateManager, 
            PriceListManager priceListManager)
        {
            this.settings = settings;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;

            taskList = Assembly.GetAssembly(typeof(ScheduleTaskBase)).GetTypes()
                .Where(tt => tt.IsClass && !tt.IsAbstract && tt.IsSubclassOf(typeof(ScheduleTaskBase)))
                .Select(tt => (ScheduleTaskBase)Activator.CreateInstance(tt))
                .ToList();

            taskList.ForEach(t => t.SetManager(this));

            checkTimer = new Timer(TimeSpan.FromSeconds(45).TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();
        }

        public async Task ExecuteImmediately(ScheduleTask name)
        {
            var task = taskList.FirstOrDefault(t => t.Name == name);
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

        private async Task ExecuteTask(ScheduleTaskBase task, TimeSpan startTime, bool forceRun = false)
        {
            string settingsPrefix = task.Name.ToString().ToLower();
            var isEnabled = await settings.GetValue<bool>($"task_{settingsPrefix}_active");

            if(!isEnabled && !forceRun) { return; }

            TimeSpan taskCheckTime = await settings.GetValue<TimeSpan>($"task_{settingsPrefix}_exec_time");
            if (IsTimeToRun(taskCheckTime, startTime))
            {
                bool isDone = false;

                var logEntry = new LogEntryEntity()
                {
                    group_name = LogEntryGroupName.PereodicTask,
                    message = "Обновления товаров из прайс-листа Symmetron",
                    date_time = DateTime.Now,
                    user = "Система"
                };

                var sw = Stopwatch.StartNew();

                try
                {
                    await task.Execute();

                    logEntry.title = $"{task.Name} выполнено успешно";
                    logEntry.message += $" Выполнено за: {sw.Elapsed.TotalSeconds} секунд.";

                    isDone = true;

                }
                catch (Exception ex)
                {
                    logEntry.title = $"{task.Name} ошибка выполнения";
                    logEntry.message += $" Ошибка: {ex.Message}";
                }                 

                if (isDone)
                {
                    await settings.SetValue($"task_{settingsPrefix}_last_exec_date_time", DateTime.Now);
                }

                await logger.Write(logEntry);
                TaskProcessed?.Invoke(Tuple.Create(task, isDone));
                
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

        private bool IsTimeToRun(TimeSpan taskTime, TimeSpan now)
        {
            return now.Hours == taskTime.Hours && now.Minutes == taskTime.Minutes;
        }
    }
}
