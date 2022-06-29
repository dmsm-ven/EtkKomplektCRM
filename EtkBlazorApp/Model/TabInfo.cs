using EtkBlazorApp.Components.SettingsTabs;
using Microsoft.AspNetCore.Components;
using System;

namespace EtkBlazorApp
{
    public class SettingsTabData
    {
        public string Title { get; set; }
        public MarkupString Icon { get; set; }
        public Action SaveButtonClicked { get; set; }
    }
}
