using System.Collections.Generic;

namespace EtkBlazorApp.BL
{
    public struct VseInstrumentiReportOptions
    {
        public bool HasEan { get; init; }
        public bool StockGreaterThanZero { get; init; }
        public string GLN { get; init; }
        public Dictionary<StockPartner, bool> UsePartnerStock { get; init; } 
    }
}
