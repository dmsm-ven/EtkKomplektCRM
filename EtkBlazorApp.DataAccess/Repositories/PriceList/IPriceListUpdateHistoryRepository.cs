using EtkBlazorApp.DataAccess.Entity.PriceList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Repositories;

public interface IPriceListUpdateHistoryRepository
{
    Task SavePriceListUpdateProductsPriceData(string guid, IEnumerable<KeyValuePair<int, decimal>> productToPrice);
    Task<List<PriceListUpdateEntryEntity>> GetPriceListUpdateEntries(string guid);
    Task<List<PriceListUpdateProductHistory>> GetProductHistory(int update_id);
}

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

    public async Task<List<PriceListUpdateProductHistory>> GetProductHistory(int update_id)
    {
        string sql = "SELECT * FROM etk_app_price_list_update_product_history WHERE update_id = @update_id";
        var data = await database.GetList<PriceListUpdateProductHistory, dynamic>(sql, new { update_id });
        return data;
    }

    public async Task SavePriceListUpdateProductsPriceData(string guid, IEnumerable<KeyValuePair<int, decimal>> productToPrice)
    {
        if (productToPrice.Count() == 0) { return; }

        await database.ExecuteQuery("INSERT INTO etk_app_price_list_update_entry (price_list_id) VALUES (@guid)", new { guid });

        int update_id = await database.GetScalar<int>("SELECT max(update_id) FROM etk_app_price_list_update_entry");


        var sb = new StringBuilder()
            .AppendLine("INSERT INTO etk_app_price_list_update_product_history (update_id, product_id, price) VALUES");

        foreach (var kvp in productToPrice)
        {
            sb.Append($"({update_id}, {kvp.Key}, {kvp.Value.ToString().Replace(",", ".")}),");
        }

        var sql = sb.ToString().Trim(',') + ";";

        await database.ExecuteQuery(sql);
    }
}
