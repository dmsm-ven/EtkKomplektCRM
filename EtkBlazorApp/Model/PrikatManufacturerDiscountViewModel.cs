using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;

namespace EtkBlazorApp.Model
{
    public class PrikatManufacturerDiscountViewModel
    {
        public int TemplateId { get; set; }
        public int Manufacturer_id { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        public string Manufacturer { get; set; }
        public string CurrencyCode { get; set; }

        public string CheckedStocks { get; set; }
        public List<StockPartnerEntity> checked_stocks_list { get; set; } = new List<StockPartnerEntity>();
    }
}
