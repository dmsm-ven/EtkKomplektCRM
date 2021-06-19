using System;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class DiscountToManufacturerEntity
    {
        public int manufacturer_id { get; set; }
        public string manufacturer_name { get; set; }
        public decimal discount { get; set; } 
        public DateTime date_start { get; set; } = DateTime.Now.Date;
        public DateTime date_end { get; set; } = DateTime.Now.Date;
    }
}
