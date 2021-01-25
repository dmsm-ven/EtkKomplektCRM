using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class PrikatManufacturerDiscountViewModel : ViewModelBase
    {
        private int discount1, discount2;
        private bool isChecked;

        public int Id { get; set; }

        [Range(-99, maximum: 999)]
        public int Discount1 { get => discount1; set => Set(ref discount1, value); }
        [Range(-99, maximum: 999)]
        public int Discount2 { get => discount2; set => Set(ref discount2, value); }

        public bool IsChecked { get => isChecked; set => Set(ref isChecked, value); }

        public string Manufacturer { get; }

        public PrikatManufacturerDiscountViewModel(string manufacturerName)
        {
            Manufacturer = manufacturerName;
        }
    }
}
