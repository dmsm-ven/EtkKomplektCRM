using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class PartnerViewModel
    {
        [Required]
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string ContactPerson { get; set; }
        public decimal Discount { get; set; }

        [StringLength(16, MinimumLength = 6)]
        public string Password { get; set; }       
        
        [Range(1, 5)]
        public int Priority { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime? PriceListLastAccessDateTime { get; set; }

        public string PriceListUri
        {
            get 
            {
                if(CheckedManufacturers.Count == 0)
                {
                    return "Необходимо выбрать хотя бы одного производителя";
                }

                if (Id != Guid.Empty && !string.IsNullOrWhiteSpace(Password) && Password.Length >= 6)
                {
                    return $"https://etk-komplekt.ru/index.php?route=api/partners/getPrice&partner_id={Id}&password={Password}";
                }

                return "Ссылка на прайс-лист появится после сохранения";
            }
        }

        public List<ManufacturerViewModel> CheckedManufacturers { get; set; } = new List<ManufacturerViewModel>();
    }
}
