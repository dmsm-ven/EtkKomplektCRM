using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Model
{
    public class ProductUpdateData
    {
        public int product_id { get; set; }      
        public decimal? price { get; set; }
        public int? quantity { get; set; }
        public string currency_code { get; set; }
    }
}
