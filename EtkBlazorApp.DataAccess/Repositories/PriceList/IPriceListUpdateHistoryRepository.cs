using EtkBlazorApp.DataAccess.Entity.PriceList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Repositories;

public interface IPriceListUpdateHistoryRepository
{
    Task SavePriceListUpdateProductsPriceData(string guid, Dictionary<int, decimal> productToPrice);
    Task<List<PriceListUpdateEntryEntity>> GetPriceListUpdateEntries(string guid);
    Task<Dictionary<PriceListUpdateEntryEntity, List<PriceListUpdateProductHistoryEntity>>> GetPriceListUpdateHistory(string guid);
    Task<List<PriceListUpdateProductHistoryEntity>> GetProductHistory(int update_id);
    Task<HashSet<int>> GetUniqueProductIdsInHistory(string guid);

    Task<List<ProductPriceDynamicEntry>> GetPriceDynamicForProduct(int product_id);
}

public class ProductPriceDynamicEntry
{
    public string price_list_title { get; set; }
    public decimal price { get; set; }
    public DateTime date_time { get; set; }
}

// Вынести логику в класс Менеджер
public class PriceListUpdateHistoryRepository : IPriceListUpdateHistoryRepository
{
    private readonly IDatabaseAccess database;

    public PriceListUpdateHistoryRepository(IDatabaseAccess database)
    {
        this.database = database;
    }

    public async Task<List<PriceListUpdateEntryEntity>> GetPriceListUpdateEntries(string guid)
    {
        string sql = "SELECT * FROM etk_app_price_list_update_entry WHERE price_list_id = @guid";
        var data = await database.GetList<PriceListUpdateEntryEntity, dynamic>(sql, new { guid });
        return data;
    }

    public async Task<Dictionary<PriceListUpdateEntryEntity, List<PriceListUpdateProductHistoryEntity>>> GetPriceListUpdateHistory(string guid)
    {
        var entiresDictionary = (await GetPriceListUpdateEntries(guid))
            .ToDictionary(i => i, i => new List<PriceListUpdateProductHistoryEntity>());

        foreach (var entry in entiresDictionary.Keys)
        {
            entiresDictionary[entry] = await GetProductHistory(entry.update_id);
        }

        return entiresDictionary;
    }

    public async Task<List<PriceListUpdateProductHistoryEntity>> GetProductHistory(int update_id)
    {
        string sql = "SELECT * FROM etk_app_price_list_update_product_history WHERE update_id = @update_id";
        var data = await database.GetList<PriceListUpdateProductHistoryEntity, dynamic>(sql, new { update_id });
        return data;
    }


    public async Task SavePriceListUpdateProductsPriceData(string guid, Dictionary<int, decimal> productToPrice)
    {
        if (productToPrice.Count() == 0)
        {
            return;
        }

        // Шаг 1. Добавляем вхождение загрузки
        await database.ExecuteQuery("INSERT INTO etk_app_price_list_update_entry (price_list_id) VALUES (@guid)", new { guid });
        int update_id = await database.GetScalar<int>("SELECT max(update_id) FROM etk_app_price_list_update_entry");

        // Шаг 2. Добавляем изменившиеся данные
        string sql = BuildInsertNewUpdateLinesSql(update_id, productToPrice);
        if (sql != null)
        {
            await database.ExecuteQuery(sql);
        }
    }

    public async Task<HashSet<int>> GetUniqueProductIdsInHistory(string guid)
    {
        string sql = @"SELECT DISTINCT product_id
                       FROM etk_app_price_list_update_product_history ph
                       JOIN etk_app_price_list_update_entry ue ON (ph.update_id = ue.update_id)
                       WHERE ue.price_list_id = @guid";

        var data = await database.GetList<int, dynamic>(sql, new { guid });

        return data.ToHashSet();
    }



    private string BuildInsertNewUpdateLinesSql(int update_id, Dictionary<int, decimal> lines)
    {
        var sb = new StringBuilder()
            .AppendLine("INSERT INTO etk_app_price_list_update_product_history (update_id, product_id, price) VALUES");

        foreach (var kvp in lines)
        {
            sb.Append($"({update_id}, {kvp.Key}, {kvp.Value.ToString().Replace(",", ".")}),");
        }

        var sql = sb.ToString().Trim(',') + ";";

        return sql;
    }

    public async Task<List<ProductPriceDynamicEntry>> GetPriceDynamicForProduct(int product_id)
    {
        string sql = @"SELECT ph.price, ue.date_time, t.title as price_list_title
                       FROM etk_app_price_list_update_product_history ph
                       JOIN etk_app_price_list_update_entry ue ON (ph.update_id = ue.update_id)
                       JOIN etk_app_price_list_template t ON (ue.price_list_id = t.id)
                       WHERE product_id = @product_id";

        var data = await database.GetList<ProductPriceDynamicEntry, dynamic>(sql, new { product_id });

        return data;
    }

}
