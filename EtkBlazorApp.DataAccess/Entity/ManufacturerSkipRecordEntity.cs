using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class ManufacturerSkipRecordEntity
    {
        public string price_list_guid { get; set; }
        public int manufacturer_id { get; set; }
        public string list_type { get; set; }
        public string name { get; set; }
    }
}
