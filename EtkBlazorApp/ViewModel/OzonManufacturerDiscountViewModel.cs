using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class OzonManufacturerDiscountViewModel
    {
        public int Id { get; set; }

        [Range(-99, maximum: 999)]
        public int Discount { get; set; }
        public bool IsChecked { get; set; }
        public string Manufacturer { get; }

        public OzonManufacturerDiscountViewModel(string manufacturerName)
        {
            Manufacturer = manufacturerName;
        }
    }
}
