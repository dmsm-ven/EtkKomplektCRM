using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.Pages
{
    public partial class PriceListLoadedLines : ComponentBase
    {
        [Parameter] public string TemplateGuid { get; set; }

        [Inject] PriceListManager manager { get; set; }
        [Inject] IManufacturerStorage manufacturerStorage { get; set; }
        [Inject] IPriceListTemplateStorage templateStorage { get; set; }
        [Inject] IProductStorage productStorage { get; set; }
        [Inject] IJSRuntime js { get; set; }

        PriceLine selectedPriceLine = null;
        List<PriceLine> source = new List<PriceLine>();
        List<PriceLine> priceLines = null;
        Dictionary<string, int> productsByBrand;

        PriceListTemplateEntity templateInformation = null;

        string filteredManufacturer;
        string singleManufacturer;
        string searchText;

        bool hasManufacturerColumn;
        bool hasModelColumn;
        bool hasNameColumn;
        bool hasPriceColumn;
        bool hasQuantityColumn;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) { return; }

            templateInformation = await templateStorage.GetPriceListTemplateById(TemplateGuid);

            var data = manager.LoadedFiles.FirstOrDefault(p => p.TemplateDescription.id == TemplateGuid)?.ReadedPriceLines;

            source.AddRange(data);
            priceLines = new List<PriceLine>(data);

            var groupByBrand = priceLines.GroupBy(l => l.Manufacturer);

            hasManufacturerColumn = groupByBrand.Count() > 1;
            hasModelColumn = priceLines.Any(pl => pl.Model != null && pl.Model != pl.Sku);
            hasNameColumn = priceLines.Any(pl => !string.IsNullOrWhiteSpace(pl.Name));
            hasQuantityColumn = priceLines.Any(pl => pl.Quantity.HasValue);
            hasPriceColumn = priceLines.Any(pl => pl.Price.HasValue);

            if (!hasManufacturerColumn)
            {
                singleManufacturer = groupByBrand.FirstOrDefault()?.Key ?? string.Empty;
            }
            else
            {
                var allowedManufacturers = (await manufacturerStorage.GetManufacturers()).Select(m => m.name).ToList();
                productsByBrand = groupByBrand
                    .Where(m => allowedManufacturers.Contains(m.Key, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(i => i.Count())
                    .ToDictionary(i => i.Key, j => j.Count());
            }

            StateHasChanged();
        }

        private async Task SearchSkuOnWebsite(string tag, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString)) { return; }

            ProductEntity bySku = await productStorage.GetProductBySku(searchString);
            ProductEntity byModel = await productStorage.GetProductByModel(searchString);
            ProductEntity findedProduct = (tag == "findBySku") ? (bySku ?? byModel) : (byModel ?? bySku);

            string searchUri = "https://etk-komplekt.ru/index.php?route=product/search&search=" + HttpUtility.HtmlEncode(searchString);
            if (findedProduct != null)
            {
                searchUri = $"https://etk-komplekt.ru/index.php?route=product/product&product_id={findedProduct.product_id}";
            }

            await js.InvokeVoidAsync("open", searchUri, "_blank");
        }

        private void RemoveSelectedPriceLine()
        {
            source.Remove(selectedPriceLine);
            priceLines.Remove(selectedPriceLine);
            manager.PriceLines.Remove(selectedPriceLine);
        }

        private void ApplyManufacturerFilter(string manufacturer)
        {
            if (manufacturer == null)
            {
                filteredManufacturer = null;
                priceLines = source;
            }
            else
            {
                filteredManufacturer = manufacturer;
                priceLines = source.Where(pl => pl.Manufacturer.Equals(manufacturer)).ToList();
            }
        }

        private void ApplySearchFilter()
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                priceLines = source
                    .Where(pl => 
                        (pl.Name != null && pl.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                        (pl.Ean != null && pl.Ean.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                        (pl.Model != null && pl.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                       (pl.Sku != null && pl.Sku.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
            }
            else if(!string.IsNullOrWhiteSpace(filteredManufacturer))
            {
                ApplyManufacturerFilter(filteredManufacturer);
            }
            else
            {
                priceLines = source;
            }
            StateHasChanged();
        }
    }
}
