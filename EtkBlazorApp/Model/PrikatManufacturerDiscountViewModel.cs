using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class PrikatManufacturerDiscountViewModel
    {       
        public int Manufacturer_id { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        public bool IsChecked { get; set; }
        public string Manufacturer { get; set; }
        public string CurrencyCode { get; set; }

    }
}
