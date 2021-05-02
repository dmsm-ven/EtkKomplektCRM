using EtkBlazorApp.BL;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp
{
    public class PriceListTemplateItemViewModel
    {
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

        [Required]
        public int PriceListTypeId { get; set; }
        public string PriceListTypeName { get; set; }

        public string RemoteUrl { get; set; }
        public int? RemoteUrlMethodId { get; set; }
        public string RemoteUrlMethodName { get; set; }

        public Type Type { get; private set; }

        public PriceListTemplateItemViewModel(string guid)
        {
            Guid = guid;

            Type = Assembly
                .GetAssembly(typeof(PriceListTemplateGuidAttribute))
                .GetTypes()
                .FirstOrDefault(type => type.GetCustomAttribute<PriceListTemplateGuidAttribute>()?.Guid == guid);
        }
    }
}
