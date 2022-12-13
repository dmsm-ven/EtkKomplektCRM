using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Components
{
    public abstract partial class DiscountTabBase : ComponentBase
    {
        [Parameter] public string TabName { get; set; }
        [CascadingParameter] public string SelectedTab { get; set; }
    }
}
