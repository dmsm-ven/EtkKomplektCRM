﻿using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories.Product;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class ProductsPriceAndStockUpdateManager
    {
        //Запуск пересчета цен товаров в валюте
        //readonly string CurrencyPlusUri = "https://etk-komplekt.ru/cron/currency_plus.php";
        //Запуск пересчета цен товаров в валюте - собственный улучшенный модуль
        private readonly string CurrencyCustomUri;

        private readonly IProductStorage productsStorage;
        private readonly IProductUpdateService productUpdateService;
        private readonly ISettingStorageReader settingStorage;
        private readonly IManufacturerStorage manufacturerStorage;
        private readonly IMonobrandStorage monobrandStorage;
        private readonly IDatabaseProductCorrelator correlator;
        private readonly PriceListPriceHistoryManager priceHistoryManager;
        private readonly HttpClient httpClient;

        public ProductsPriceAndStockUpdateManager(IProductStorage productsStorage,
            IProductUpdateService productUpdateService,
            ISettingStorageReader settingStorage,
            IManufacturerStorage manufacturerStorage,
            IMonobrandStorage monobrandStorage,
            IDatabaseProductCorrelator correlator,
            IOptions<CurrencyUpdaterEndpointOptions> CurrencyUpdaterEndpoint,
            IHttpClientFactory httpClientFactory,
            PriceListPriceHistoryManager priceHistoryManager
            )
        {
            this.productsStorage = productsStorage;
            this.productUpdateService = productUpdateService;
            this.settingStorage = settingStorage;
            this.manufacturerStorage = manufacturerStorage;
            this.monobrandStorage = monobrandStorage;
            this.correlator = correlator;
            this.priceHistoryManager = priceHistoryManager;
            this.CurrencyCustomUri = CurrencyUpdaterEndpoint.Value.Uri;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task UpdatePriceAndStock(
            IEnumerable<PriceLine> priceLines,
            IProgress<string> progress = null)
        {
            progress?.Report("Подготовка списка производителей для обновления");
            int[] affectedBrandsIds = await GetAffectedBrandIds(priceLines);

            progress?.Report("Загрузка товаров");
            var products = await productsStorage.ReadProducts(affectedBrandsIds);

            progress?.Report("Сопоставление товаров для обновления");
            var data = await correlator.GetCorrelationData(products, priceLines);

            progress?.Report("Сохранение истории изменения цены");
            await priceHistoryManager.SavePriceChangesHistory(priceLines, data);

            progress?.Report("Обновление цен/остатков отдельных складов");
            await productUpdateService.UpdateStockProducts(data, affectedBrandsIds);

            progress?.Report("Перерасчет основного остатка в товарах");
            await productUpdateService.ComputeStocksQuantity(data);

            progress?.Report("Перерасчет основной цены в товарах");
            await productUpdateService.ComputeProductsPrice(data);

            if (data.Any(d => d.NextStockDelivery != null))
            {
                progress?.Report("Обновление следующих поставок товара для складов");
                await productUpdateService.UpdateNextStockDelivery(data);
            }

            if (data.Any(line => line.price.HasValue))
            {
                progress?.Report("Пересчет цен товаров");
                await httpClient.GetStringAsync(CurrencyCustomUri);
            }

            if (await settingStorage.GetValue<bool>("update-monobrand-websites"))
            {
                await UpdateMonobrands(affectedBrandsIds, progress);
            }

            progress?.Report("Обновление завершено");
        }

        private async Task<int[]> GetAffectedBrandIds(IEnumerable<PriceLine> priceLines)
        {
            var allManufacturers = await manufacturerStorage.GetManufacturers();

            var affectedBrands = priceLines
                .Select(pl => pl.Manufacturer)
                .Distinct()
                .ToList();

            var templates = priceLines.GroupBy(p => p.Template.GetType()).Select(g => g.Key).Distinct().ToArray();

            AddAdditionalAffectedBrands(templates, affectedBrands);

            var affectedBrandsIds = allManufacturers
                .Where(m => affectedBrands.Contains(m.name, StringComparer.OrdinalIgnoreCase))
                .Select(m => m.manufacturer_id)
                .ToList();

            return affectedBrandsIds.ToArray();
        }

        /// <summary>
        /// Дополнительное добавление брендов для нахождения сопоставления. Если в прайс-листе поставщика нет отдельного поля с брендом - то что бы он корректно загрузился, нужно добавить его в дополнительную загруку
        /// </summary>
        /// <param name="affectedBrands"></param>
        private void AddAdditionalAffectedBrands(Type[] templateTypes, List<string> affectedBrands)
        {
            if (templateTypes != null && templateTypes.FirstOrDefault(t => t == typeof(_1CPriceListTemplate)) != null)
            {
                //В выгрузке 1С есть товары без брендаы
                affectedBrands.Add("Не указан");
            }

            // В производителе Bosch есть подбренд Dremel, но он так же считается и отдельным брендом. 
            // Хотя находится в прайс-листе Bosch и там не указан как отдельный бренд
            if (affectedBrands.Contains("Bosch"))
            {
                affectedBrands.Add("Dremel");
            }

            //Вместе с Weller в прайс-листах Spring-E идет так же Erem, без указания бренда
            if (affectedBrands.Contains("Weller"))
            {
                affectedBrands.Add("Erem");
            }

            //Для корректной загрузки остатков бренда Guide в прайс-листе Testo
            if (affectedBrands.Contains("Testo"))
            {
                affectedBrands.Add("Guide");
            }

            // Если есть какой-либо бренд из списка то добавляем всех, т.к. у них есть товары которые по факту один и тот же
            // Но называется в каждом из брендов по разному и у всех одна модель (на сайте есть только 1 вариант)
            var eltechBrands = new string[] { "Tianma", "BOE", "NEC", "AU Optronics" };
            if (eltechBrands.Any(eltechBrand => affectedBrands.Contains(eltechBrand)))
            {
                affectedBrands.AddRange(eltechBrands);
            }
        }

        private async Task UpdateMonobrands(IEnumerable<int> affectedBrandsIds, IProgress<string> progress = null)
        {
            var monobrands = await monobrandStorage.GetMonobrands();
            var affectedMonobrands = monobrands
                .Where(monobrand => monobrand.is_update_enabled && affectedBrandsIds.Contains(monobrand.manufacturer_id))
                .ToList();

            if (affectedMonobrands.Count == 0) { return; }

            progress?.Report("Обновление монобренд сайтов");
            string key = await settingStorage.GetValue("monobrand_updater_key");
            foreach (var monobrand in affectedMonobrands)
            {
                try
                {
                    string uriQuery = $"client_name=monobrand&key={key}&manufacturer={monobrand.manufacturer_name}&currency_code={monobrand.currency_code}";
                    string apiUri = $"{monobrand.website}/?route=api/monobrand_updater/update&{uriQuery}";

                    progress?.Report($"Обновление сайта {monobrand.website}");
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await new WebClient().DownloadStringTaskAsync(apiUri);
                }
                catch (Exception ex)
                {
                    progress?.Report($"Ошибка обновления сайта {monobrand.website}. " + ex.Message);
                }
            }
        }
    }
}
