using Blazored.Toast.Services;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Managers.ReportFormatters;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.Marketplace;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.Product;
using EtkBlazorApp.Model;
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

    private List<PrikatManufacturerDiscountViewModel> itemsSource;

    public bool ShowPriceExample { get; set; } = false;
    public decimal ExamplePrice { get; set; } = 1000;

    private bool inProgress = false;
    private ManufacturerEntity newManufacturer;

    private bool reportButtonDisabled => itemsSource == null || inProgress;

    public List<StockPartnerEntity> allStocks { get; private set; }

    public Dictionary<int, List<StockPartnerEntity>> stocksWithProductsForManufacturer { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        itemsSource = (await templateStorage.GetPrikatTemplates())
            .Select(t => new PrikatManufacturerDiscountViewModel()
            {
                Discount1 = t.discount1,
                Discount2 = t.discount2,
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

        newManufacturer = new ManufacturerEntity();

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

    private async Task WaitUntilActiveTaskCompleted(CancellationToken cancelToken)
    {
        while (cronTaskService.TaskInProgress != null)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        //Запускаем задачу запрещающую выполнение других задач пока выполняет генерация файла
        cronTaskService.PauseQueueUntilCancellationNotRequested(cancelToken);
    }

    private async Task<VseInstrumentiReportOptions> GetReportOptions()
    {
        var gln = await settingsReader.GetValue("vse_instrumenti_gln");

        var options = new VseInstrumentiReportOptions()
        {
            GLN = gln
        };

        return options;
    }

    private async Task DiscountChanged(PrikatManufacturerDiscountViewModel vmItem)
    {
        var dbItem = new PrikatReportTemplateEntity()
        {
            manufacturer_id = vmItem.Manufacturer_id,
            discount1 = vmItem.Discount1,
            discount2 = vmItem.Discount2,
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
        return stocksWithProductsForManufacturer[item.Manufacturer_id];
    }
}

