using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using EtkBlazorApp.Model.PriceListTemplate;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Components.Controls;

//TODO: Нужно разбить настройки на отдельные файлы
public partial class PriceListTemplateEditAdditionalSettngs
{
    [Parameter] public PriceListTemplateItemViewModel sourceTemplate { get; set; }
    [Inject] public IPriceListTemplateAdditionalTabsStorage templateStorage { get; set; }
    [Inject] public UserLogger logger { get; set; }
    [Inject] public IToastService toasts { get; set; }

    private SkipManufacturerListType newSkipManufacturerListType = SkipManufacturerListType.black_list;
    private ManufacturerEntity newSkipManufacturerItem;

    private string newManufacturerMapRecordWord;
    private ManufacturerEntity newManufacturerMapRecordItem;

    private ManufacturerEntity newDiscountMapRecordItem;
    private decimal newDiscountMapValue;

    private string newModelMapKey;
    private string newModelMapValue;

    private string newQuantityMapRecordWord;
    private int newQuantityMapRecordValue;

    private bool addNewManufacturerMapButtonDisabled
    {
        get
        {
            return string.IsNullOrWhiteSpace(newManufacturerMapRecordWord) ||
                newManufacturerMapRecordItem == null || newManufacturerMapRecordItem.name.Equals(newManufacturerMapRecordWord, StringComparison.OrdinalIgnoreCase);
        }
    }

    private bool skipManufacturerAddNewRecordButtonDisabled
    {
        get
        {
            return newSkipManufacturerItem == null ||
                sourceTemplate.ManufacturerSkipList.Any(i => i.manufacturer_id == newSkipManufacturerItem.manufacturer_id || i.ListType != newSkipManufacturerListType);
        }
    }

    private bool addNewModelMapRecordDisabled
    {
        get
        {
            return string.IsNullOrWhiteSpace(newModelMapKey) || string.IsNullOrWhiteSpace(newModelMapValue);
        }
    }

    private async Task AddDiscountMapRecord()
    {
        await templateStorage.AddDiscountMapRecord(sourceTemplate.Guid, newDiscountMapRecordItem.manufacturer_id, newDiscountMapValue);

        sourceTemplate.ManufacturerDiscountMap.Add(new ManufacturerDiscountItemViewModel()
        {
            manufacturer_id = newDiscountMapRecordItem.manufacturer_id,
            manufacturer_name = newDiscountMapRecordItem.name,
            discount = newDiscountMapValue
        });
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Скидка у бренда в прайс-листе'{newDiscountMapRecordItem.name}' --> '{newDiscountMapValue}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveDiscountMapRecord(ManufacturerDiscountItemViewModel data)
    {
        await templateStorage.RemoveDiscountMapRecord(sourceTemplate.Guid, data.manufacturer_id);
        sourceTemplate.ManufacturerDiscountMap.Remove(data);
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Наценка для '{data.manufacturer_name}' из шаблона {sourceTemplate.Title}");
    }

    private async Task AddPurchaseDiscountRecord()
    {
        await templateStorage.AddPurchaseDiscountMapRecord(sourceTemplate.Guid, newDiscountMapRecordItem.manufacturer_id, newDiscountMapValue);

        sourceTemplate.ManufacturerPurchaseDiscountMap.Add(new ManufacturerDiscountItemViewModel()
        {
            manufacturer_id = newDiscountMapRecordItem.manufacturer_id,
            manufacturer_name = newDiscountMapRecordItem.name,
            discount = newDiscountMapValue
        });
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Закупочная скидка в прайс-листе {sourceTemplate.Title} у бренда {newDiscountMapRecordItem.name} поменяна на [{newDiscountMapValue}]");
    }

    private async Task RemovePurchaseDiscountRecord(ManufacturerDiscountItemViewModel data)
    {
        await templateStorage.RemovePurchaseDiscountMapRecord(sourceTemplate.Guid, data.manufacturer_id);
        sourceTemplate.ManufacturerPurchaseDiscountMap.Remove(data);
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Закупочная скидка из прайс-листа {sourceTemplate.Title} для бренда {data.manufacturer_name}");
    }

    private async Task AddManufacturerMapRecord()
    {
        await templateStorage.AddManufacturerMapRecord(sourceTemplate.Guid, newManufacturerMapRecordWord, newManufacturerMapRecordItem.manufacturer_id);

        sourceTemplate.ManufacturerNameMap[newManufacturerMapRecordWord] = newManufacturerMapRecordItem.name;
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Преобразование названия бренда '{newManufacturerMapRecordWord}' --> '{newManufacturerMapRecordItem.name}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveManufacturerMapRecord(string word)
    {
        await templateStorage.RemoveManufacturerMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.ManufacturerNameMap.ContainsKey(word))
        {
            sourceTemplate.ManufacturerNameMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Преобразование названия бренда '{word}' из шаблона {sourceTemplate.Title}");
    }

    private async Task AddNewQuantityMapRecord()
    {
        await templateStorage.AddQuantityMapRecord(sourceTemplate.Guid, newQuantityMapRecordWord, newQuantityMapRecordValue);
        sourceTemplate.QuantityMap[newQuantityMapRecordWord] = newQuantityMapRecordValue;
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Преобразование остатков '{newQuantityMapRecordWord}' --> '{newQuantityMapRecordValue}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveQuantityMapRecord(string word)
    {
        await templateStorage.RemoveQuantityMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.QuantityMap.ContainsKey(word))
        {
            sourceTemplate.QuantityMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Преобразование остатков '{word}' из шаблона {sourceTemplate.Title}");
    }

    private async Task AddSkipManufacturerRecord()
    {
        await templateStorage.AddSkipManufacturerRecord(sourceTemplate.Guid, newSkipManufacturerItem.manufacturer_id, newSkipManufacturerListType.ToString());
        var skipItem = new ManufacturerSkipItemViewModel()
        {
            manufacturer_id = newSkipManufacturerItem.manufacturer_id,
            Name = newSkipManufacturerItem.name,
            ListType = newSkipManufacturerListType
        };
        sourceTemplate.ManufacturerSkipList.Add(skipItem);
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Исключение бренда '{newSkipManufacturerItem.name}' ({skipItem.ListTypeDescription}) для шаблона {sourceTemplate.Title}");

    }

    private async Task RemoveSkipManufacturerRecord(ManufacturerSkipItemViewModel skipInfo)
    {
        await templateStorage.RemoveSkipManufacturerRecord(sourceTemplate.Guid, skipInfo.manufacturer_id, skipInfo.ListType.ToString());
        sourceTemplate.ManufacturerSkipList.Remove(skipInfo);
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Исключение бренда '{skipInfo.Name}' ({skipInfo.ListTypeDescription}) из шаблона {sourceTemplate.Title}");
    }

    private async Task AddNewModelMapRecord()
    {
        await templateStorage.AddModelMapRecord(sourceTemplate.Guid, newModelMapKey, newModelMapValue);

        sourceTemplate.ModelMap[newModelMapKey] = newModelMapValue;
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Преобразование модели/артикула '{newModelMapKey}' --> '{newModelMapValue}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveModelMapRecord(string word)
    {
        await templateStorage.RemoveModelMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.ModelMap.ContainsKey(word))
        {
            sourceTemplate.ModelMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("Выполнено");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "Удалено", $"Преобразование модели/артикула '{word}' из шаблона {sourceTemplate.Title}");
    }
}
