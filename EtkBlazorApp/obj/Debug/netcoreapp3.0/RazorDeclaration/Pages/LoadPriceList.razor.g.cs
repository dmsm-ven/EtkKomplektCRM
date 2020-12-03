#pragma checksum "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\Pages\LoadPriceList.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0d3596727c1ae24f20e46c101fcd2ce44df7998f"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace EtkBlazorApp.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using EtkBlazorApp;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using EtkBlazorApp.Shared;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\_Imports.razor"
using BlazorInputFile;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\Pages\LoadPriceList.razor"
using EtkBlazorApp.Data;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\Pages\LoadPriceList.razor"
using EtkBlazorApp.BL;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\Pages\LoadPriceList.razor"
using System.IO;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/load-price-list")]
    public partial class LoadPriceList : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 56 "D:\Programming\CSharp\Blazor\EtkBlazorApp\EtkBlazorApp\Pages\LoadPriceList.razor"
 
    List<IPriceListTemplate> templates = null;
    IPriceListTemplate selectedTemplate = null;
    IFileListEntry selectedFile = null;
    List<LoadedFileData> loadedFiles = null;
    bool isFileLoading = false;

    protected override Task OnInitializedAsync()
    {
        loadedFiles = new List<LoadedFileData>();
        templates = new List<IPriceListTemplate>(new[] { new SymmetronPriceListTemplate() });
        selectedTemplate = templates.First();
        return Task.CompletedTask;
    }

    private async Task HandleFileSelected(IFileListEntry[] newFiles)
    {
        selectedFile = newFiles.FirstOrDefault();
        await LoadSelectedFile();
    }

    private void SelectedTemplateChanged(ChangeEventArgs e)
    {
        selectedTemplate = (IPriceListTemplate)e.Value;
    }

    private async Task LoadSelectedFile()
    {
        isFileLoading = true;

        if (selectedFile != null)
        {
            string tempFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + Path.GetExtension(selectedFile.Name);

            //Загрузили файл
            using (var fileStream = File.Create(tempFilePath))
            {
                var bytes = new byte[selectedFile.Data.Length];
                await selectedFile.Data.ReadAsync(bytes, 0, bytes.Length);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            selectedTemplate.FileName = tempFilePath;
            var records = await Task.Run(() => selectedTemplate.ReadPriceLines());

            var fileData = new LoadedFileData()
            {
                FileName = selectedFile.Name,
                RecordsInFile = records.Count,
                TemplateName = selectedTemplate.GetType().Name,
                TempFilePath = tempFilePath
            };

            loadedFiles.Add(fileData);

            File.Delete(tempFilePath);
        }

        isFileLoading = false;
        selectedFile = null;
        selectedTemplate = null;
    }

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
