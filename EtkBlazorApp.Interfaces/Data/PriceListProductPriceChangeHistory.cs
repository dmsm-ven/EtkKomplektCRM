using System;
using System.Collections.Generic;

namespace EtkBlazorApp.Core.Data;

public class PriceListProductPriceChangeHistory
{
    public IReadOnlyList<ProductPriceChangeHistoryItem> Data { get; init; } = new List<ProductPriceChangeHistoryItem>();
    public string PriceListGuid { get; init; }
    public string PriceListName { get; init; }
    public double MinimumOverpricePercent { get; init; }
}
