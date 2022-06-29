using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Helpers
{
    public static class DictionaryExtensions
    {
        public static TV GetValueOrNull<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            if(dict == null) { return default(TV); }
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
