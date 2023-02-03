using EtkBlazorApp.BL.CronTask;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL
{
    //TODO: Переделать - заместо ручного выполнения по таймеру на библиотеку Hangfire

    /// <summary>
    /// Выполняет переодический опрос удаленных поставщиков для загрузки их прайс-листов
    /// </summary>
    public class CronTaskService
    {
        // event'ы для привязки на страницах, что бы можно было в любом месте приложения получить уведомление, например через Toasts
        // TODO: переделать из event в какой-то другой вариант

        public event Action<CronTaskEntity> OnTaskExecutionStart;
        public event Action<CronTaskEntity> OnTaskExecutionEnd;
        public event Action<CronTaskEntity> OnTaskExecutionSuccess;
        public event Action<CronTaskEntity> OnTaskExecutionError;

        public CronTaskEntity TaskInProgress { get; private set; }

        private readonly ICronTaskStorage cronTaskStorage;
        internal readonly IPriceListTemplateStorage templates;
        internal readonly IEtkUpdatesNotifier notifier;
        internal readonly SystemEventsLogger logger;
        internal readonly ProductsPriceAndStockUpdateManager updateManager;
        internal readonly PriceListManager priceListManager;
        internal readonly RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory;
        private readonly Timer checkTimer;
        private readonly Dictionary<CronTaskBase, CronTaskEntity> tasks;
        private readonly List<CronTaskBase> inProgress;

        public CronTaskService(
            ICronTaskStorage cronTaskStorage,
            IPriceListTemplateStorage templates,
            IEtkUpdatesNotifier notifier,
            SystemEventsLogger logger,
            ProductsPriceAndStockUpdateManager updateManager,
            PriceListManager priceListManager,
            RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory)
        {
            this.cronTaskStorage = cronTaskStorage;
            this.templates = templates;
            this.notifier = notifier;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;
            this.remoteTemplateLoaderFactory = remoteTemplateLoaderFactory;
            tasks = new Dictionary<CronTaskBase, CronTaskEntity>();
            inProgress = new List<CronTaskBase>();

            //TODO: Убрать хардкод таймера
            checkTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                checkTimer.Elapsed += CheckTimer_Elapsed;
                checkTimer.Start();
            }
        }


        /// <summary>
        /// Ручной запуск задачи вне таймера
        /// </summary>
        /// <param name="task_id"></param>
        /// <returns></returns>
        public async Task ExecuteForced(int task_id)
        {
            await RefreshTaskList(force: true);
            var kvp = tasks.FirstOrDefault(t => t.Key.TaskId == task_id);

            if (kvp.Equals(default(KeyValuePair<CronTaskBase, CronTaskEntity>))) { return; }

            if (!inProgress.Contains(kvp.Key))
            {
                await ExecuteTask(kvp.Key, kvp.Value, forced: true);
            }
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await RefreshTaskList();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            foreach (var kvp in tasks.Where(t => t.Value.enabled))
            {
                if (IsTimeToRun(kvp.Value, currentTime) && !inProgress.Contains(kvp.Key))
                {
                    await ExecuteTask(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Обновляем список задач для корректной отработки после добавления новой задачи или при запуске программы
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task RefreshTaskList(bool force = false)
        {
            if (force || tasks.Count == 0)
            {
                tasks.Clear();
                inProgress.Clear();

                var items = await cronTaskStorage.GetCronTasks();
                foreach (var entity in items)
                {
                    var taskObject = CreateTask((CronTaskType)entity.task_type_id, entity.linked_price_list_guid, entity.task_id);
                    tasks.Add(taskObject, entity);
                }
            }
        }

        //TODO: разбить метод на более мелкие части
        /// <summary>
        /// Запускаем задачу
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskInfo"></param>
        /// <param name="forced"></param>
        /// <returns></returns>
        private async Task ExecuteTask(CronTaskBase task, CronTaskEntity taskInfo, bool forced = false)
        {
            inProgress.Add(task);
            OnTaskExecutionStart?.Invoke(taskInfo);
            TaskInProgress = taskInfo;

            var sw = Stopwatch.StartNew();
            CronTaskExecResult exec_result = CronTaskExecResult.Failed;

            if (forced)
            {
                taskInfo.last_exec_file_size = null;
            }

            try
            {
                await Task.Run(async () => await task.Run(taskInfo));

                exec_result = CronTaskExecResult.Success;

                sw.Stop();

                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Выполнено", $"Задание {taskInfo.name} выполнено. Длительность выполнения {(int)sw.Elapsed.TotalSeconds} сек.");
            }
            catch (CronTaskSkipException)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Пропуск", $"Задание '{taskInfo.name}' пропущено т.к. файл уже был загружен прежде");
                exec_result = CronTaskExecResult.Skipped;
            }
            catch (Exception ex)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{taskInfo.name}'. {ex.Message} {ex.InnerException?.Message ?? string.Empty}".Trim());
            }
            finally
            {
                inProgress.Remove(task);
                taskInfo.last_exec_date_time = DateTime.Now;
                taskInfo.last_exec_result = exec_result;
                await cronTaskStorage.SaveCronTaskExecResult(taskInfo);
                TaskInProgress = null;

                OnTaskExecutionEnd?.Invoke(taskInfo);

                if (exec_result == CronTaskExecResult.Success)
                {
                    OnTaskExecutionSuccess?.Invoke(taskInfo);
                }
                else if (exec_result == CronTaskExecResult.Failed)
                {                   
                    OnTaskExecutionError?.Invoke(taskInfo);
                    notifier.NotifyPriceListLoadingError(taskInfo.name);
                }
            }
        }

        /// <summary>
        /// Проверка задачи, следует ли ее сейчас запускать
        /// </summary>
        /// <param name="task"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private bool IsTimeToRun(CronTaskEntity task, TimeSpan now)
        {
            if (now >= task.exec_time && Math.Abs((task.exec_time - now).TotalMilliseconds) <= checkTimer.Interval)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(task.additional_exec_time))
            {
                try
                {
                    var additional = JsonConvert.DeserializeObject<List<TimeSpan>>(task.additional_exec_time);
                    foreach (var ts in additional)
                    {
                        if (now >= ts && Math.Abs((ts - now).TotalMilliseconds) <= checkTimer.Interval)
                        {
                            return true;
                        }
                    }
                }
                catch
                {

                }
            }

            return false;
        }

        //TODO: возможно стоит добавить добавить кроме загрузки из удаленного источника, например парсинг сайтов - отдельный тип задачи
        /// <summary>
        /// Фабрика задач
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="parameter"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private CronTaskBase CreateTask(CronTaskType taskType, string parameter, int taskId)
        {
            Type linkedPriceListType = parameter != null ? parameter.GetPriceListTypeByGuid() : null;

            //Таблица etk_app_cron_task_type
            switch (taskType)
            {
                case CronTaskType.RemotePriceList:
                    return new BL.CronTask.LoadRemotePriceListCronTask(linkedPriceListType, this, taskId);
            }

            throw new ArgumentException(taskType + " не реализован");
        }
    }
}
