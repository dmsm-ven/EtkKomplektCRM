using EtkBlazorApp.BL;
using EtkBlazorApp.Infrastructure;
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

        public int PriceListTypeId { get; set; }
        public string PriceListTypeName { get; set; }

        public string RemoteUrl { get; set; }
        public int? RemoteUrlMethodId { get; set; }
        public string RemoteUrlMethodName { get; set; }
        
        public Type Type { get; private set; }

        public string EmailSearchCriteria_Subject { get; set; }
        public string EmailSearchCriteria_Sender { get; set; }
        public string EmailSearchCriteria_FileNamePattern { get; set; }
        public int EmailSearchCriteria_MaxAgeInDays { get; set; }

        public string Cridentials_Login { get; set; }
        public string Cridentials_Password { get; set; }
        
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
