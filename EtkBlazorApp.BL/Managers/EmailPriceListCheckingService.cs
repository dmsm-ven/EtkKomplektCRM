using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace EtkBlazorApp.BL.Managers;

public class EmailPriceListCheckingService
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    public TimeSpan TimerInterval => TimeSpan.FromMinutes(10);

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

    //TODO: переделать заход в ящик на проверку новых писем с сопоставление
    /// <summary>
    /// Каждый тик заходит на почтовый ящик и ищет письма по условию из шаблонов прайс-листов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        nlog.Trace("Timer_Elapsed START");

        await cronTaskService.RefreshTaskList();

        var tasks = cronTaskService.GetLoadedTasks();

        var templates = await templatesRepository.GetPriceListTemplates();

        var emailTemplatesStep1 = templates
            .Where(pl => cronTaskService.EmailOnlyPriceListsIds.Contains(pl.id))
            .Where(pl => tasks.FirstOrDefault(t => t.linked_price_list_guid == pl.id) != null)
            .Select(pl => new
            {
                PriceListTemplate = pl,
                LinkedTaskId = tasks.FirstOrDefault(t => t.linked_price_list_guid == pl.id).task_id
            })
            .Where(d => cronTaskService.TasksQueue.FirstOrDefault(i => i.task_id == d.LinkedTaskId) == null)
            .ToList();

        var emailTemplatesStep2 = emailTemplatesStep1
            .Where(data => !IsExecutedToday(data.LinkedTaskId))
            .Select(data => data)
            .ToArray();


        //Нашли список emailов от поставщиков
        var extractor = await emailExtractor.GetExtractor();

        if (emailTemplatesStep2 == null || emailTemplatesStep2.Length == 0)
        {
            return;
        }

        IReadOnlyDictionary<string, bool> foundEmails = await extractor.IsAttachmentsExist(emailTemplatesStep2.Select(i => i.PriceListTemplate));

        foreach (var email in foundEmails.Where(em => em.Value == true))
        {
            var templateTaskId = emailTemplatesStep2.FirstOrDefault(sp => sp.PriceListTemplate?.id == email.Key)?.LinkedTaskId;
            if (templateTaskId.HasValue)
            {
                cronTaskService.AddTaskToQueue(templateTaskId.Value, forced: false);
            }
        }

        nlog.Trace("Timer_Elapsed END");
    }

    private bool IsExecutedToday(int taskId)
    {
        if (!cronTaskService.TasksLastExecutionTime.ContainsKey(taskId))
        {
            return false;
        }
        return IsExecutedToday(cronTaskService.TasksLastExecutionTime[taskId]);
    }

    private bool IsExecutedToday(DateTime? taskLastExecTime)
    {
        return taskLastExecTime.HasValue && taskLastExecTime.Value.Date == DateTime.Now.Date;
    }

    public void Start()
    {
        timer.Interval = TimerInterval.TotalMilliseconds;
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
    }
}