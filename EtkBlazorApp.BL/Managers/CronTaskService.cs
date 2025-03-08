using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.BL.Templates.CronTask;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using Humanizer;
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
    /// <summary>
    /// Выполняет переодический опрос удаленных поставщиков для загрузки их прайс-листов и др. задачи унаследованных от CronTask
    /// </summary>
    public class CronTaskService
    {
        // event'ы для привязки на страницах, что бы можно было в любом месте приложения получить уведомление, например через Toasts
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        //В каждый момент может выполняться только 1 задание, что бы ло проблем когда разные задачи пытаются обновить одни и теже товары
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        private static readonly SemaphoreSlim queueAddSemaphore = new(1, 1);

        //Таймер который проверяет нужно ли добавить задачу в очередь
        private readonly System.Timers.Timer checkTimer;
        //Таймер в котом выполняются текущие задачи которые уже добавлены в очередь
        private readonly System.Timers.Timer queueWorkerTimer;

        public event Action<CronTaskEntity> OnTaskExecutionStart;
        public event Action<CronTaskEntity> OnTaskExecutionEnd;
        public event Action<CronTaskEntity> OnTaskExecutionSuccess;
        public event Action<CronTaskEntity> OnTaskExecutionError;

        //Текущее задание которое находится в процессе выполнения
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

        private readonly WildberriesUpdateService wbUpdateService;
        private readonly ReportManager reportManager;
        private readonly EncryptHelper encryptHelper;
        private readonly List<CronTaskEntity> taskDefinitions;
        private readonly List<CronTaskQueueEntry> tasksQueue;

        public IEnumerable<CronTaskEntity> GetLoadedTasks() => taskDefinitions;
        public IEnumerable<CronTaskEntity> ActiveTasksInQueue => tasksQueue.Select(i => i.TaskDefinition);

        public CronTaskService(
            ICronTaskStorage cronTaskStorage,
            IPriceListTemplateStorage templates,
            IEtkUpdatesNotifier notifier,
            ISettingStorageReader settings,
            SystemEventsLogger logger,
            ProductsPriceAndStockUpdateManager updateManager,
            PriceListManager priceListManager,
            RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory,
            WildberriesUpdateService wbUpdateService,
            ReportManager reportManager,
            EncryptHelper encryptHelper)
        {
            this.cronTaskStorage = cronTaskStorage;
            this.templates = templates;
            this.notifier = notifier;
            this.settings = settings;
            this.logger = logger;
            this.updateManager = updateManager;
            this.priceListManager = priceListManager;
            this.remoteTemplateLoaderFactory = remoteTemplateLoaderFactory;
            this.wbUpdateService = wbUpdateService;
            this.reportManager = reportManager;
            this.encryptHelper = encryptHelper;

            taskDefinitions = new List<CronTaskEntity>();
            tasksQueue = new List<CronTaskQueueEntry>();

            checkTimer = new System.Timers.Timer(CheckTimerInterval.TotalMilliseconds);
            queueWorkerTimer = new System.Timers.Timer(QueueWorkerInterval.TotalMilliseconds);
        }

        public void Start()
        {
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Start();

            queueWorkerTimer.Elapsed += QueueWorker_Elapsed;
            queueWorkerTimer.Start();
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
            CronTaskEntity taskDefinition = taskDefinitions.FirstOrDefault(t => t.task_id == taskId);

            if (taskDefinition == null)
            {
                nlog.Warn("Не найден определить шаблона задачи с ID{taskId}", taskDefinition.task_id);
                return;
            }

            CronTask task = CreateTask((CronTaskType)taskDefinition.task_type_id, taskDefinition.linked_price_list_guid, taskId);
            bool isTaskAlreadyInQueue = tasksQueue.FirstOrDefault(t => t.Task.GetType() == task.GetType()) != null;

            if (isTaskAlreadyInQueue)
            {
                nlog.Warn("Попытка добавить в очередь задачу, которая уже в списке на выполнение ID{taskId}", taskDefinition.task_id);
                return;
            }

            var entry = new CronTaskQueueEntry(taskDefinition, task);
            if (forced)
            {
                tasksQueue.Insert(0, entry);
            }
            else
            {
                tasksQueue.Add(entry);
            }

            nlog.Trace("Задача ID{taskId} добавлена в очередь. Теперь в очереди: {total} задач", entry.TaskDefinition.task_id, tasksQueue.Count);
        }

        public bool IsTaskInQueue(int taskId)
        {
            return ActiveTasksInQueue.FirstOrDefault(t => t.task_id == taskId) != null;
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await queueAddSemaphore.WaitAsync();

            await RefreshTaskList();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            var enabledTasks = taskDefinitions.Where(t => t.enabled);

            foreach (var task in enabledTasks)
            {
                bool isEmailTask = EmailOnlyPriceListsIds.Contains(task.linked_price_list_guid);

                //Email Task задачи пропускаем (их обрабатывает отдельный worker) 
                if (IsTimeToRun(task, currentTime) && !isEmailTask)
                {
                    AddTaskToQueue(task.task_id);
                }
            }

            queueAddSemaphore.Release();
        }

        private async void QueueWorker_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (tasksQueue.Count == 0)
            {
                return;
            }

            await semaphore.WaitAsync();

            CronTaskQueueEntry headTaskEntry = tasksQueue[0];
            //Получаем свежую версия описания задачи из БД (а не из списка в поле класса) т.к. состояние могло изменится извне
            CronTaskEntity headTaskDefinition = await cronTaskStorage.GetCronTaskById(headTaskEntry.TaskDefinition.task_id);

            try
            {
                await ExecuteTask(headTaskEntry.Task, headTaskDefinition, forced: false);
            }
            catch (Exception ex)
            {
                nlog.Warn("Ошибка выполнения задачи '{taskName}'. {errorMessage}",
                    headTaskDefinition.name, ex.Message);
                //Ничего не делаем, просто пропускаем задачу
            }
            finally
            {
                tasksQueue.Remove(headTaskEntry);
                semaphore.Release();
                nlog.Trace("Задача {taskName} удалена из очереди. Теперь в очереди: {count} задач",
                    headTaskDefinition.name, tasksQueue.Count);
            }
        }

        /// <summary>
        /// Обновляем список задач для корректной отработки после добавления новой задачи и при первом запуске
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task RefreshTaskList(bool force = false)
        {
            if (force || tasksQueue.Count == 0)
            {
                taskDefinitions.Clear();

                var items = await cronTaskStorage.GetCronTasks();
                foreach (var entity in items)
                {
                    var taskObject = CreateTask((CronTaskType)entity.task_type_id, entity.linked_price_list_guid, entity.task_id);
                    taskDefinitions.Add(entity);
                }

                var allPriceLists = await templates.GetPriceListTemplates();

                //Получаем список задач по загрузке прайс-листов которые приходят на почтовый ящик, что бы на странице списка задач отметить их в таблице
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
        /// <param name="taskDefinition"></param>
        /// <param name="forced"></param>
        /// <returns></returns>
        private async Task ExecuteTask(CronTask task, CronTaskEntity taskDefinition, bool forced = false)
        {
            OnTaskExecutionStart?.Invoke(taskDefinition);
            TaskInProgress = taskDefinition;

            var sw = Stopwatch.StartNew();
            CronTaskExecResult exec_result = CronTaskExecResult.Failed;

            if (forced)
            {
                //Очищаем размер последнего загруженного файла для этого задания
                //в следствии чего, задание выполнится, даже если загружаем этот же самый файл
                taskDefinition.last_exec_file_size = null;
            }

            try
            {
                nlog.Trace("Запуск выполнения задачи {taskName}", taskDefinition?.name);

                await task.Run(taskDefinition, forced);
                exec_result = CronTaskExecResult.Success;

                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Выполнено", $"Задание {taskDefinition.name} выполнено. Длительность выполнения {sw.Elapsed.Humanize()}");
            }
            catch (CronTaskSkipException)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Пропуск", $"Задание '{taskDefinition.name}' пропущено т.к. файл уже был загружен прежде");
                exec_result = CronTaskExecResult.Skipped;
            }
            catch (Exception ex)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.CronTask, "Ошибка", $"Ошибка выполнения задания '{taskDefinition.name}'. {ex.Message} {ex.StackTrace ?? ""}".Trim());
                exec_result = CronTaskExecResult.Failed;
            }
            finally
            {
                taskDefinition.last_exec_date_time = DateTime.Now;
                taskDefinition.last_exec_result = exec_result;
                await cronTaskStorage.SaveCronTaskExecResult(taskDefinition);
                TaskInProgress = null;

                OnTaskExecutionEnd?.Invoke(taskDefinition);

                if (exec_result == CronTaskExecResult.Success)
                {
                    OnTaskExecutionSuccess?.Invoke(taskDefinition);
                }
                else if (exec_result == CronTaskExecResult.Failed)
                {
                    OnTaskExecutionError?.Invoke(taskDefinition);
                    notifier.NotifyPriceListLoadingError(taskDefinition.name);
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
            var day = DateTime.Now.DayOfWeek;

            //В выходные выполнение не производится.
            //EMAIL задачи выполняются все равно, т.к. запускаются при получении письма
            //TODO: Вообще в идеале все значение в будущем стоит перенести на CRON Expression 
            //if ((day == DayOfWeek.Saturday) || (day == DayOfWeek.Sunday))
            //{
            //    return false;
            //}

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

        /// <summary>
        /// Создаем задачу указанного типа. 
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="parameter"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private CronTask CreateTask(CronTaskType taskType, string parameter, int taskId)
        {
            Type linkedPriceListType = parameter != null ? parameter.GetPriceListTypeByGuid() : null;


            //Таблица etk_app_cron_task_type
            //TODO: доделать DI проброс конструктора подклассов
            switch (taskType)
            {
                case CronTaskType.RemotePriceList:
                    return new LoadRemotePriceListCronTask(
                        linkedPriceListType, templates, remoteTemplateLoaderFactory, priceListManager, updateManager);
                case CronTaskType.WildberriesSync:
                    return new WildberriesCronTask(wbUpdateService);
                case CronTaskType.ViPricatUpload:
                    return new VseInstrumentiPricatUploaderCronTask(settings, logger, reportManager, encryptHelper);
            }

            throw new ArgumentException(taskType + " не реализован");
        }
    }
}