using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Model
{
    public class ShopAccountEntity
    {
        public int website_id { get; set; }

        public string uri { get; set; }   
        
        public string title { get; set; }

        [JsonEncrypt]
        public string ftp_host { get; set; }

        [JsonEncrypt]
        public string ftp_login { get; set; }

        [JsonEncrypt]
        public string ftp_password { get; set; }

        [JsonEncrypt]
        public string db_host { get; set; }

        [JsonEncrypt]
        public string db_login { get; set; }

        [JsonEncrypt]
        public string db_password { get; set; }
    }
}
