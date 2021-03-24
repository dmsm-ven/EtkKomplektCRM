using System;

namespace EtkBlazorApp.BL
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PriceListTemplateDescriptionAttribute : Attribute
    {
        public string Guid { get; }

        public PriceListTemplateDescriptionAttribute(string guid)
        {
            Guid = guid;
        }
    }
}