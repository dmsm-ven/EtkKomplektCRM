using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages
{
    public partial class UpdateProducts : ComponentBase, IDisposable
    {
        [Inject] public IToastService toastService { get; set; }
        [Inject] public ISettingStorage setting { get; set; }
        [Inject] public IManufacturerStorage manufacturers { get; set; }
        [Inject] public CronTaskService cronTaskService { get; set; }
        [Inject] public PriceListManager priceListManager { get; set; }
        [Inject] public UpdateManager databaseManager { get; set; }
        [Inject] public UserLogger logger { get; set; }

        bool clearStockBeforeUpdate = false;
        bool inProgress = false;

        CronTaskEntity activeTask;

        List<string> websiteManufacturers;
        List<string> updateProgressSteps = new List<string>();

        string selectedTabName = "tab-1";

        string clearStockBrandsArrayString
        {
            get
            {
                var priceLinesBrands = priceListManager.PriceLines
                    .Where(line => line.StockPartner == null && line.Quantity != null)
                    .GroupBy(p => p.Manufacturer)
                    .Select(g => g.Key)
                    .OrderBy(m => m)
                    .ToList();

                var affectedBrands = priceLinesBrands
                    .Intersect(websiteManufacturers, StringComparer.OrdinalIgnoreCase);

                return string.Join(", ", affectedBrands);
            }
        }

        protected override void OnInitialized()
        {
            activeTask = cronTaskService.TaskInProgress;
            cronTaskService.OnTaskExecutionStart += OnTaskManagerStart;
            cronTaskService.OnTaskExecutionEnd += OnTaskManagerFinish;
            priceListManager.OnPriceListLoaded += OnPriceListLoad;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                websiteManufacturers = (await manufacturers.GetManufacturers()).Select(m => m.name).ToList();
            }
        }

        private async void OnTaskManagerStart(CronTaskEntity t)
        {
            activeTask = t;
            await InvokeAsync(() => StateHasChanged());
        }

        private async void OnTaskManagerFinish(CronTaskEntity t)
        {
            activeTask = null;
            await InvokeAsync(() => StateHasChanged());
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

                await databaseManager.UpdatePriceAndStock(priceListManager.PriceLines, clearStockBeforeUpdate, progress);

                await logger.Write(LogEntryGroupName.PriceUpdate, "Выполнено", "Обновление цен выполнено");
                toastService.ShowSuccess($"Остатки и цены на сайте etk-komplekt.ru обновлены", "Информация");
            }
            catch (Exception ex)
            {
                toastService.ShowError($"При обновлении данных произошла ошибка\r\n" + ex.Message, "Ошибка обновления");
                await logger.Write(LogEntryGroupName.PriceUpdate, "Ошибка", $"Ошибка обновления цен: {ex.Message}");
            }
            finally
            {
                inProgress = false;
            }
        }

        public void Dispose()
        {
            cronTaskService.OnTaskExecutionStart -= OnTaskManagerStart;
            cronTaskService.OnTaskExecutionEnd -= OnTaskManagerFinish;
            priceListManager.OnPriceListLoaded -= OnPriceListLoad;
        }

    }
}
