using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers;


public class PriceListPriceHistoryManager
{
    private readonly IPriceListUpdateHistoryRepository repo;

    public PriceListPriceHistoryManager(IPriceListUpdateHistoryRepository repo)
    {
        this.repo = repo;
    }

    public async Task SavePriceChangesHistory(IEnumerable<PriceLine> products, IEnumerable<ProductUpdateData> updateData)
    {
        var priceData = updateData
            .Where(p => p.price.HasValue)
            .ToDictionary(p => p.product_id, p => p.price.Value);

        //TODO: проверить, тут может быть ошибка, если загружается несколько прайс-листов за раз
        string guid = products
            .GroupBy(p => p.Template)
            .Single()
            .Key
            .GetType()
            .GetPriceListGuidByType();

        await repo.SavePriceListUpdateProductsPriceData(guid, priceData);
    }
}
