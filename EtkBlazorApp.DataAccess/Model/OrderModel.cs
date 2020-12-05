using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Model
{
    public class OrderModel
    {
        public DateTime date_added { get; set; }
        public int order_id { get; set; }
        public string payment_city { get; set; }
        public string payment_firstname { get; set; }
        public decimal total { get; set; }
        public string order_status { get; set; }
    }
}
