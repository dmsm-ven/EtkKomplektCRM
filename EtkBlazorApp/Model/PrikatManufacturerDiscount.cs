using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class PrikatManufacturerDiscountViewModel
    {       
        public int Manufacturer_id { get; set; }
        public int Discount1 { get; set; }
        public int Discount2 { get; set; }
        public bool IsChecked { get; set; }
        public string Manufacturer { get; set; }
        public string CurrencyCode { get; set; }
    }
}
