using System;

namespace EtkBlazorApp.Data
{
    public class ManufacturerViewModel
    {
        public int manufacturer_id { get; set; }
        public string name { get; set; }
        public string keyword { get; set; }
        public int shipment_period { get; set; }
        public int? productsCount { get; set; }

        public string Uri => !string.IsNullOrEmpty(keyword) ? $"https://etk-komplekt.ru/{keyword}" : "#";
        public DateTime NextShipmentDate => shipment_period > 0 ? DateTime.Now.AddDays(shipment_period).Date : DateTime.Now.Date;
    }
}
