using System;

namespace EtkBlazorApp.BL
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PriceListTemplateGuidAttribute : Attribute
    {
        public string Guid { get; }

        public PriceListTemplateGuidAttribute(string guid)
        {
            Guid = guid;
        }
    }
}