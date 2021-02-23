using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class OrderEntity
    {
        public DateTime date_added { get; set; }
        public int order_id { get; set; }
        public string payment_city { get; set; }
        public string payment_firstname { get; set; }
        public decimal total { get; set; }
        public string order_status { get; set; }

        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string fax { get; set; }
        public string custom_field { get; set; }
        public string payment_lastname { get; set; }
        public string payment_company { get; set; }
        public string payment_address_1 { get; set; }
        public string payment_address_2 { get; set; }
        public string payment_postcode { get; set; }
        public string payment_country { get; set; }
        public string payment_zone { get; set; }
        public string payment_address_format { get; set; }
        public string payment_custom_field { get; set; }
        public string payment_method { get; set; }
        public string payment_code { get; set; }
        public string shipping_firstname { get; set; }
        public string shipping_lastname { get; set; }
        public string shipping_company { get; set; }
        public string shipping_address_1 { get; set; }
        public string shipping_address_2 { get; set; }
        public string shipping_city { get; set; }
        public string shipping_postcode { get; set; }
        public string shipping_country { get; set; }
        public string shipping_zone { get; set; }
        public string shipping_address_format { get; set; }
        public string shipping_method { get; set; }
        public string comment { get; set; }
        public string ip { get; set; }
        public string forwarded_ip { get; set; }
    }
}
