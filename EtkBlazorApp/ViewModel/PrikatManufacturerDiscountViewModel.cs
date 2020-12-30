using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.ViewModel
{
    public class PrikatManufacturerDiscountViewModel
    {
        public int Id { get; set; }

        [Range(-99, maximum: 999)]
        public int Discount1 { get; set; }
        [Range(-99, maximum: 999)]
        public int Discount2 { get; set; }
        public bool IsChecked { get; set; }
        public string Manufacturer { get; }

        public PrikatManufacturerDiscountViewModel(string manufacturerName)
        {
            Manufacturer = manufacturerName;
        }
    }
}
