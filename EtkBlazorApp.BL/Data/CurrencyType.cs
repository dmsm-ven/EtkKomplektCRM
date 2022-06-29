using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EtkBlazorApp.BL
{
    public enum CurrencyType
    {
        [Description("₽")]
        RUB,
        [Description("€")]
        EUR,
        [Description("$")]
        USD
    }
}
