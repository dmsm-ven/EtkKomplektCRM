using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Product;

public partial class UpdateProducts : ComponentBase, IDisposable
{
    [Inject] public IToastService toastService { get; set; }
    [Inject] public ISettingStorageReader setting { get; set; }
    [Inject] public IManufacturerStorage manufacturers { get; set; }
    [Inject] public PriceListManager priceListManager { get; set; }
    [Inject] public ProductsPriceAndStockUpdateManager databaseManager { get; set; }
    [Inject] public UserLogger logger { get; set; }

    bool inProgress = false;

    List<string> websiteManufacturers;
    List<string> updateProgressSteps = new List<string>();

    string selectedTabName = "tab-1";

    protected override void OnInitialized()
    {
        priceListManager.OnPriceListLoaded += OnPriceListLoad;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            websiteManufacturers = (await manufacturers.GetManufacturers()).Select(m => m.name).ToList();
        }
    }

    private async void OnPriceListLoad()
    {
        await InvokeAsync(() => StateHasChanged());
    }

    private async Task UpdateProductsPriceAndStock()
    {
        try
        {
            updateProgressSteps.Clear();
            inProgress = true;

            StateHasChanged();

            await Task.Delay(TimeSpan.FromSeconds(1));

            var progress = new Progress<string>((msg) =>
            {
                updateProgressSteps.Add($"[{DateTime.Now.ToString()}] " + msg);
                StateHasChanged();
            });

            await databaseManager.UpdatePriceAndStock(priceListManager.PriceLines, progress);

            await logger.Write(LogEntryGroupName.PriceUpdate, "Выполнено", "Обновление цен выполнено");
            toastService.ShowSuccess($"Информация - Остатки и цены на сайте etk-komplekt.ru обновлены");
        }
        catch (Exception ex)
        {
            toastService.ShowError($"Ошибка обновления .\r\n" + ex.Message + " | " + ex.StackTrace);
            await logger.Write(LogEntryGroupName.PriceUpdate, "Ошибка", $"Ошибка обновления цен: {ex.Message} | {ex.StackTrace}");
        }
        finally
        {
            inProgress = false;
        }
    }

    public void Dispose()
    {
        priceListManager.OnPriceListLoaded -= OnPriceListLoad;
    }

}

