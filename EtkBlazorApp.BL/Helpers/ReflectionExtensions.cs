using System;
using System.Linq;
using System.Reflection;

namespace EtkBlazorApp.BL
{
    public static class ReflectionExtensions
    {
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
