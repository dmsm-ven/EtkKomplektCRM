using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class PriceListTemplateEntity
    {
        public string id { get; set; }
        public string title { get; set; }
        public string group_name { get; set; }
        public string description { get; set; }
        public string image { get; set; }     
        public decimal discount { get; set; }
        public bool nds { get; set; }

        public string remote_uri { get; set; }
        public int? remote_uri_method_id { get; set; }
        public string remote_uri_method_name { get; set; }

        public int content_type_id { get; set; }
        public string content_type_name { get; set; }
    }
}
