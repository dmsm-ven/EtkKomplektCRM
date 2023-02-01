using System;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp.BL
{
    public static class ReflectionExtensions
    {
        public static string GetPriceListGuidByType(this Type priceListTemplateType)
        {
            var id = ((PriceListTemplateGuidAttribute)priceListTemplateType
                .GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .FirstOrDefault())
                .Guid;

            return id;
        }

        public static Type GetPriceListTypeByGuid(this string guid)
        {
            var typesWithMyAttribute =
            from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
            from t in a.GetTypes()
            let attributes = t.GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), true)
            where attributes != null && attributes.Length > 0
            select new { Type = t, Attributes = attributes.Cast<PriceListTemplateGuidAttribute>() };

            return typesWithMyAttribute.FirstOrDefault(t => t.Attributes.First().Guid == guid)?.Type;
        }

        public static string GetDescriptionAttribute(this Enum currentEnum)
        {
            Type genericEnumType = currentEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(currentEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return currentEnum.ToString();
        }
    }
}
