using EtkBlazorApp.BL.CronTask;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL
{
    public class CronTaskService
    {
        public event Action<CronTaskEntity> OnTaskExecutionStart;
        public event Action<CronTaskEntity> OnTaskExecutionEnd;
        public event Action<CronTaskEntity> OnTaskExecutionSuccess;
        public event Action<CronTaskEntity> OnTaskExecutionError;
        public CronTaskEntity TaskInProgress { get; private set; }

        private readonly ICronTaskStorage cronTaskStorage;
        internal readonly IPriceListTemplateStorage templates;
        internal readonly SystemEventsLogger logger;
        internal readonly UpdateManager updateManager;
        internal readonly PriceListManager priceListManager;
        internal readonly RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory;

        private readonly Timer checkTimer;
        private readonly Dictionary<CronTaskBase, CronTaskEntity> tasks;
        private readonly List<CronTaskBase> inProgress;

        public CronTaskService(
            ICronTaskStorage cronTaskStorage,
            IPriceListTemplateStorage templates,
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

            tasks = new Dictionary<CronTaskBase, CronTaskEntity>();
            inProgress = new List<CronTaskBase>();

            checkTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();
        }

        public async Task ExecuteImmediately(int task_id)
        {            
            await RefreshTaskList(force: true);
            var kvp = tasks.FirstOrDefault(t => t.Key.TaskId == task_id);
            if (kvp.Equals(default(KeyValuePair<CronTaskBase, CronTaskEntity>))) { return; }

            if (!inProgress.Contains(kvp.Key))
            {
                await ExecuteTask(kvp.Key, kvp.Value);
            }
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await RefreshTaskList();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            foreach(var kvp in tasks)
            {
                if(IsTimeToRun(kvp.Value, currentTime) && !inProgress.Contains(kvp.Key))
                {
                    await ExecuteTask(kvp.Key, kvp.Value);
                }
            }
        }

        public async Task RefreshTaskList(bool force = false)
        {
            if (force == true || tasks.Count == 0)
            {
                tasks.Clear();
                inProgress.Clear();

                var items = await cronTaskStorage.GetCronTasks();
                foreach (var entity in items)
                {
                    var taskObject = CreateTask(entity.task_type_name, entity.linked_price_list_guid, entity.task_id);
                    tasks.Add(taskObject, entity);
                }
            }
        }

        private async Task ExecuteTask(CronTaskBase task, CronTaskEntity taskInfo)
        {
            inProgress.Add(task);
            OnTaskExecutionStart?.Invoke(taskInfo);
            TaskInProgress = taskInfo;

            var sw = Stopwatch.StartNew();
            bool exec_result = false;

            try
            {
                await Task.Run(async () => await task.Run());
                
                exec_result = true;

                sw.Stop();
                      
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Выполнено", $"Задание {taskInfo.name} выполнено. Длительность выполнения {(int)sw.Elapsed.TotalSeconds} сек.");
            }
            catch (Exception ex)
            {               
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{taskInfo.name}'. {ex.Message}");
                //...exec_result = false 
            }
            finally
            {
                inProgress.Remove(task);
                taskInfo.last_exec_date_time = DateTime.Now;
                taskInfo.last_exec_result = exec_result;
                await cronTaskStorage.SaveCronTaskExecResult(taskInfo);               
                TaskInProgress = null;

                OnTaskExecutionEnd?.Invoke(taskInfo);
                if (exec_result)
                {
                    OnTaskExecutionSuccess?.Invoke(taskInfo);
                }
                else
                {
                    OnTaskExecutionError?.Invoke(taskInfo);
                }
            }         
        }

        private bool IsTimeToRun(CronTaskEntity task, TimeSpan now)
        {
            if(now >= task.exec_time && Math.Abs((task.exec_time - now).TotalMilliseconds) <= checkTimer.Interval)
            {
                return true;
            }
            return false;
        }

        private CronTaskBase CreateTask(string taskTypeName, string parameter, int taskId)
        {
            Type linkedPriceListType = PriceListManager.GetPriceListTypeByGuid(parameter);

            //Таблица etk_app_cron_task_type
            switch (taskTypeName)
            {
                case "Одиночный прайс-лист":
                    return new BL.CronTask.CronTaskUsingRemotePriceList(linkedPriceListType, this, taskId);
            }

            throw new ArgumentException(taskTypeName + " не реализован");
        }
    }
}
