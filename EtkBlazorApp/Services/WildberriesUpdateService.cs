using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.WildberriesApi;
using Humanizer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services;

public class WildberriesUpdateService : BackgroundService
{
    public readonly TimeSpan UpdateInterval = TimeSpan.FromHours(2);

    private readonly WildberriesApiClient wbApiClient;
    private readonly ISettingStorageReader settingStorageReader;
    private readonly ILogger<WildberriesUpdateService> logger;

    public WildberriesUpdateService(WildberriesApiClient wbApiClient,
        ISettingStorageReader settingStorageReader,
        ILogger<WildberriesUpdateService> logger)
    {
        this.wbApiClient = wbApiClient;
        this.settingStorageReader = settingStorageReader;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Запуск службы Wildberries API");

        using PeriodicTimer timer = new(UpdateInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Остановка службы Wildberries API");
        }
    }

    private async Task DoWork()
    {
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Запуск задачи по обновлению товаров на Wildberries");

        var progress = new Progress<WildberriesUpdateProgress>(v =>
        {
            string msgTemplate = "Wildberries API: \"{desc}\" шаг {step} из {totalSteps}";
            logger.LogTrace(msgTemplate, v.CurrentStepDescription, v.CurrentStep, v.TotalSteps);
        });

        //Токен, судя по описанию API, должен меняться каждые 180 дн. (на 18.05.2024).
        //Т.е. его нужно будет перевыпускать и обновлнять в настройках в личном кабинете
        string secureToken = await settingStorageReader.GetValue("wildberries_api_token");

        await wbApiClient.UpdateProducts(secureToken, SkuReader, progress);

        logger.LogInformation("Конец выполнения задачи по обновлению товаров на Wildberries. Выполнение заняло {elapsed}", sw.Elapsed.Humanize());
    }

    private async Task<WildberriesEtkProductUpdateEntry[]> SkuReader()
    {
        await Task.Yield();
        return Enumerable.Empty<WildberriesEtkProductUpdateEntry>().ToArray();
    }

}
