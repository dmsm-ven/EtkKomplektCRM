using EtkBlazorApp.BL;
using EtkBlazorApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp
{
    public class PriceListTemplateItemViewModel
    {
        public Type Type { get; private set; }

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

        public Dictionary<string, int> QuantityMap { get; set; } = new();
        public Dictionary<string, string> ManufacturerNameMap { get; set; } = new();
        public List<ManufacturerDiscountItemViewModel> ManufacturerDiscountMap { get; set; } = new();
        public List<ManufacturerSkipItemViewModel> ManufacturerSkipList { get; set; } = new();

        public PriceListTemplateItemViewModel(string guid)
        {
            Guid = guid;

            Type = Assembly
                .GetAssembly(typeof(PriceListTemplateGuidAttribute))
                .GetTypes()
                .FirstOrDefault(type => type.GetCustomAttribute<PriceListTemplateGuidAttribute>()?.Guid == guid);
        }
    }

    public class ManufacturerDiscountItemViewModel
    {
        public int manufacturer_id { get; set; }
        public string manufacturer_name { get; set; }
        public decimal discount { get; set; }
    }

    public class ManufacturerSkipItemViewModel
    {
        public int manufacturer_id { get; set; }
        public string Name { get; set; }
        public SkipManufacturerListType ListType { get; set; }
        public string ListTypeDescription => (ListType == SkipManufacturerListType.black_list ? "Черный список" : "Белый список");
    }

    public enum SkipManufacturerListType { black_list, white_list }

}
