using System.Collections.Generic;

namespace EtkBlazorApp.DataAccess.Entity.Marketplace
{
    public class PrikatReportTemplateEntity
    {
        public int template_id { get; set; }
        public int manufacturer_id { get; set; }
        public decimal discount { get; set; }
        public bool enabled { get; set; }

        public string manufacturer_name { get; set; }
        public string currency_code { get; set; }

        public string checked_stocks { get; set; } = string.Empty;
        public List<StockPartnerEntity> checked_stocks_list { get; set; } = new List<StockPartnerEntity>();
    }
}
