using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.Wildberries;
using EtkBlazorApp.WildberriesApi;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services;

public class WildberriesUpdateService
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    private readonly WildberriesApiClient wbApiClient;
    private readonly IWildberriesProductRepository productRepository;
    private readonly IMarketplaceExportService marketplaceExportService;
    private readonly ISettingStorageReader settingStorageReader;
    private readonly ISettingStorageWriter settingStorageWriter;
    private readonly IWebHostEnvironment env;
    private readonly SystemEventsLogger sysLogger;

    public WildberriesUpdateService(WildberriesApiClient wbApiClient,
        IWildberriesProductRepository productRepository,
        IMarketplaceExportService marketplaceExportService,
        ISettingStorageReader settingStorageReader,
        ISettingStorageWriter settingStorageWriter,
        IWebHostEnvironment env,
        SystemEventsLogger sysLogger)
    {
        this.wbApiClient = wbApiClient;
        this.productRepository = productRepository;
        this.settingStorageReader = settingStorageReader;
        this.settingStorageWriter = settingStorageWriter;
        this.marketplaceExportService = marketplaceExportService;
        this.env = env;
        this.sysLogger = sysLogger;
    }

    public async Task UpdateWildberriesProducts(IProgress<string> progressIndicator = null)
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

            progressIndicator?.Report(v.CurrentStepDescription);
        });

        try
        {
            var products = await productRepository.ReadProducts();

            await ApplyDiscountAndRoundPrice(products);

            await wbApiClient.UpdateProducts(secureToken, products, progress);

            await settingStorageWriter.SetValueToDateTimeNow("wildberries_last_request");

            string updateType = progressIndicator == null ? "по таймеру" : "форсированное";

            await sysLogger.WriteSystemEvent(LogEntryGroupName.Wildberries,
                "Синхронизация выполнена",
                $"Синхронизация цен и остатков с Wildberries выполнена ({updateType})");
        }
        catch (Exception ex)
        {
            await sysLogger.WriteSystemEvent(LogEntryGroupName.Wildberries,
                "Ошибка обновления",
                $"Ошибка в работе службы обновление товаров на Wildberries. Детали {ex.Message}. StackTrace: {ex.StackTrace}");
        }
    }

    public async Task<List<WildberriesEtkProductUpdateEntry>> GetProductsFeed()
    {
        var products = await productRepository.ReadProducts();

        await ApplyDiscountAndRoundPrice(products);

        return products;
    }

    /// <summary>
    /// Добавляем наценку по условию (если цена меньше определенной) и округляем цену.
    /// </summary>
    /// <param name="products"></param>
    /// <returns></returns>
    private async Task ApplyDiscountAndRoundPrice(IEnumerable<WildberriesEtkProductUpdateEntry> products)
    {
        var stepsDic = new Dictionary<int, decimal>();
        //{
        //    [500] = 2.5m,
        //    [1000] = 2.0m
        //};

        //Тут дополнить логику, когда нужно будет сделать, что для каждого маркетплейса своя наценка, а не одна общая на все
        //marketplaceExportService.GetAllStepDiscounts(marketplace: "wildberries")
        try
        {
            var stepItems = await marketplaceExportService.GetAllStepDiscounts();

            foreach (var item in stepItems.OrderBy(i => i.price_border_in_rub))
            {
                stepsDic[item.price_border_in_rub] = item.ratio;
            }
        }
        catch
        {
            //не удалось загрузить наценки, пропускаем
        }

        int maxStep = stepsDic.Count > 0 ? stepsDic.Keys.Max() : 0;

        foreach (var product in products)
        {
            decimal productPrice = product.PriceInRUB;

            if (productPrice < maxStep)
            {
                foreach (var (border, ratio) in stepsDic)
                {
                    if ((int)productPrice < border)
                    {
                        productPrice *= ratio;
                        break;
                    }
                }
            }

            // округляем цену до 10 руб в любом случае, даже если наценки нет
            product.PriceInRUBWithDiscounts = ((int)Math.Ceiling(productPrice / 10m)) * 10;
        }
    }
}