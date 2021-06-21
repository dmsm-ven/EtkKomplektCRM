using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class ManufacturerViewModel
    {
        public int Id { get; }
        public string name { get; set; }
        public string keyword { get; set; }
        public int ShipmentPeriodInDays { get; set; }
        public int OldShipmentPeriod { get; set; }
        public int? productsCount { get; set; }

        public string Uri => !string.IsNullOrEmpty(keyword) ? $"https://etk-komplekt.ru/{keyword}" : "#";

        public DateTime NextShipmentDate => ShipmentPeriodInDays > 0 ?
            DateTime.Now.AddDays(ShipmentPeriodInDays).Date :
            DateTime.Now.Date;

        public ManufacturerViewModel(int id, int shipmentPeriod)
        {
            Id = id;
            ShipmentPeriodInDays = OldShipmentPeriod = shipmentPeriod;
        }
    }

    public class StockPartnerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int ShipmentPeriodInDays { get; set; }
        public int OldShipmentPeriod { get; set; }

        public DateTime NextShipmentDate => ShipmentPeriodInDays > 0 ?
            DateTime.Now.AddDays(ShipmentPeriodInDays).Date :
            DateTime.Now.Date;

        public StockPartnerViewModel(int id, int shipmentPeriod)
        {
            Id = id;
            ShipmentPeriodInDays = OldShipmentPeriod = shipmentPeriod;
        }
    }
}
