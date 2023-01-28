using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class PartnerEntity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string address { get; set; }
        public string description { get; set; }
        public int priority { get; set; }
        public decimal discount { get; set; }
        public string contact_person { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public DateTime? price_list_last_access { get; set; }
        public string price_list_password { get; set; }
    }
}
