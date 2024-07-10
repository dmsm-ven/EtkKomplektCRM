using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories.Wildberries;
using EtkBlazorApp.WildberriesApi;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
    public readonly TimeSpan FirstRunDelay = TimeSpan.FromSeconds(15);

    private readonly WildberriesApiClient wbApiClient;
    private readonly IWildberriesProductRepository productRepository;
    private readonly ISettingStorageReader settingStorageReader;
    private readonly ISettingStorageWriter settingStorageWriter;
    private readonly IWebHostEnvironment env;
    private readonly SystemEventsLogger sysLogger;

    public WildberriesUpdateService(WildberriesApiClient wbApiClient,
        IWildberriesProductRepository productRepository,
        ISettingStorageReader settingStorageReader,
        ISettingStorageWriter settingStorageWriter,
        IWebHostEnvironment env,
        SystemEventsLogger sysLogger)
    {
        this.wbApiClient = wbApiClient;
        this.productRepository = productRepository;
        this.settingStorageReader = settingStorageReader;
        this.settingStorageWriter = settingStorageWriter;
        this.env = env;
        this.sysLogger = sysLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (env.IsDevelopment())
        {
            nlog.Info("Служба обновления Wildberries в Development режиме отключена");
            return;
        }

        nlog.Info("Служба обновление товаров на Wildberries запущена. Следующий запуск через {delay}", FirstRunDelay.Humanize());

        await Task.Delay(FirstRunDelay);

        using PeriodicTimer timer = new(UpdateInterval);

        try
        {
            //Запускаем один раз
            await UpdateWildberriesProducts();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                //И по таймеру
                await UpdateWildberriesProducts();
            }
        }
        catch (OperationCanceledException)
        {
            nlog.Trace("Служба обновление товаров на Wildberries остановлена");
        }
        catch (Exception ex)
        {
            await sysLogger.WriteSystemEvent(LogEntryGroupName.Wildberries,
                "Обновление остановлено",
                $"Ошибка в работе службы обновление товаров на Wildberries. Детали {ex.Message}. StackTrace: {ex.StackTrace}");
        }
    }

    public async Task UpdateWildberriesProducts()
    {
        nlog.Info("Запуск обновление товаров на Wildberries");

        string secureToken = await settingStorageReader.GetValue("wildberries_api_token");

        if (string.IsNullOrWhiteSpace(secureToken))
        {
            nlog.Error("WB API secure token was empty");
            throw new ArgumentNullException(nameof(secureToken) + " не предоставлен");
        }

        var sw = Stopwatch.StartNew();

        var progress = new Progress<WildberriesUpdateProgress>(v =>
        {
            string msgTemplate = "Wildberries API: {desc} [{step} | {totalSteps}]. Длительность выполнения: {elapsed}";
            nlog.Trace(msgTemplate,
                v.CurrentStepDescription,
                v.CurrentStep,
                v.TotalSteps,
                (v.CurrentStep == 1 ? TimeSpan.Zero : sw.Elapsed).Humanize());
        });

        //Токен, судя по описанию API, должен меняться каждые 180 дн. (на 18.05.2024).
        //Т.е. его нужно будет перевыпускать и обновлнять в настройках в личном кабинете

        //Наценки считаются прямо в SQL запросе
        var products = await productRepository.ReadProducts();
        await wbApiClient.UpdateProducts(secureToken, products, progress);

        await settingStorageWriter.SetValueToDateTimeNow("wildberries_last_request");
    }
}
