using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.Pages.Product
{
    public partial class EditProduct
    {
        [Inject]
        public IProductStorage productStorage { get; set; }

        [Inject]
        public IProductUpdateService productUpdateService { get; set; }

        [Inject]
        public ISettingStorage settingStorage { get; set; }

        [Inject]
        public ICurrencyChecker currencyChecker { get; set; }

        [Inject]
        public IToastService toasts { get; set; }

        [Inject]
        public IMapper mapper { get; set; }

        [Inject]
        public UserLogger log { get; set; }

        ProductModel editedProduct = null;
        ProductEntity replacementProduct;
        string[] basePriceCurrencyNames = Enum.GetNames(typeof(CurrencyType));
        string[] stockStatusNames = null;
        string enteredUri = null;
        [Parameter]
        public string Keyword { get; set; }

        int currentStateCode = 0;
        bool hasChanges => currentStateCode != 0 && currentStateCode != GetCurrentStateCode();
        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                enteredUri = $"https://etk-komplekt.ru/{Keyword}";
            }
            else
            {
                enteredUri = await settingStorage.GetValue("edit-product-page-last-uri");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                stockStatusNames = (await productStorage.GetStockStatuses()).Select(e => e.name).ToArray();
                if (!string.IsNullOrWhiteSpace(enteredUri))
                {
                    await LoadProduct();
                }
            }
        }

        private async Task LoadProduct()
        {
            if (string.IsNullOrWhiteSpace(enteredUri))
            {
                return;
            }

            string keyword = new Uri(enteredUri).AbsolutePath.Trim('/', '?', '&');
            var entity = await productStorage.GetProductByKeyword(keyword);
            if (entity != null)
            {
                editedProduct = mapper.Map<ProductModel>(entity);
                if (editedProduct.ReplacementProductId.HasValue)
                {
                    replacementProduct = await productStorage.GetProductById(editedProduct.ReplacementProductId.Value);
                }

                await settingStorage.SetValue("edit-product-page-last-uri", editedProduct.Uri);
                currentStateCode = GetCurrentStateCode();
                StateHasChanged();
            }
        }

        private async Task SaveChanges()
        {
            if (editedProduct.BasePriceCurrency != CurrencyType.RUB.ToString())
            {
                decimal ratio = await currencyChecker.GetCurrencyRate(Enum.Parse<CurrencyType>(editedProduct.BasePriceCurrency));
                editedProduct.Price = Math.Round(ratio * editedProduct.BasePrice);
            }
            else
            {
                editedProduct.BasePriceCurrency = CurrencyType.RUB.ToString();
                editedProduct.Price = editedProduct.BasePrice;
            }

            editedProduct.ReplacementProductId = replacementProduct?.product_id;
            var entity = mapper.Map<ProductEntity>(editedProduct);
            await productUpdateService.UpdateDirectProduct(entity);
            await log.Write(LogEntryGroupName.ProductUpdate, "Товар обновлен", $"{HttpUtility.HtmlDecode(editedProduct.Name)} ({editedProduct.ProductIdUri})");
            currentStateCode = GetCurrentStateCode();
        }

        private int GetCurrentStateCode()
        {
            int code = 0;
            if (editedProduct != null)
            {
                code += editedProduct.Id;
                code += editedProduct.Price.GetHashCode();
                code += editedProduct.BasePrice.GetHashCode();
                code += editedProduct.Quantity.GetHashCode();
                code += (editedProduct?.StockStatus ?? string.Empty).GetHashCode();
                code += (editedProduct?.BasePriceCurrency ?? string.Empty).GetHashCode();
                code += (replacementProduct?.product_id.GetHashCode() ?? 9);
            }

            return code;
        }
    }
}