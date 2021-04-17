using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class ManufacturerViewModel
    {
        public int manufacturer_id { get; set; }
        public string name { get; set; }
        [Required]
        public string keyword { get; set; }
        [Required]
        [Range(0, 355, ErrorMessage = "Срок поставки должен быть в диапазоне от 0 до 355 (дней)")]
        public int shipment_period { get; set; }
        public int? productsCount { get; set; }

        public string Uri => !string.IsNullOrEmpty(keyword) ? $"https://etk-komplekt.ru/{keyword}" : "#";
        public DateTime NextShipmentDate => shipment_period > 0 ? DateTime.Now.AddDays(shipment_period).Date : DateTime.Now.Date;
    }
}
