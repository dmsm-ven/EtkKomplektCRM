using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class OzonManufacturerDiscountViewModel : ViewModelBase
    {
        private int discount;
        private bool isChecked;
        
        public int Id { get; set; }

        [Range(-99, maximum: 999)]
        public int Discount { get => discount; set => Set(ref discount, value); }
        public bool IsChecked { get => isChecked; set => Set(ref isChecked, value); }

        public string Manufacturer { get; }

        public OzonManufacturerDiscountViewModel(string manufacturerName)
        {
            Manufacturer = manufacturerName;
        }
    }
}
