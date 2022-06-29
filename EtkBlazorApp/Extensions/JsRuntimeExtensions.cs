using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp
{
    public static class JsRuntimeExtensions
    {
        public static async Task OpenUrlInNewTab(this IJSRuntime js, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                if (!url.StartsWith("http"))
                {
                    url = "https://" + url;
                }
                await js.InvokeAsync<object>("open", new object[] { url, "_blank" });
            }
            catch (TaskCanceledException)
            {

            }
        }

        public static async Task OpenAddressAsYandexMapLocation(this IJSRuntime js, string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                string yandexMapUri = $"https://yandex.ru/maps/?text={address}";
                await OpenUrlInNewTab(js, yandexMapUri);
            }
        }

        
    }
}
