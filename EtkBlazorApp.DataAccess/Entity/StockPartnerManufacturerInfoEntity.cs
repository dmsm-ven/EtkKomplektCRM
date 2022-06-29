using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ManufacturerAvaibleStocksEntity
    {
        public int manufacturer_id { get; set; }
        public string stock_ids { get; set; }
    }
    public class StockPartnerManufacturerInfoEntity
    {
        public int stock_partner_id { get; set; }
        public string name { get; set; }
        public int total_products { get; set; }
        public int total_quantity { get; set; }
    }

    public class StockPartnerLinkedManufacturerInfoEntity
    {
        public int manufacturer_id { get; set; }
        public string name { get; set; }
        public int total_products { get; set; }
        public int total_quantity { get; set; }
    }
}
