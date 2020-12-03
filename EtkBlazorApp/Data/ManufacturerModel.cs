using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess
{
    public class ManufacturerModel
    {
        public int manufacturer_id { get; set; }
        public string name { get; set; }
        public int shipment_period { get; set; }
        public int? productsCount { get; set; }

        public DateTime NextShipmentDate => shipment_period > 0 ? DateTime.Now.AddDays(shipment_period).Date : DateTime.Now.Date;
    }
}
