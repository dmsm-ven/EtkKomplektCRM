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
        public int city_id { get; set; }
        public int shipment_period { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string description { get; set; }
        public string phone_number { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public bool show_name_for_all { get; set; }
    }
}
