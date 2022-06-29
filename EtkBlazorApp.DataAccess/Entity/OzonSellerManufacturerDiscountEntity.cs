using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class OzonSellerManufacturerDiscountEntity
    {
        public int manufacturer_id { get; set; }
        public decimal discount { get; set; }
        public bool enabled { get; set; }

        public string manufacturer_name { get; set; }
    }
}
