using EtkBlazorApp.Core.Data.Wildberries;

namespace EtkBlazorApp.WildberriesApi;

public class WildberriesApiClient
{
    private readonly HttpClient client;
    private readonly int TOTAL_STEPS = 6;

    public WildberriesApiClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task UpdateProducts(string secureToken,
        Func<Task<WildberriesEtkProductUpdateEntry[]>> productsDataReader,
        IProgress<WildberriesUpdateProgress> progress)
    {
        progress?.Report(new WildberriesUpdateProgress(1, TOTAL_STEPS, "Получаем список товаров которые есть на Wildberries"));
        progress?.Report(new WildberriesUpdateProgress(2, TOTAL_STEPS, "Запрос на информацию по товарам от БД"));
        progress?.Report(new WildberriesUpdateProgress(3, TOTAL_STEPS, "Получаем список складов от WB"));
        progress?.Report(new WildberriesUpdateProgress(4, TOTAL_STEPS, "Выполнение API запроса на обнуление остатков"));
        progress?.Report(new WildberriesUpdateProgress(5, TOTAL_STEPS, "Выполнение API запроса на обновление остатков"));
        progress?.Report(new WildberriesUpdateProgress(6, TOTAL_STEPS, "Выполнение API запроса на обновление цен"));

        await Task.Yield();
    }
}
