using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Managers;

public class PriceListProductPriceChangeHistory
{
    public IReadOnlyList<ProductPriceChangeHistoryItem> Data { get; init; } = new List<ProductPriceChangeHistoryItem>();
}
