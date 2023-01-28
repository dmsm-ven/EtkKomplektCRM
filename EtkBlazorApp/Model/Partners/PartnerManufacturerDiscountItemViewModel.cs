using System;

namespace EtkBlazorApp
{
    public class PartnerManufacturerDiscountItemViewModel
    {
        public string ManufacturerName { get; set; }
        public Guid PartnerGuid { get; set; }
        public int ManufacturerId { get; set; }
        public decimal? Discount { get; set; }
    }
}
