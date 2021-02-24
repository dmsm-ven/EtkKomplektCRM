using EtkBlazorApp.DataAccess;
using System;
using System.Threading.Tasks;
using EtkBlazorApp.ImapClient;
using System.IO;
using EtkBlazorApp.DataAccess.Entity;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp.BL.Managers
{
    public class TaskScheduleManager
    {
        private readonly ISettingStorage settings;
        private readonly ILogStorage logger;
        private readonly UpdateManager updateManager;
        private readonly PriceListManager priceListManager;

        DateTime lastCheckDate;
        private readonly Timer checkTimer;
        private readonly List<ScheduleTaskBase> taskList;

        public bool IsEnabled => checkTimer.Enabled;

        public TaskScheduleManager(ISettingStorage settings, ILogStorage logger, UpdateManager updateManager, PriceListManager priceListManager)
        {
            this.settings = settings;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;

            taskList = Assembly.GetAssembly(typeof(ScheduleTaskBase)).GetTypes()
                .Where(tt => tt.IsClass && !tt.IsAbstract && tt.IsSubclassOf(typeof(ScheduleTaskBase)))
                .Select(tt => (ScheduleTaskBase)Activator.CreateInstance(tt))
                .ToList();

            checkTimer = new Timer(TimeSpan.FromSeconds(45).TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();
        }

        public async Task ExecuteImmediately(ScheduleTask name)
        {
            var task = taskList.FirstOrDefault(t => t.Name == name);
            if(task != null)
            {
                await ExecuteTask(task, DateTime.Now.TimeOfDay);
            }
        }

        //TODO тут возможна 'ошибка', что если сменили время задания на более позднее, а сегодня оно уже выполнялось, то оно не выполнится еще раз
        //и возможно стоит заменить bool на enum (done, not exected, inprogress)
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

        private async Task ExecuteTask(ScheduleTaskBase task, TimeSpan startTime)
        {
            string settingsPrefix = task.Name.ToString().ToLower();
            var isEnabled = await settings.GetValue<bool>($"task_{settingsPrefix}_active");
            if (isEnabled)
            {
                TimeSpan taskCheckTime = await settings.GetValue<TimeSpan>("task_{settingsPrefix}_exec_time");
                if (IsTimeToRun(taskCheckTime, startTime))
                {
                    bool isDone = await task.Execute(logger);
                    if (isDone)
                    {
                        await settings.SetValue($"task_{settingsPrefix}_last_exec_date_time", DateTime.Now);
                    }
                }
            }
        }

        /// <summary>
        /// Очищаем статус выполнения если настал новый день
        /// </summary>
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
