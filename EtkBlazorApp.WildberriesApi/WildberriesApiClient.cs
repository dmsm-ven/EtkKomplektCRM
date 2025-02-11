using EtkBlazorApp.ApiCommonExtensions;
using EtkBlazorApp.Core.Data.Wildberries;
using EtkBlazorApp.WildberriesApi.Data;
using Microsoft.Extensions.Logging;
using NLog;
using System.Net.Http.Json;


namespace EtkBlazorApp.WildberriesApi;

//TODO: Можно доработать следуюие моменты
// Отправлять только данные на товары которые изменились, что уменьшит количество запросов. Но усложнит логику, пока лимита хватает
public class WildberriesApiClient
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
    private const int MAX_PRODUCTS_PER_PAGE_FOR_STOCK = 1000;
    private const int SPAM_CHECK_TRY_COUNT = 5; // 1000 * 5 = 5000 товаров, если больше - то нужно переработать проверку с этим полем
    private readonly ILogger<WildberriesApiClient> logger;
    private readonly HttpClient httpClient;
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

        int currentStep = 1;

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Получаем список товаров которые есть на WB"));
        await ReceiveWbProductsData();
        nlog.Trace("OK - элементы словарей: WBIDS[{wbids}] | BARCODES[{barcodes}]", etkIdToWbNMID.Count, etkIdToWbBarcode.Count);

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Преобразование товаров ЕТК в словари остатков и цен"));
        ConvertEtkProductsToDictionaries(productsData);
        nlog.Trace("Преобразовано элементов: {count}", productsData.Count());

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Получаем список складов WB и берем первый"));
        int warehouseId = await GetWarehouseId();
        nlog.Trace("WarehouseID: {id}", warehouseId);

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Выполнение API запроса к WB на обнуление остатков"));
        await ClearStock(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Выполнение API запроса к WB на обновление остатков"));
        await UpdateQuantity(warehouseId);

        progress?.Report(new WildberriesUpdateProgress(currentStep++, TOTAL_STEPS, "Выполнение API запроса к WB на обновление цен"));
        await UpdatePrices();

        progress?.Report(new WildberriesUpdateProgress(TOTAL_STEPS, TOTAL_STEPS, "Конец обновление остатков и цен WB"));
        nlog.Trace("UpdateProducts END");
    }

    /// <summary>
    /// Шаг 1. Получаем список товаров которые есть на WB
    /// </summary>
    /// <returns></returns>
    private async Task ReceiveWbProductsData()
    {
        etkIdToWbBarcode.Clear();
        etkIdToWbNMID.Clear();

        try
        {
            var cards = await GetAllWbCards();

            foreach (var card in cards)
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

    /// <summary>
    /// Шаг 2. Преобразование товаров ЕТК в словари остатков и цен
    /// </summary>
    /// <param name="productsData"></param>
    private void ConvertEtkProductsToDictionaries(IEnumerable<WildberriesEtkProductUpdateEntry> productsData)
    {
        etkIdToPriceMap = productsData.ToDictionary(i => i.ProductId, i => i.PriceInRUB);
        etkIdToQuantity = productsData.ToDictionary(i => i.ProductId, i => i.Quantity);
    }

    /// <summary>
    /// Шаг 3. Получаем список складов WB и берем первый
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<int> GetWarehouseId()
    {
        var uri = $"https://marketplace-api.wildberries.ru/api/v3/warehouses";

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

    /// <summary>
    /// Шаг 4. Выполнение API запроса к WB на обнуление остатков
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private async Task ClearStock(int warehouseId)
    {
        var uri = $"https://marketplace-api.wildberries.ru/api/v3/stocks/{warehouseId}";

        int currentPage = 0;
        int lastPage = (int)Math.Ceiling((double)etkIdToWbBarcode.Values.Count / MAX_PRODUCTS_PER_PAGE_FOR_STOCK);

        do
        {
            var currentPageSkus = etkIdToWbBarcode.Values.Skip(currentPage * MAX_PRODUCTS_PER_PAGE_FOR_STOCK).Take(MAX_PRODUCTS_PER_PAGE_FOR_STOCK).ToList();

            var payload = new WBQuantityClearPayload()
            {
                skus = currentPageSkus
            };

            var response = await httpClient.DeleteAsJsonAsync(uri, payload);
            var responseMessage = await response.Content.ReadAsStringAsync();

            nlog.Trace("ClearStock Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);

            currentPage++;
            // УБРАТЬ, ЕСЛИ ТОВАРОВ БОЛЬШЕ ЧЕМ 5000
            if (currentPage > SPAM_CHECK_TRY_COUNT)
            {
                break;
            }
        } while (currentPage < lastPage);
    }

    /// <summary>
    /// Шаг 5. Выполнение API запроса к WB на обновление остатков
    /// </summary>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private async Task UpdateQuantity(int warehouseId)
    {
        string uri = $"https://marketplace-api.wildberries.ru/api/v3/stocks/{warehouseId}";

        int currentPage = 0;
        int lastPage = (int)Math.Ceiling((double)etkIdToWbBarcode.Values.Count / MAX_PRODUCTS_PER_PAGE_FOR_STOCK);

        do
        {
            var stocksData = etkIdToWbBarcode
                .Skip(currentPage * MAX_PRODUCTS_PER_PAGE_FOR_STOCK).Take(MAX_PRODUCTS_PER_PAGE_FOR_STOCK)
                .Select(i => new WBQuantityUpdatePayload_Stock()
                {
                    sku = i.Value,
                    amount = ((etkIdToPriceMap.ContainsKey(i.Key) && etkIdToPriceMap[i.Key] > 0)
                        ? (etkIdToQuantity.ContainsKey(i.Key) ? etkIdToQuantity[i.Key] : 0)
                        : 0)
                }).ToList();

            WBQuantityUpdatePayload payload = new()
            {
                stocks = stocksData
            };

            //nlog.Trace("UpdateQuantity PAYLOAD: {payload}", JsonSerializer.Serialize(payload));
            var response = await httpClient.PutAsJsonAsync(uri, payload);
            var responseMessage = await response.Content.ReadAsStringAsync();
            nlog.Trace("UpdateQuantity Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);

            currentPage++;
            //Удалить если товаров больше 5000
            if (currentPage > SPAM_CHECK_TRY_COUNT)
            {
                break;
            }
        } while (currentPage < lastPage);
    }

    /// <summary>
    /// Шаг 6. Выполнение API запроса к WB на обновление цен
    /// </summary>
    /// <returns></returns>
    private async Task UpdatePrices()
    {
        string uri = "https://discounts-prices-api.wb.ru/api/v2/upload/task";

        int currentPage = 0;
        int lastPage = (int)Math.Ceiling((double)etkIdToWbBarcode.Values.Count / MAX_PRODUCTS_PER_PAGE_FOR_STOCK);

        do
        {
            var priceData = etkIdToWbNMID
                .Skip(currentPage * MAX_PRODUCTS_PER_PAGE_FOR_STOCK).Take(MAX_PRODUCTS_PER_PAGE_FOR_STOCK)
                .Select(i => new WBProductPriceUpdate()
                {
                    nmID = i.Value,
                    price = etkIdToPriceMap.ContainsKey(i.Key) ? etkIdToPriceMap[i.Key] : 0
                }).ToList();


            var payload = new WBPriceUpdatePayload()
            {
                data = priceData
            };

            //nlog.Trace("UpdatePrices PAYLOAD: {payload}", JsonSerializer.Serialize(payload));

            var response = await httpClient.PostAsJsonAsync(uri, payload);
            var responseMessage = await response.Content.ReadAsStringAsync();
            nlog.Trace("UpdatePrices Response | StatusCode = {code}, Message = {message}", response.StatusCode, responseMessage);

            currentPage++;

            //Убрать если товаров больше 5000
            if (currentPage > SPAM_CHECK_TRY_COUNT)
            {
                break;
            }
        } while (currentPage < lastPage);
    }

    /// <summary>
    /// Загружаем список товаров от WB для того что бы узнать соответствие [артикула етк] --> [артикулу WB]
    /// </summary>
    /// <returns></returns>
    private async Task<List<WBCardListResponse_Card>> GetAllWbCards()
    {
        var uri = $"https://content-api.wildberries.ru/content/v2/get/cards/list?locale=ru";
        const int PRODUCTS_PER_REQUEST = 100;
        const int MAX_REQUEST_TRY_COUNT = 50;

        var list = new List<WBCardListResponse_Card>();
        int readedProducts = 0;
        int tryCount = 0;

        DateTimeOffset lastUpdatedAt = DateTimeOffset.MinValue;
        int lastNmID = 0;

        do
        {
            var payload = list.Count == 0 ?
                WBCardListPayloadFactory.Empty :
                WBCardListPayloadFactory.GetPayloadWithPagination(PRODUCTS_PER_REQUEST, lastUpdatedAt, lastNmID);

            using HttpResponseMessage? res = await httpClient.PostAsJsonAsync(uri, payload);

            var responseData = await res.Content.ReadFromJsonAsync<WBCardListResponse>();

            if (responseData?.cards == null)
            {
                break;
            }

            lastUpdatedAt = responseData.cursor.updatedAt;
            lastNmID = responseData.cursor.nmID;
            readedProducts = responseData.cursor.total;

            list.AddRange(responseData.cards);

            if (readedProducts == 0)
            {
                break;
            }

            //50 запросов = 5000 товаров, если товаров на WB больше 5000 то необходимо убрать эту проверку
            if (++tryCount > MAX_REQUEST_TRY_COUNT) // - на всякий случай делаем проверку, что бы не заспамить API, и не быть заблокированными
            {
                break;
            }
        } while (readedProducts == PRODUCTS_PER_REQUEST);


        if (list.GroupBy(p => p.vendorCode).Count() > (MAX_REQUEST_TRY_COUNT - 1) * PRODUCTS_PER_REQUEST)
        {
            throw new NotSupportedException("Необходимо убрать проверку на максимальное число запросов");
        }

        return list;
    }
}