﻿using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers;


public class PriceListPriceHistoryManager
{
    private readonly IPriceListUpdateHistoryRepository repo;
    private readonly ISettingStorageReader settings;
    private readonly IProductStorage productStorage;
    private readonly IPriceListTemplateStorage priceListRepo;
    private readonly IEtkUpdatesNotifier notifier;
    private const int MAX_ITEMS = 500;

    public PriceListPriceHistoryManager(IPriceListUpdateHistoryRepository repo,
        ISettingStorageReader settings,
        IProductStorage productStorage,
        IPriceListTemplateStorage priceListRepo,
        IEtkUpdatesNotifier notifier)
    {
        this.repo = repo;
        this.settings = settings;
        this.productStorage = productStorage;
        this.priceListRepo = priceListRepo;
        this.notifier = notifier;
    }

    /// <summary>
    /// Сохраняем в базу данных историю изменений цены для товаров, сохраняются только строки которые были изменены по сравнению с прошлой загрузкой
    /// </summary>
    /// <param name="products"></param>
    /// <param name="updateData"></param>
    /// <returns></returns>
    public async Task SavePriceChangesHistory(IEnumerable<PriceLine> products, IEnumerable<ProductUpdateData> updateData)
    {
        var priceData = updateData
            .Where(p => p.price.HasValue)
            .OrderBy(i => i.product_id)
            .ToDictionary(p => p.product_id, p => p.price.Value);

        //TODO: проверить, тут может быть ошибка, если загружается несколько прайс-листов за раз

        var templateTypes = products.GroupBy(p => p.Template).Select(i => i.Key.GetType().GetPriceListGuidByType()).ToArray();
        if (templateTypes.Length == 0)
        {
            int productsCount = products != null ? products.Count() : 0;
            throw new ArgumentException($"В товарах ({productsCount}) не указан шаблон");
        }
        if (templateTypes.Length > 1)
        {
            string templateNames = string.Join(", ", templateTypes);
            throw new ArgumentOutOfRangeException($"Невозможно загрузить сразу несколько шаблонов прайс-листов: {templateNames ?? "<Пусто>"}");
        }
        string guid = templateTypes.Single();


        // Шаг 1. Берем все вхождения (заголовки)
        var entries = await repo.GetPriceListUpdateEntries(guid);

        //Шаг 2, 3
        Dictionary<int, decimal> linesData = priceData;
        if (entries.Any())
        {
            // Шаг 2. Берем товары из истории, если за прошлую загрузку данных нет, значит должна идти максимально близкая записить где были какие-то данные о цене
            var items = await repo.GetPriceListUpdateHistory(guid);
            var previuosUpdateData = items
                .OrderByDescending(i => i.Key.update_id)
                .SelectMany(i => i.Value.Select(j => new
                {
                    product_id = j.product_id,
                    price = j.price,
                    update_id = i.Key.update_id
                }))
                .GroupBy(i => i.product_id)
                .ToDictionary(i => i.Key, p => p.First().price);

            // Шаг 3. Находим только те которые нужно добавлять
            linesData = GetNewLines(priceData, previuosUpdateData);
        }

        // Шаг 4. Сохраняем
        await repo.SavePriceListUpdateProductsPriceData(guid, linesData);

        // Шаг 5. Уведомляем если есть изменения
        if (linesData.Count > 0 && await notifier.IsActive())
        {
            await NotifyAboutPriceListChanges(guid);
        }
    }

    private async Task NotifyAboutPriceListChanges(string guid)
    {
        var newData = await GetProductsPriceChangeHistoryForPriceList(guid, getOnlyLast: true);

        if (newData.Data.Count > 0)
        {
            await notifier.NotifyPriceListProductPriceChanged(newData);
        }

    }

    private Dictionary<int, decimal> GetNewLines(Dictionary<int, decimal> currentUpdateData, Dictionary<int, decimal> previuosUpdateData)
    {
        Dictionary<int, decimal> dic = new();

        foreach (var kvp in currentUpdateData)
        {
            // Пропускаем если: в прошлую загрузку этот товар с точно такой же ценой как и сейчас
            if (previuosUpdateData.TryGetValue(kvp.Key, out var prev) && prev == kvp.Value)
            {
                continue;
            }

            dic[kvp.Key] = kvp.Value;
        }

        return dic;
    }

    public async Task<PriceListProductPriceChangeHistory> GetProductsPriceChangeHistoryForPriceList(string guid, bool getOnlyLast)
    {
        double minmumChangePercent = await settings.GetValue<double>("price_list_product_price_change_percent_to_notify");
        DateTime now = DateTime.Now.Date;

        if (minmumChangePercent == default(double))
        {
            throw new ArgumentNullException(nameof(minmumChangePercent));
        }
        minmumChangePercent /= 100;

        var entiresDictionary = await repo.GetPriceListUpdateHistory(guid);
        int lastUpdateId = entiresDictionary.Max(i => i.Key.update_id);

        int[] productIds = entiresDictionary.Values
            .SelectMany(i => i)
            .Select(i => i.product_id)
            .Distinct()
            .OrderBy(i => i)
            .ToArray();
        Dictionary<int, string> productNames = await productStorage.GetProductNames(productIds);

        var items = new Dictionary<int, List<ProductPriceChangeHistoryItem>>();

        foreach (var kvp in entiresDictionary.OrderBy(i => i.Key.update_id))
        {
            foreach (var i in kvp.Value)
            {
                if (!items.ContainsKey(i.product_id))
                {
                    items[i.product_id] = new List<ProductPriceChangeHistoryItem>();
                }

                var node = new ProductPriceChangeHistoryItem()
                {
                    Price = i.price,
                    DateTime = kvp.Key.date_time,
                    ProductId = i.product_id,
                    ProductName = productNames[i.product_id],
                    PreviousItem = items[i.product_id].LastOrDefault(),
                    UpdateId = kvp.Key.update_id
                };

                items[i.product_id].Add(node);
            }
        }

        // Проверить при getOnlyLast == true.
        // Если несколько загрузок прайс-листа за день, то может выдавать от предудыщей.
        // Как вариант можно сохранять update_id вхождение, даже если не было новых данных
        var list = items
            .Values
            .Where(v => v.Count > 1)
            .Select(i => i.Last())
            .Where(i => getOnlyLast ? (i.UpdateId == lastUpdateId && i.DateTime.Date == now) : true)
            .Where(i => i.ChangePercent > minmumChangePercent)
            .Take(MAX_ITEMS)
            .ToList();

        if (list.Count > 0)
        {
            string priceListName = (await priceListRepo.GetPriceListTemplateById(guid)).title;

            return new PriceListProductPriceChangeHistory
            {
                Data = list,
                PriceListGuid = guid,
                PriceListName = priceListName,
                MinimumOverpricePercent = minmumChangePercent,
                MaxItems = list.Count == MAX_ITEMS
            };
        }
        return new();
    }
}
