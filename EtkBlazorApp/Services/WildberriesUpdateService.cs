using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.WildberriesApi;
using Humanizer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services;

public class WildberriesUpdateService : BackgroundService
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    public readonly TimeSpan UpdateInterval = TimeSpan.FromHours(2);

    private readonly WildberriesApiClient wbApiClient;
    private readonly ISettingStorageReader settingStorageReader;
    private readonly IProductStorage productStorage;
    private readonly SystemEventsLogger sysLogger;

    public WildberriesUpdateService(WildberriesApiClient wbApiClient,
        ISettingStorageReader settingStorageReader,
        IProductStorage productStorage,
        SystemEventsLogger sysLogger)
    {
        this.wbApiClient = wbApiClient;
        this.settingStorageReader = settingStorageReader;
        this.productStorage = productStorage;
        this.sysLogger = sysLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(UpdateInterval);
        nlog.LogInformation("Служба обновление товаров на Wildberries запущена");

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            nlog.LogTrace("Служба обновление товаров на Wildberries остановлен");
        }
        catch (Exception ex)
        {
            await sysLogger.WriteSystemEvent(LogEntryGroupName.Wildberries,
                "Обновление остановлено",
                $"Ошибка в работе службы обновление товаров на Wildberries. Детали {ex.Message}");
        }
    }

    private async Task DoWork()
    {
        nlog.LogInformation("Запуск обновление товаров на Wildberries");

        var sw = Stopwatch.StartNew();

        var progress = new Progress<WildberriesUpdateProgress>(v =>
        {
            string msgTemplate = "Wildberries API: {desc} [{step} | {totalSteps}]. Длительность выполнения: {elapsed}";
            nlog.LogTrace("Служба обновление товаров на Wildberries запущена", msgTemplate, v.CurrentStepDescription, v.CurrentStep, v.TotalSteps, v.CurrentStep == 1 ? TimeSpan.Zero.Humanize() : sw.Elapsed.Humanize());
        });

        //Токен, судя по описанию API, должен меняться каждые 180 дн. (на 18.05.2024).
        //Т.е. его нужно будет перевыпускать и обновлнять в настройках в личном кабинете
        string secureToken = await settingStorageReader.GetValue("wildberries_api_token");

        if (string.IsNullOrWhiteSpace(secureToken))
        {
            throw new ArgumentNullException(nameof(secureToken) + " не предоставлен");
        }

        await wbApiClient.UpdateProducts(secureToken, ReadWildberriesProductsData, progress);
    }

    private async Task<WildberriesEtkProductUpdateEntry[]> ReadWildberriesProductsData()
    {
        await Task.Yield();
        return new WildberriesEtkProductUpdateEntry[];
    }

}
