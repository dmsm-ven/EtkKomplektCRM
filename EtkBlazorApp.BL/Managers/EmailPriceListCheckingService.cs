using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.BL.Managers;

public class EmailPriceListCheckingService
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    public TimeSpan TimerInterval => TimeSpan.FromMinutes(10);
    public DateTimeOffset? LastEmailDateTime
    {
        get => lastEmailDateTime;
        private set
        {
            if (lastEmailDateTime != value)
            {
                lastEmailDateTime = value;
                nlog.Trace("Обновление переменной LastEmailDateTime - {dt}", lastEmailDateTime);
            }
        }
    }
    private DateTimeOffset? lastEmailDateTime = null;

    private readonly Timer timer;
    private readonly CronTaskService cronTaskService;
    private readonly RemoteTemplateFileLoaderFactory priceListLoaderFactory;
    private readonly IPriceListTemplateStorage templatesRepository;
    private readonly EmailAttachmentExtractorInitializer emailExtractor;

    public EmailPriceListCheckingService(CronTaskService cronTaskService,
        RemoteTemplateFileLoaderFactory priceListLoaderFactory,
        IPriceListTemplateStorage templatesRepository,
        EmailAttachmentExtractorInitializer emailExtractor)
    {
        this.cronTaskService = cronTaskService;
        this.priceListLoaderFactory = priceListLoaderFactory;
        this.templatesRepository = templatesRepository;
        this.emailExtractor = emailExtractor;
        timer = new Timer();
    }
    //TODO: переделать таймер на BackgroundService
    /// <summary>
    /// Каждый тик заходит на почтовый ящик и ищет письма по условию из шаблонов прайс-листов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        await cronTaskService.RefreshTaskList();

        var tasks = cronTaskService.GetLoadedTasks();
        var templates = await templatesRepository.GetPriceListTemplates();

        var emailTemplates = templates
            .Where(pl => cronTaskService.EmailOnlyPriceListsIds.Contains(pl.id))
            .Where(pl => tasks.FirstOrDefault(t => t.linked_price_list_guid == pl.id) != null)
            .ToDictionary(pl => tasks.FirstOrDefault(t => t.linked_price_list_guid == pl.id), pl => pl);

        var args = await BuildSearchArgs(emailTemplates);
        if (args.Count == 0)
        {
            return;
        }

        var extractor = await emailExtractor.GetExtractor();

        var foundEmailsData = await extractor.GetPriceListIdsWithNewEmail(args, LastEmailDateTime, TimerInterval);
        LastEmailDateTime = foundEmailsData.CurrentLastMessageDateTime;

        nlog.Trace("Проверка почтового ящика на новые письма - найдено: {total}", foundEmailsData.PriceListIds.Length);

        foreach (var emailTemplate in emailTemplates)
        {
            if (foundEmailsData.PriceListIds.Contains(emailTemplate.Value.id))
            {
                cronTaskService.AddTaskToQueue(emailTemplate.Key.task_id, forced: false);
            }
        }

    }

    private async Task<IReadOnlyDictionary<string, ImapEmailSearchCriteria>> BuildSearchArgs(
        IReadOnlyDictionary<DataAccess.Entity.CronTaskEntity, DataAccess.Entity.PriceList.PriceListTemplateEntity> emailTemplates)
    {
        var args = new Dictionary<string, ImapEmailSearchCriteria>();

        foreach (var i in emailTemplates)
        {
            var fullTemplateInfo = await templatesRepository.GetPriceListTemplateById(i.Value.id);
            if (cronTaskService.TasksQueue.FirstOrDefault(t => t.task_id == i.Key.task_id) != null)
            {
                //Пропускаем, т.к. задача уже в очереди на выполнение
                continue;
            }
            if (i.Key.last_exec_date_time.HasValue &&
                i.Key.last_exec_date_time.Value.Date == DateTime.Now.Date &&
                i.Key.last_exec_result == DataAccess.CronTaskExecResult.Success)
            {
                //Пропускаем, т.к. сегодня уже выполняли эту задачу и выполнение было успешно
                continue;
            }

            if (string.IsNullOrWhiteSpace(fullTemplateInfo?.email_criteria_sender))
            {
                //Пропускаем, т.к. не заполнено поле from
                continue;
            }

            //OK - добавляем в поиск письма
            args.Add(i.Value.id, new ImapEmailSearchCriteria()
            {
                Subject = fullTemplateInfo.email_criteria_subject,
                Sender = fullTemplateInfo.email_criteria_sender
            });
        }

        return args;
    }

    public void Start()
    {
        timer.Interval = TimerInterval.TotalMilliseconds;
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
    }
}