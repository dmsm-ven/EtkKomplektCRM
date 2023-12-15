using EtkBlazorApp.BL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.Model.PriceListTemplate
{
    public class PriceListTemplateItemViewModel
    {
        public Type Type => Guid.GetPriceListTypeByGuid();

        [Required]
        public string Guid { get; set; }
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Выберите изображение")]
        public string Image { get; set; }

        [Required]
        public string GroupName { get; set; }

        public decimal Discount { get; set; }
        public bool Nds { get; set; }

        public string RemoteUrl { get; set; }
        public int? RemoteUrlMethodId { get; set; }
        public string RemoteUrlMethodName { get; set; }

        public int? LinkedStockId { get; set; }

        public string EmailSearchCriteria_Subject { get; set; }
        public string EmailSearchCriteria_Sender { get; set; }
        public string EmailSearchCriteria_FileNamePattern { get; set; }
        public int EmailSearchCriteria_MaxAgeInDays { get; set; }

        public string Cridentials_Login { get; set; }
        public string Cridentials_Password { get; set; }

        //Данные вкладки дополнительные настройки
        public Dictionary<string, int> QuantityMap { get; set; } = new();
        public Dictionary<string, string> ManufacturerNameMap { get; set; } = new();
        public Dictionary<string, string> ModelMap { get; set; } = new();

        public List<ManufacturerDiscountItemViewModel> ManufacturerDiscountMap { get; set; } = new();
        public List<ManufacturerDiscountItemViewModel> ManufacturerPurchaseDiscountMap { get; set; } = new();
        public List<ManufacturerSkipItemViewModel> ManufacturerSkipList { get; set; } = new();
    }

}
