using System;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class DiscountToCategoryEntity
    {
        public int category_id { get; set; }
        public string category_name { get; set; }
        public decimal discount { get; set; } = 0;
        public DateTime date_start { get; set; } = DateTime.Now.Date;
        public DateTime date_end { get; set; } = DateTime.Now.Date;
    }
}
