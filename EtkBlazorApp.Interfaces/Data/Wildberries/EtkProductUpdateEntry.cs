﻿namespace EtkBlazorApp.Core.Data.Wildberries;

public class WildberriesEtkProductUpdateEntry
{
    public string ProductId { get; init; } // ETK-000000, где цифры это ID товара в базе данных
    public int Quantity { get; init; }
    public int PriceInRUB { get; init; } // Цена в рублях
}
