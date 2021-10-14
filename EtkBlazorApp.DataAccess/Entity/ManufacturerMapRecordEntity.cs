using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ManufacturerMapRecordEntity
    {
        public string price_list_guid { get; set; }
        public string text { get; set; }
        public int manufacturer_id { get; set; }
        public string name { get; set; }
    }
}
