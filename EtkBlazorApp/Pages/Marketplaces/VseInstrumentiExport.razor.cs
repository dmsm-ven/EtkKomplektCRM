using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Controls;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Marketplaces;

public partial class VseInstrumentiExport : ComponentBase
{
    [Inject] public IPrikatTemplateStorage templateStorage { get; set; }
    [Inject] public ISettingStorage settingStorage { get; set; }
    [Inject] public IToastService toasts { get; set; }
    [Inject] public IManufacturerStorage manufacturerStorage { get; set; }
    [Inject] public IJSRuntime js { get; set; }
    [Inject] public UserLogger logger { get; set; }
    [Inject] public ReportManager ReportManager { get; set; }

    List<PrikatManufacturerDiscountViewModel> itemsSource;
    List<PrikatManufacturerDiscountViewModel> orderedSource => itemsSource.OrderByDescending(t => t.IsChecked).ToList();

    StocksCheckListBox selectedStocksCheckListBox;

    public bool ShowPriceExample { get; set; } = false;
    public decimal ExamplePrice { get; set; } = 1000;

    bool reportOptionsHasStock = false;
    bool reportOptionsHasEan = false;
    string reportOptionsGln;

    bool uncheckAllState = false;
    bool showSettingsBox = false;

    bool inProgress = false;
    bool reportButtonDisabled => itemsSource == null || itemsSource.All(m => m.IsChecked == false);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            itemsSource = (await templateStorage.GetPrikatTemplates())
                    .Select(t => new PrikatManufacturerDiscountViewModel()
                    {
                        IsChecked = t.enabled || (t.discount1 != decimal.Zero || t.discount2 != decimal.Zero),
                        Discount1 = t.discount1,
                        Discount2 = t.discount2,
                        Manufacturer_id = t.manufacturer_id,
                        Manufacturer = t.manufacturer_name,
                        CurrencyCode = t.currency_code ?? "RUB",
                        TemplateId = t.template_id
                    })
                    .ToList();

            reportOptionsHasStock = await settingStorage.GetValue<bool>("vse_instrumenti_export_options_stock");
            reportOptionsHasEan = await settingStorage.GetValue<bool>("vse_instrumenti_export_options_ean");
            reportOptionsGln = await settingStorage.GetValue("vse_instrumenti_gln");

            StateHasChanged();
        }
    }

    private async Task GetReport()
    {
        inProgress = true;
        StateHasChanged();
        string filePath = null;

        try
        {
            filePath = await ReportManager.Prikat.Create(GetSelectedManufacturerIds(), GetReportOptions());

            await js.InvokeAsync<object>("saveAsFile", Path.GetFileName(filePath), Convert.ToBase64String(File.ReadAllBytes(filePath)));

            await logger.Write(LogEntryGroupName.Prikat, "Создан", "Выгрузка для ВсеИнструменты создана");
        }
        catch (Exception ex)
        {
            await logger.Write(LogEntryGroupName.Prikat, "Ошибка", $"Ошибка создания выгрузки для ВсеИнструменты: {ex.Message}. {ex.StackTrace}");
            toasts.ShowError("Ошибка создания отчета" + ex.Message, "ВсеИнструменты");
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            inProgress = false;
        }
    }

    private VseInstrumentiReportOptions GetReportOptions()
    {
        var options = new VseInstrumentiReportOptions()
        {
            HasEan = reportOptionsHasEan,
            StockGreaterThanZero = reportOptionsHasStock,
            GLN = reportOptionsGln,
            UsePartnerStock = selectedStocksCheckListBox.CheckedStocks
        };

        return options;
    }

    private IEnumerable<int> GetSelectedManufacturerIds()
    {
        return itemsSource.Where(item => item.IsChecked).Select(item => item.Manufacturer_id);
    }

    private async Task ExportOptionsChanged()
    {
        await settingStorage.SetValue("vse_instrumenti_export_options_stock", reportOptionsHasStock);
        await settingStorage.SetValue("vse_instrumenti_export_options_ean", reportOptionsHasEan);
    }

    private void HeaderCheckAll(ChangeEventArgs e)
    {
        uncheckAllState = !uncheckAllState;

        foreach (var item in itemsSource)
        {
            item.IsChecked = uncheckAllState && new[] { item.Discount1, item.Discount2 }.Any(d => d != decimal.Zero);
        }

        StateHasChanged();
    }

    private async Task DiscountChanged(PrikatManufacturerDiscountViewModel vmItem)
    {
        var dbItem = new PrikatReportTemplateEntity()
        {
            manufacturer_id = vmItem.Manufacturer_id,
            discount1 = vmItem.Discount1,
            discount2 = vmItem.Discount2,
            enabled = vmItem.IsChecked,
            currency_code = vmItem.CurrencyCode ?? "RUB",
            template_id = vmItem.TemplateId
        };

        await templateStorage.SavePrikatTemplate(dbItem);
        vmItem.TemplateId = dbItem.template_id;

        StateHasChanged();
    }

}

