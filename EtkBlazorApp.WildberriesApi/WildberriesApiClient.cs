using EtkBlazorApp.ApiCommonExtensions;
using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.WildberriesApi.Data;
using Microsoft.Extensions.Logging;
using NLog;
using System.Net.Http.Json;


namespace EtkBlazorApp.WildberriesApi;

//TODO: Доработать следуюие моменты
// 1. Пагинация, если товаров больше 1000
// 2. Не отправлять на обновление, если статус точно такой же как у WB, либо
// 3. Отправлять только данные на товары которые изменились
public class WildberriesApiClient
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    private readonly ILogger<WildberriesApiClient> logger;
    private readonly HttpClient httpClient;
    private readonly int MAX_PRODUCTS_PER_PAGE = 1000;
    private readonly int TOTAL_STEPS = 7;
    private readonly Dictionary<string, string> etkIdToWbBarcode = new();   //ETK-123456 -> 20941237821 (баркод/ean) от WB
    private readonly Dictionary<string, int> etkIdToWbNMID = new();         //ETK-123456 -> 37372 (внутренний код товара от WB)
    private Dictionary<string, int> etkIdToPriceMap;                        //ETK-123456 -> 9900 
    private Dictionary<string, int> etkIdToQuantity;                        //ETK-123456 -> 6 

    public WildberriesApiClient(HttpClient httpClient, ILogger<WildberriesApiClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <summary>
    /// Обновить все товары (цены и остатки) на маркетплесе Wildberries в соответствии с наценками и выбранными складами в настройках маркетплейса в личном кабинете на странице /marketplace-export
    /// И токеном доступа на странице /settings вкладки "Маркетплейсы" раздела Wildberries
    /// </summary>
    /// <param name="secureToken"></param>
    /// <param name="productsDataReader"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task UpdateProducts(string secureToken,
        IEnumerable<WildberriesEtkProductUpdateEntry> productsData,
        IProgress<WildberriesUpdateProgress> progress)
    {
        nlog.Trace("UpdateProducts START");

        if (httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
        }
        httpClient.DefaultRequestHeaders.Add("Authorization", secureToken);

        progress?.Report(new WildberriesUpdateProgress(1, TOTAL_STEPS, "Получаем список товаров которые есть на WB"));
        await ReceiveWbProductsData();

        progress?.Report(new WildberriesUpdateProgress(2, TOTAL_STEPS, "Преобразование товаров ЕТК в словари остатков и цен"));
        ConvertEtkProductsToDictionaries(productsData);

        progress?.Report(new WildberriesUpdateProgress(3, TOTAL_STEPS, "Получаем список складов WB и берем первый"));
        int warehouseId = await GetWarehouseId();

        progress?.Report(new WildberriesUpdateProgress(4, TOTAL_STEPS, "Выполнение API запроса к WB на обнуление остатков"));
        await ClearStock(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(5, TOTAL_STEPS, "Выполнение API запроса к WB на обновление остатков"));
        await UpdateQuantity(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(6, TOTAL_STEPS, "Выполнение API запроса к WB на обновление цен"));
        await UpdatePrices();

        progress?.Report(new WildberriesUpdateProgress(TOTAL_STEPS, TOTAL_STEPS, "Конец обновление остатков и цен WB"));

        nlog.Trace("UpdateProducts END");
    }

    //STEP 1
    private async Task ReceiveWbProductsData()
    {
        etkIdToWbBarcode.Clear();
        etkIdToWbNMID.Clear();

        //Если товаров больше MAX_PRODUCTS_PER_PAGE - то тут нужна пагинация
        var uri = $"https://suppliers-api.wildberries.ru/content/v2/get/cards/list?locale=ru";

        try
        {
            var res = await httpClient.PostAsJsonAsync(uri, WBCardListPayload.Empty);
            WBCardListResponse? productsData = null;
            if (res.IsSuccessStatusCode)
            {
                productsData = await res.Content.ReadFromJsonAsync<WBCardListResponse>();
                nlog.Trace("ReceiveWbProductsData COUNT: {productsReaded}", productsData?.cards.Count);
            }
            else
            {
                string responseMessage = await res.Content.ReadAsStringAsync();
                nlog.Warn("Ошибка загрузки товаров WB: {msg}", responseMessage);
            }


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
                    nlog.Warn("Ошибка парсинга карточки товара {cardName}", card?.vendorCode ?? "<Без артикула>");
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    //STEP 2
    private void ConvertEtkProductsToDictionaries(IEnumerable<WildberriesEtkProductUpdateEntry> productsData)
    {
        etkIdToPriceMap = productsData.ToDictionary(i => i.ProductId, i => i.PriceInRUB);
        etkIdToQuantity = productsData.ToDictionary(i => i.ProductId, i => i.Quantity);
    }

    //STEP 3
    private async Task<int> GetWarehouseId()
    {
        var uri = $"https://suppliers-api.wildberries.ru/api/v3/warehouses";

        var list = await httpClient.GetFromJsonAsync<WBWarehouseList>(uri);

        if (list == null || list.Count == 0 || list[0].id == 0)
        {
            throw new Exception("Не найдено ни одного склада Wildberries");
        }
        if (list.Count > 1)
        {
            throw new Exception("Найден дополнительный склад WB логика для обработки которого не реализована");
        }

        return list[0].id;
    }

    //STEP 4
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

        var response = await httpClient.DeleteAsJsonAsync(uri, payload);
        var responseMessage = await response.Content.ReadAsStringAsync();
        nlog.Trace("ClearStock Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);
    }

    //STEP 5
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
                amount = ((etkIdToPriceMap.ContainsKey(i.Key) && etkIdToPriceMap[i.Key] > 0)
                    ? (etkIdToQuantity.ContainsKey(i.Key) ? etkIdToQuantity[i.Key] : 0)
                    : 0)
            }).ToList()
        };

        //nlog.Trace("UpdateQuantity PAYLOAD: {payload}", JsonSerializer.Serialize(payload));
        var response = await httpClient.PutAsJsonAsync(uri, payload);
        var responseMessage = await response.Content.ReadAsStringAsync();
        nlog.Trace("UpdateQuantity Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);

    }

    //STEP 6
    private async Task UpdatePrices()
    {
        string uri = "https://discounts-prices-api.wb.ru/api/v2/upload/task";

        var payload = new WBPriceUpdatePayload()
        {
            data = etkIdToWbNMID.Select(i => new WBProductPriceUpdate()
            {
                nmID = i.Value,
                price = etkIdToPriceMap.ContainsKey(i.Key) ? etkIdToPriceMap[i.Key] : 0
            }).ToList()
        };

        //nlog.Trace("UpdatePrices PAYLOAD: {payload}", JsonSerializer.Serialize(payload));

        var response = await httpClient.PostAsJsonAsync(uri, payload);
        var responseMessage = await response.Content.ReadAsStringAsync();
        nlog.Trace("UpdatePrices Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);
    }
}