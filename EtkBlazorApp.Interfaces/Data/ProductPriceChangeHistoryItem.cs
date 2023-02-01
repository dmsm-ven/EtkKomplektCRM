using System;

namespace EtkBlazorApp.Core.Data;

public class ProductPriceChangeHistoryItem
{
    public string ProductName { get; init; }

    public int ProductId { get; init; }

    public decimal Price { get; init; }

    public DateTime DateTime { get; init; }

    public double ChangePercent
    {
        get
        {
            if (PreviousItem != null && PreviousItem.Price != 0 && Price != 0)
            {
                var percent = ((double)Price / (double)PreviousItem.Price) - 1;
                return percent;
            }
            return 0;
        }
    }

    public decimal ChangeInCurrency
    {
        get
        {
            if (PreviousItem != null && PreviousItem.Price != 0 && Price != 0)
            {
                return Price - PreviousItem.Price;
            }
            return 0;
        }
    }

    public ProductPriceChangeHistoryItem PreviousItem { get; init; }
}
