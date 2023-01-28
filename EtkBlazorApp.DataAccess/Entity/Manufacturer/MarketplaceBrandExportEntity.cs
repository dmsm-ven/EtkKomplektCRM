using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class MarketplaceBrandExportEntity
    {
        public string manufacturer_name { get; set; }
        public int manufacturer_id { get; set; }
        public decimal discount { get; set; }
        public string checked_stocks { get; set; }
        public List<StockPartnerEntity> checked_stocks_list { get; set; } = new List<StockPartnerEntity>();        
    }
}
