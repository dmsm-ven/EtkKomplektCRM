using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ProductSpecialEntity
    {
        public int product_special_id { get; set; }
        public int product_id { get; set; }
        public string name { get; set; }
        public string manufacturer { get; set; }
        public string base_currency_code { get; set; }

        public decimal NewPriceInRub { get; set; }
        public decimal NewPriceInCurrency { get; set; }
        public decimal RegularPriceInRub { get; set; }
        public decimal RegularPriceInCurrency { get; set; }

        public DateTime date_start { get; set; }
        public DateTime date_end { get; set; }
    }
}
