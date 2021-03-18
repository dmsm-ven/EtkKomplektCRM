using EtkBlazorApp.BL;
using System;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp.ViewModel
{
    public class PriceListTemplateItemViewModel : ViewModelBase
    {
        string guid;
        public string Guid
        {
            get => guid;
            set
            {
                if(Set(ref guid, value) && value != null) 
                { 
                    Type = Assembly
                        .GetAssembly(typeof(PriceListTemplateDescriptionAttribute))
                        .GetTypes()
                        .FirstOrDefault(type => type.GetCustomAttribute<PriceListTemplateDescriptionAttribute>()?.Guid == Guid);
                }
            }
        }
        public string Title { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public string RemoteUrl { get; set; }
        public string GroupName { get; set; }
        public decimal Discount { get; set; }

        // TODO тут надо заменить на ENUM 
        public int PriceListType { get; set; }

        public Type Type { get; private set; }

    }
}
