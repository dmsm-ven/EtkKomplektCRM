namespace EtkBlazorApp.BL
{
    public struct VseInstrumentiReportOptions
    {
        public bool HasEan { get; init; }
        public bool StockGreaterThanZero { get; init; }
        public bool Adding1CStockQuantity { get; init; }
        public string GLN { get; init; }
    }
}
