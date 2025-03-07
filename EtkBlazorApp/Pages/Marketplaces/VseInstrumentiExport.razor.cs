using Blazored.Toast.Services;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.Marketplace;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.Product;
using EtkBlazorApp.Model;
using EtkBlazorApp.Model.Product;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Marketplaces;

public partial class VseInstrumentiExport : ComponentBase
{
    [Inject] public IPrikatTemplateStorage templateStorage { get; set; }
    [Inject] public ISettingStorageReader settingsReader { get; set; }
    [Inject] public ISettingStorageWriter settingsWriter { get; set; }
    [Inject] public IStockStorage stockStorage { get; set; }
    [Inject] public IToastService toasts { get; set; }
    [Inject] public IManufacturerStorage manufacturerStorage { get; set; }
    [Inject] public CronTaskService cronTaskService { get; set; }
    [Inject] public IJSRuntime js { get; set; }
    [Inject] public UserLogger logger { get; set; }
    [Inject] public ReportManager ReportManager { get; set; }

    private List<StockPartnerEntity> allStocks { get; set; }
    private Dictionary<int, List<StockPartnerEntity>> stocksWithProductsForManufacturer { get; set; } = new();
    private List<PrikatManufacturerDiscountViewModel> itemsSource = new();
    private List<ProductDiscountViewModel> discountedProducts = new();
    private bool inProgress = false;
    private ManufacturerEntity newManufacturer = new();
    private ProductDiscountViewModel newDiscountProduct = new();
    private bool reportButtonDisabled => itemsSource == null || inProgress;
    private PricatFormat selectedFormat = PricatFormat.Csv;

    protected override async Task OnInitializedAsync()
    {
        itemsSource = (await templateStorage.GetPrikatTemplates(includeDisabled: false))
            .Select(t => new PrikatManufacturerDiscountViewModel()
            {
                Discount = t.discount,
                Manufacturer_id = t.manufacturer_id,
                Manufacturer = t.manufacturer_name,
                CurrencyCode = t.currency_code ?? "RUB",
                TemplateId = t.template_id,
                CheckedStocks = t.checked_stocks
            })
            .ToList();

        var stocksSource = await stockStorage.GetManufacturersAvailableStocks();
        allStocks = await stockStorage.GetStocks();

        stocksWithProductsForManufacturer = stocksSource
            .ToDictionary(
                i => i.manufacturer_id,
                j => allStocks.Where(s => j.stock_ids.Split(",").Contains(s.stock_partner_id.ToString())).ToList());

        foreach (var ei in itemsSource)
        {
            if (string.IsNullOrWhiteSpace(ei.CheckedStocks))
            {
                ei.checked_stocks_list = GetStockListWithProductsForBrand(ei);
            }
            else
            {
                ei.checked_stocks_list = allStocks
                    .Where(s => ei.CheckedStocks.Split(",").Select(stockId => int.Parse(stockId)).Contains(s.stock_partner_id))
                    .Select(i => new StockPartnerEntity()
                    {
                        stock_partner_id = i.stock_partner_id,
                        name = i.name
                    })
                    .ToList();
            }
        }

        discountedProducts = (await templateStorage.GetDiscountedProducts())
            .Select(i => new ProductDiscountViewModel()
            {
                Id = i.product_id,
                Name = i.name,
                DiscountPercent = i.discount_price ?? 0
            })
            .ToList();

        selectedFormat = await settingsReader.GetValue<PricatFormat>("vse_instrumenti_selected_format");

        StateHasChanged();
    }

    private async Task GetReport()
    {
        using var cts = new CancellationTokenSource();

        await WaitUntilActiveTaskCompleted(cts.Token);

        inProgress = true;
        StateHasChanged();
        string filePath = null;

        try
        {
            var options = await GetReportOptions();
            filePath = await ReportManager.Prikat.Create(options);

            await js.InvokeAsync<object>("saveAsFile", Path.GetFileName(filePath), Convert.ToBase64String(File.ReadAllBytes(filePath)));
            await logger.Write(LogEntryGroupName.Prikat, "Создан", "Выгрузка для ВсеИнструменты создана");

            await settingsWriter.SetValue("vse_instrumenti_selected_format", options.PricatFormat.ToString());

            toasts.ShowInfo("Выгрузка создана. Примечание: в выгрузку не попадают товары с [0 ценой] [0 остатком] [без EAN13]");
        }
        catch (Exception ex)
        {
            await logger.Write(LogEntryGroupName.Prikat, "Ошибка", $"Ошибка создания выгрузки для ВсеИнструменты: {ex.Message}");
            toasts.ShowError($"Ошибка создания отчета: {ex.Message}");
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            inProgress = false;
            cts.Cancel();
        }
    }

    /// <summary>
    /// Запускаем задачу, которая запрещает выполнение других задач пока будет выполняет генерация файла - это нужно что бы не получилось, что во время генерации обновятся товары
    /// </summary>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    private async Task WaitUntilActiveTaskCompleted(CancellationToken cancelToken)
    {
        try
        {
            while (cronTaskService.TaskInProgress != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
            cronTaskService.PauseQueueUntilCancellationNotRequested(cancelToken);
        }
        catch
        {
            //ignore
        }
    }

    private async Task<VseInstrumentiReportOptions> GetReportOptions()
    {
        var gln_etk = await settingsReader.GetValue("vse_instrumenti_gln_etk");
        var gln_vi = await settingsReader.GetValue("vse_instrumenti_gln_vi");

        var options = new VseInstrumentiReportOptions()
        {
            GLN_ETK = gln_etk,
            GLN_VI = gln_vi,
            PricatFormat = selectedFormat
        };

        return options;
    }

    private async Task DeleteClick(PrikatManufacturerDiscountViewModel item)
    {
        await templateStorage.DisablePrikatTemplate(item.TemplateId);
        itemsSource.Remove(item);
        StateHasChanged();
    }

    private async Task AddNewManufacturer()
    {
        await templateStorage.AddNewOrRestorePrikatTemplate(newManufacturer.manufacturer_id);
        StateHasChanged();
        newManufacturer = new ManufacturerEntity();
    }

    private async Task DiscountChanged(PrikatManufacturerDiscountViewModel vmItem)
    {
        var dbItem = new PrikatReportTemplateEntity()
        {
            manufacturer_id = vmItem.Manufacturer_id,
            discount = vmItem.Discount,
            currency_code = vmItem.CurrencyCode ?? "RUB",
            template_id = vmItem.TemplateId,
            checked_stocks = vmItem.CheckedStocks,
            enabled = true
        };

        await templateStorage.SavePrikatTemplate(dbItem);

        vmItem.TemplateId = dbItem.template_id;

        StateHasChanged();
    }

    private List<StockPartnerEntity> GetStockListWithProductsForBrand(PrikatManufacturerDiscountViewModel item)
    {
        if (stocksWithProductsForManufacturer.TryGetValue(item.Manufacturer_id, out var list))
        {
            return list;
        }
        return new List<StockPartnerEntity>();
    }

    // PRODUCT DISCOUNT

    private void SelectedProductChanged(ProductEntity product)
    {
        if (product != null)
        {
            newDiscountProduct.Id = product.product_id;
            newDiscountProduct.Name = product.name;
            StateHasChanged();
        }
    }

    private async Task AddDiscountItem()
    {
        if (newDiscountProduct == null || newDiscountProduct.Id == 0)
        {
            newDiscountProduct = new ProductDiscountViewModel();
            toasts.ShowInfo("Ошибка добавления товара. ID не найден");
            StateHasChanged();
            return;
        }

        await templateStorage.AddOrUpdateSingleProductDiscount(newDiscountProduct.Id, newDiscountProduct.DiscountPercent);
        toasts.ShowSuccess($"{newDiscountProduct.Name}. Скидка добавлена");

        var existedItem = discountedProducts.FirstOrDefault(di => di.Id == newDiscountProduct.Id);
        if (existedItem != null)
        {
            existedItem.DiscountPercent = newDiscountProduct.DiscountPercent;
            await logger.Write(LogEntryGroupName.Prikat, "Обновлена", $"Обновлена скидка {newDiscountProduct.DiscountPercent}% для товара '{newDiscountProduct.Name}'");
        }
        else
        {
            discountedProducts.Insert(0, newDiscountProduct);
            await logger.Write(LogEntryGroupName.Prikat, "Добавление", $"Скидка для товара '{newDiscountProduct.Name}' ({newDiscountProduct.DiscountPercent}%)");
        }

        newDiscountProduct = new ProductDiscountViewModel();
        newDiscountProduct.PropertyChanged += (o, e) => InvokeAsync(() => StateHasChanged());

        StateHasChanged();
    }

    private async Task RemoveDiscountItem(ProductDiscountViewModel product)
    {
        discountedProducts.Remove(product);
        await templateStorage.RemoveSingleProductDiscount(product.Id);
        await logger.Write(LogEntryGroupName.Prikat, "Удаление скидки", $"Скидка для товара '{product.Name}' удалена");
        StateHasChanged();
    }
}

