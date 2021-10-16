using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class PriceListTemplateEntity
    {
        public string id { get; set; }
        public string title { get; set; }
        public string group_name { get; set; }
        public string description { get; set; }
        public string image { get; set; }     
        public decimal discount { get; set; }
        public bool nds { get; set; }

        public string remote_uri { get; set; }
        public int? remote_uri_method_id { get; set; }
        public string remote_uri_method_name { get; set; }

        public string email_criteria_subject { get; set; }
        public string email_criteria_sender { get; set; }
        public string email_criteria_file_name_pattern { get; set; }
        public int email_criteria_max_age_in_days { get; set; }

        public int? stock_partner_id { get; set; }

        public string credentials_login { get; set; }
        public string credentials_password { get; set; }

        public List<QuantityMapRecordEntity> quantity_map { get; set; }
        public List<ManufacturerMapRecordEntity> manufacturer_name_map { get; set; }
        public List<ManufacturerSkipRecordEntity> manufacturer_skip_list { get; set; }
    }
}
