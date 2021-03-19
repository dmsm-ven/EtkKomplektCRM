using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class PrikatManufacturerDiscountViewModel : ViewModelBase
    {       
        public int Manufacturer_id { get; set; }

        int discount1;
        [Range(-99, maximum: 999)]
        public int Discount1 { get => discount1; set => Set(ref discount1, value); }

        int discount2;
        [Range(-99, maximum: 999)]
        public int Discount2 { get => discount2; set => Set(ref discount2, value); }

        bool isChecked;
        public bool IsChecked { get => isChecked; set => Set(ref isChecked, value); }

        public string Manufacturer { get; set; }

        string currencyCode;
        public string CurrencyCode { get => currencyCode; set => Set(ref currencyCode, value); }

    }
}
