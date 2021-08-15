using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp
{
    public class StockPartnerViewModel
    {
        public int Id { get; set; }
        public int CityId { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [StringLength(64)]
        public string City { get; set; }

        [StringLength(64)]
        public string PhoneNumber { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        public bool ShowNameForAll { get; set; }

        [Range(0, 365, ErrorMessage = "Срок поставки (в днях) должен быть от 0 до 365")]
        public int ShipmentPeriodInDays { get; set; }

        public DateTime NextShipmentDate => ShipmentPeriodInDays > 0 ?
            DateTime.Now.AddDays(ShipmentPeriodInDays).Date :
            DateTime.Now.Date;
    }
}
