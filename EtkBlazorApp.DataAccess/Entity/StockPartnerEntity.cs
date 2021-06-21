using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class StockPartnerEntity
    {
        public int stock_partner_id { get; set; }
        public int shipment_period { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}
