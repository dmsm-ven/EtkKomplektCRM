using EtkBlazorApp.BL;
using System;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp.ViewModel
{
    public class PriceListTemplateItemViewModel
    {
        public string Guid { get; }
        public string Title { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string RemoteUrl { get; set; }
        public string GroupName { get; set; }
        public decimal Discount { get; set; }
        public int PriceListType { get; set; }
        public bool Nds { get; set; }

        public Type Type { get; private set; }

        public PriceListTemplateItemViewModel(string guid)
        {
            if(System.Guid.Parse(guid) == System.Guid.Empty)
            {
                throw new ArgumentException(guid);
            }

            Guid = guid;

            Type = Assembly
                .GetAssembly(typeof(PriceListTemplateGuidAttribute))
                .GetTypes()
                .FirstOrDefault(type => type.GetCustomAttribute<PriceListTemplateGuidAttribute>()?.Guid == guid);
        }

    }
}
