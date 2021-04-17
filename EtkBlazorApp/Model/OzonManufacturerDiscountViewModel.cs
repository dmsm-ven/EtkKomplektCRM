using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class OzonManufacturerDiscountViewModel
    {
        public string Manufacturer { get; }

        public int Id { get; set; }
        public decimal Discount { get; set; }
        public bool IsChecked { get; set; }

        public OzonManufacturerDiscountViewModel(string manufacturerName)
        {
            Manufacturer = manufacturerName;
        }
    }
}
