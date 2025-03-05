using System.Collections.Generic;

namespace EtkBlazorApp.DataAccess.Entity.Manufacturer
{
    public class MarketplaceBrandExportEntity
    {
        public string manufacturer_name { get; set; }
        public int manufacturer_id { get; set; }
        public decimal discount { get; set; }
        public string checked_stocks { get; set; }
        public List<StockPartnerEntity> checked_stocks_list { get; set; } = new List<StockPartnerEntity>();
    }

    public class MarketplaceStepDiscountEntity
    {
        public string marketplace { get; set; }
        public int price_border_in_rub { get; set; }
        public decimal ratio { get; set; }
    }
}
