using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.WildberriesApi.Data;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EtkBlazorApp.WildberriesApi;

public class WildberriesApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<WildberriesApiClient> logger;

    private readonly int MAX_PRODUCTS_PER_PAGE = 1000;
    private readonly int TOTAL_STEPS = 7;
    private readonly Dictionary<string, string> etkIdToWbBarcode = new();
    private readonly Dictionary<string, int> etkIdToWbNMID = new();
    private Dictionary<string, int> etkIdTopriceMap;
    private Dictionary<string, int> etkIdToQuantity;

    public WildberriesApiClient(HttpClient httpClient, ILogger<WildberriesApiClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task UpdateProducts(string secureToken,
        Func<Task<WildberriesEtkProductUpdateEntry[]>> productsDataReader,
        IProgress<WildberriesUpdateProgress> progress)
    {
        progress?.Report(new WildberriesUpdateProgress(1, TOTAL_STEPS, "Получаем список товаров которые есть на WB"));
        await ReceiveWbProductsData();

        progress?.Report(new WildberriesUpdateProgress(2, TOTAL_STEPS, "Получаем список товаров ETK"));
        await ReceiveEtkProductsData(productsDataReader);

        progress?.Report(new WildberriesUpdateProgress(3, TOTAL_STEPS, "Получаем список складов WB и берем первый"));
        int warehouseId = await GetWarehouseId();

        progress?.Report(new WildberriesUpdateProgress(4, TOTAL_STEPS, "Выполнение API запроса к WB на обнуление остатков"));
        await ClearStock(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(5, TOTAL_STEPS, "Выполнение API запроса к WB на обновление остатков"));
        await UpdateQuantity(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(6, TOTAL_STEPS, "Выполнение API запроса к WB на обновление цен"));
        await UpdatePrices();

        progress?.Report(new WildberriesUpdateProgress(7, TOTAL_STEPS, "Конец обновление остатков и цен WB"));
    }

    private async Task<int> GetWarehouseId()
    {
        var uri = $"https://suppliers-api.wildberries.ru/api/v3/warehouses";

        var list = await httpClient.GetFromJsonAsync<WBWarehouseList>(uri);

        if (list == null || list.Count == 0 || list[0].id == 0)
        {
            throw new Exception("Не найдено ни одного склада Wildberries");
        }

        return list[0].id;
    }

    private async Task ReceiveEtkProductsData(Func<Task<WildberriesEtkProductUpdateEntry[]>> productsDataReader)
    {
        logger.LogTrace("ReceiveEtkProductsData START");

        WildberriesEtkProductUpdateEntry[] productsData = await productsDataReader();
        etkIdTopriceMap = productsData.ToDictionary(i => i.ProductId, i => i.PriceInRUB);
        etkIdToQuantity = productsData.ToDictionary(i => i.ProductId, i => i.Quantity);

        logger.LogTrace("ReceiveEtkProductsData END");
    }

    private async Task ReceiveWbProductsData()
    {
        etkIdToWbBarcode.Clear();
        etkIdToWbNMID.Clear();

        //Если товаров больше MAX_PRODUCTS_PER_PAGE - то тут нужна пагинация
        var uri = $"https://suppliers-api.wildberries.ru/content/v2/get/cards/list?locale=ru";

        try
        {
            var res = await httpClient.PostAsJsonAsync(uri, WBCardListPayload.Empty);
            var productsData = await res.Content.ReadFromJsonAsync<WBCardListResponse>();
            logger.LogTrace("ReceiveWbProductsData");

            if (productsData?.cards?.Count == 0)
            {
                throw new Exception("Ошибка считывания о товара по API Wildberries");
            }

            if (productsData?.cards.Count >= MAX_PRODUCTS_PER_PAGE)
            {
                throw new NotSupportedException("Пагинация товаров в API Wildberries не поддерживается (реализована)");
            }

            foreach (var card in productsData.cards)
            {
                try
                {
                    etkIdToWbBarcode[card.vendorCode] = card.sizes[0].skus[0];
                    etkIdToWbNMID[card.vendorCode] = card.nmID;
                }
                catch
                {
                    //пропускаем карточку товара
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task ClearStock(int warehouseId)
    {
        var uri = $"https://suppliers-api.wildberries.ru/api/v3/stocks/{warehouseId}";

        if (etkIdToWbBarcode.Values.Count >= MAX_PRODUCTS_PER_PAGE)
        {
            throw new NotSupportedException("Пагинация товаров не реализована");
        }

        var payload = new WBQuantityClearPayload()
        {
            skus = etkIdToWbBarcode.Values.ToList()
        };

        logger.LogTrace("ClearStock");
        //var res = await httpClient.DeleteAsJsonAsync(uri, payload);
    }

    private async Task UpdateQuantity(int warehouseId)
    {
        string uri = $"https://suppliers-api.wildberries.ru/api/v3/stocks/{warehouseId}";

        if (etkIdToWbBarcode.Values.Count >= MAX_PRODUCTS_PER_PAGE)
        {
            throw new NotSupportedException("Пагинация товаров не реализована");
        }

        WBQuantityUpdatePayload payload = new()
        {
            stocks = etkIdToWbBarcode.Select(i => new WBQuantityUpdatePayload_Stock()
            {
                sku = i.Value,
                amount = etkIdToQuantity.ContainsKey(i.Key) ? etkIdToQuantity[i.Key] : 0
            }).ToList()
        };

        logger.LogTrace("UpdateQuantity");
        //var res = await httpClient.PutAsJsonAsync(uri, payload);
    }

    private async Task UpdatePrices()
    {
        string uri = "https://discounts-prices-api.wb.ru/api/v2/upload/task";

        var payload = new WBPriceUpdatePayload()
        {
            data = etkIdToWbNMID.Select(i => new WBProductPriceUpdate()
            {
                nmID = i.Value,
                price = etkIdTopriceMap.ContainsKey(i.Key) ? etkIdTopriceMap[i.Key] : 0
            }).ToList()
        };

        logger.LogTrace("UpdatePrices");
        //var res = await httpClient.PostAsJsonAsync(uri, payload);
    }
}
