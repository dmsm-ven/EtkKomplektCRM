﻿using EtkBlazorApp.Core.Data.Wildberries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Repositories.Wildberries;
public interface IWildberriesProductRepository
{
    Task<List<WildberriesEtkProductUpdateEntry>> ReadProducts();
    Task<DateTime?> GetStockLastSuccessSyncDateDate();
}

public class WildberriesProductRepository : IWildberriesProductRepository
{
    private readonly IDatabaseAccess database;

    public WildberriesProductRepository(IDatabaseAccess database)
    {
        this.database = database;
    }

    public async Task<DateTime?> GetStockLastSuccessSyncDateDate()
    {
        string sql = @"SELECT date_time
                       FROM `etk_app_log`
                       WHERE group_name = 'Маркетплейс Wildberries' AND title = 'Синхронизация выполнена'
                       ORDER BY id DESC
                       LIMIT 1";

        var dt = await database.GetScalar<DateTime>(sql);

        if (dt == default(DateTime))
        {
            return null;
        }
        return dt;
    }

    public async Task<List<WildberriesEtkProductUpdateEntry>> ReadProducts()
    {
        //Склады (суммирование) попадают только те которые указаны на странцие настроек маркетплейса
        //TODO: Wildberries цена с распродажей
        //Сюда не попадут товары которые в разделе Распродажа
        //Соответственно на WB они должны пропасть из продажи (количество 0, цена 0)
        //Либо доработать в будущем этот момент

        var sql = @"SELECT CONCAT('ETK-', p.product_id) as ProductId, 
						((p.price * (100 + mbe.discount)) / 100) as PriceInRUB,  
						SUM(IFNULL(pts.quantity, 0)) as Quantity				
					FROM oc_product p 
						JOIN oc_product_description pd ON (p.product_id = pd.product_id) 
						JOIN oc_manufacturer m ON (p.manufacturer_id = m.manufacturer_id) 
						JOIN etk_app_marketplace_brand_export mbe ON (mbe.manufacturer_id = m.manufacturer_id AND marketplace = 'Wildberries')
						LEFT JOIN oc_product_to_stock pts ON (p.product_id = pts.product_id)
						LEFT JOIN oc_product_special ps ON (p.product_id = ps.product_id AND (NOW() < date_end))
					WHERE p.status = 1 AND 
							pd.main_product = 0 AND 
							p.price > 0 AND
							(ps.product_id IS NULL) AND
							FIND_IN_SET(pts.stock_partner_id, mbe.checked_stocks) > 0						
					GROUP BY p.product_id
					ORDER BY p.product_id";

        var products = await database.GetList<WildberriesEtkProductUpdateEntry>(sql);

        return products;
    }
}
