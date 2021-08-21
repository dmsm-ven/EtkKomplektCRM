using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ManufacturerEntity
    {
        public int manufacturer_id { get; set; }
        public string name { get; set; }
        public string keyword { get; set; }
        public int shipment_period { get; set; }
        public int? productsCount { get; set; }
    }

    public class PartnerManufacturerDiscountEntity
    {
        public string partner_id { get; set; }
        public int manufacturer_id { get; set; }      
        public string name { get; set; }
        public decimal? discount { get; set; }
    }
}
