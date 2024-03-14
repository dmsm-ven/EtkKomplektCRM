using EtkBlazorApp.BL.CronTask;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.BL.Templates.CronTask;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL.Managers
{
    //TODO: Переделать - заместо ручного выполнения по таймеру на библиотеку Hangfire

    /// <summary>
    /// Выполняет переодический опрос удаленных поставщиков для загрузки их прайс-листов
    /// </summary>
    public class CronTaskService
    {
        // event'ы для привязки на страницах, что бы можно было в любом месте приложения получить уведомление, например через Toasts
        // TODO: переделать из event в какой-то другой вариант
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly System.Timers.Timer checkTimer;
        private readonly System.Timers.Timer queueWorkerTimer;

        public event Action<CronTaskEntity> OnTaskExecutionStart;
        public event Action<CronTaskEntity> OnTaskExecutionEnd;
        public event Action<CronTaskEntity> OnTaskExecutionSuccess;
        public event Action<CronTaskEntity> OnTaskExecutionError;

        public CronTaskEntity TaskInProgress { get; private set; }

        //Минимальное время через которое выполняется следующая задача из очереди
        public TimeSpan QueueWorkerInterval { get; } = TimeSpan.FromMinutes(3);
        //Время через которое проверяется не нужно ли добавить задание в список на выполнение
        public TimeSpan CheckTimerInterval { get; } = TimeSpan.FromMinutes(1);

        public string[] EmailOnlyPriceListsIds { get; private set; }

        private readonly ICronTaskStorage cronTaskStorage;
        internal readonly IPriceListTemplateStorage templates;
        internal readonly IEtkUpdatesNotifier notifier;
        internal readonly ISettingStorageReader settings;
        internal readonly SystemEventsLogger logger;
        internal readonly ProductsPriceAndStockUpdateManager updateManager;
        internal readonly PriceListManager priceListManager;
        internal readonly RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory;

        private readonly Dictionary<CronTaskBase, CronTaskEntity> tasks;
        private readonly List<CronTaskBase> tasksQueue;


        public IEnumerable<CronTaskEntity> GetLoadedTasks() => tasks
            .Select(kvp => kvp.Value).ToArray();
        public IEnumerable<CronTaskEntity> TasksQueue => tasksQueue
            .Select(i => tasks.FirstOrDefault(t => t.Key.TaskId == i.TaskId).Value)
            .ToArray();


        public CronTaskService(
            ICronTaskStorage cronTaskStorage,
            IPriceListTemplateStorage templates,
            IEtkUpdatesNotifier notifier,
            ISettingStorageReader settings,
            SystemEventsLogger logger,
            ProductsPriceAndStockUpdateManager updateManager,
            PriceListManager priceListManager,
            RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory)
        {
            this.cronTaskStorage = cronTaskStorage;
            this.templates = templates;
            this.notifier = notifier;
            this.settings = settings;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;
            this.remoteTemplateLoaderFactory = remoteTemplateLoaderFactory;
            tasks = new Dictionary<CronTaskBase, CronTaskEntity>();
            tasksQueue = new List<CronTaskBase>();

            checkTimer = new System.Timers.Timer(CheckTimerInterval.TotalMilliseconds);
            queueWorkerTimer = new System.Timers.Timer(QueueWorkerInterval.TotalMilliseconds);
        }

        public void Start()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                checkTimer.Elapsed += CheckTimer_Elapsed;
                checkTimer.Start();

                queueWorkerTimer.Elapsed += QueueWorker_Elapsed;
                queueWorkerTimer.Start();
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

            AddTaskToQueue(task_id, forced: true);
        }

        public void AddTaskToQueue(int taskId, bool forced = false)
        {
            var task = tasks.FirstOrDefault(t => t.Value.task_id == taskId).Key;
            if (task != null)
            {
                AddTaskToQueue(task, forced);
            }
        }

        public void AddTaskToQueue(CronTaskBase task, bool forced = false)
        {
            if (tasksQueue.FirstOrDefault(t => t.TaskId == task.TaskId) != null)
            {
                return;
            }

            if (forced)
            {
                tasksQueue.Insert(0, task);
            }
            else
            {
                tasksQueue.Add(task);
            }

            nlog.Trace("Задача ID{taskId} добавлена в очередь. Теперь в очереди: {total} задач", task.TaskId, tasksQueue.Count);
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await RefreshTaskList();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            var source = tasks.Where(t => t.Value.enabled);

            foreach (var kvp in source)
            {
                bool isEmailTask = EmailOnlyPriceListsIds.Contains(kvp.Value.linked_price_list_guid);

                //Email Task добавляет в очередь отдельный worker класс
                if (IsTimeToRun(kvp.Value, currentTime) && !isEmailTask)
                {
                    AddTaskToQueue(kvp.Key.TaskId);
                }
            }
        }

        private async void QueueWorker_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (tasksQueue.Count == 0)
            {
                return;
            }

            await semaphore.WaitAsync();

            var headTask = tasksQueue[0];
            var taskInfo = await cronTaskStorage.GetCronTaskById(headTask.TaskId);

            try
            {
                var taskEntity = tasks.FirstOrDefault(t => t.Value.task_id == headTask.TaskId).Value;

                await ExecuteTask(headTask, taskEntity, false);
            }
            catch (Exception ex)
            {
                nlog.Warn("Ошибка выполнения задачи '{taskName}'. {errorMessage}",
                    taskInfo.name, ex.Message);
                throw;
            }
            finally
            {
                tasksQueue.Remove(headTask);
                semaphore.Release();
                nlog.Trace("Задача {taskName} извлечена из списка. Теперь в очереди: {count} задач", taskInfo.name, tasksQueue.Count);
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
                var items = await cronTaskStorage.GetCronTasks();
                foreach (var entity in items)
                {
                    var taskObject = CreateTask((CronTaskType)entity.task_type_id, entity.linked_price_list_guid, entity.task_id);
                    tasks.Add(taskObject, entity);
                }

                var allPriceLists = await templates.GetPriceListTemplates();

                EmailOnlyPriceListsIds = allPriceLists
                    .Where(pl => pl?.remote_uri_method_name == "EmailAttachment")
                    .Select(pl => pl.id)
                    .ToArray();
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
                await task.Run(taskInfo);

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
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{taskInfo.name}'. {ex.Message} {ex.StackTrace ?? ""}".Trim());
            }
            finally
            {
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
                    await notifier.NotifyPriceListLoadingError(taskInfo.name);
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
                    return new LoadRemotePriceListCronTask(linkedPriceListType, this, taskId);
            }

            throw new ArgumentException(taskType + " не реализован");
        }
    }
}