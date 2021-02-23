using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EtkBlazorApp
{
    public static class StringHelper
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

        public static string ToStringValue(this bool value)
        {
            return value ? "1" : "0";
        }

        /// <summary>
        /// Если входящая строка равна "1" то возращает false, иначе false
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ToBooleanNumber(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str == "1";
            }
            return false;
        }
    }
}
