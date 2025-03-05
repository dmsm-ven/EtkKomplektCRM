using Blazored.Toast.Services;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using EtkBlazorApp.Model.PriceListTemplate;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Components.Controls;

//TODO: ����� ������� ��������� �� ��������� �����
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

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"������ � ������ � �����-�����'{newDiscountMapRecordItem.name}' --> '{newDiscountMapValue}' ��� ������� {sourceTemplate.Title}");
    }

    private async Task RemoveDiscountMapRecord(ManufacturerDiscountItemViewModel data)
    {
        await templateStorage.RemoveDiscountMapRecord(sourceTemplate.Guid, data.manufacturer_id);
        sourceTemplate.ManufacturerDiscountMap.Remove(data);
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"������� ��� '{data.manufacturer_name}' �� ������� {sourceTemplate.Title}");
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

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"���������� ������ � �����-����� {sourceTemplate.Title} � ������ {newDiscountMapRecordItem.name} �������� �� [{newDiscountMapValue}]");
    }

    private async Task RemovePurchaseDiscountRecord(ManufacturerDiscountItemViewModel data)
    {
        await templateStorage.RemovePurchaseDiscountMapRecord(sourceTemplate.Guid, data.manufacturer_id);
        sourceTemplate.ManufacturerPurchaseDiscountMap.Remove(data);
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"���������� ������ �� �����-����� {sourceTemplate.Title} ��� ������ {data.manufacturer_name}");
    }

    private async Task AddManufacturerMapRecord()
    {
        await templateStorage.AddManufacturerMapRecord(sourceTemplate.Guid, newManufacturerMapRecordWord, newManufacturerMapRecordItem.manufacturer_id);

        sourceTemplate.ManufacturerNameMap[newManufacturerMapRecordWord] = newManufacturerMapRecordItem.name;
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"�������������� �������� ������ '{newManufacturerMapRecordWord}' --> '{newManufacturerMapRecordItem.name}' ��� ������� {sourceTemplate.Title}");
    }

    private async Task RemoveManufacturerMapRecord(string word)
    {
        await templateStorage.RemoveManufacturerMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.ManufacturerNameMap.ContainsKey(word))
        {
            sourceTemplate.ManufacturerNameMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"�������������� �������� ������ '{word}' �� ������� {sourceTemplate.Title}");
    }

    private async Task AddNewQuantityMapRecord()
    {
        await templateStorage.AddQuantityMapRecord(sourceTemplate.Guid, newQuantityMapRecordWord, newQuantityMapRecordValue);
        sourceTemplate.QuantityMap[newQuantityMapRecordWord] = newQuantityMapRecordValue;
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"�������������� �������� '{newQuantityMapRecordWord}' --> '{newQuantityMapRecordValue}' ��� ������� {sourceTemplate.Title}");
    }

    private async Task RemoveQuantityMapRecord(string word)
    {
        await templateStorage.RemoveQuantityMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.QuantityMap.ContainsKey(word))
        {
            sourceTemplate.QuantityMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"�������������� �������� '{word}' �� ������� {sourceTemplate.Title}");
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

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"���������� ������ '{newSkipManufacturerItem.name}' ({skipItem.ListTypeDescription}) ��� ������� {sourceTemplate.Title}");

    }

    private async Task RemoveSkipManufacturerRecord(ManufacturerSkipItemViewModel skipInfo)
    {
        await templateStorage.RemoveSkipManufacturerRecord(sourceTemplate.Guid, skipInfo.manufacturer_id, skipInfo.ListType.ToString());
        sourceTemplate.ManufacturerSkipList.Remove(skipInfo);
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"���������� ������ '{skipInfo.Name}' ({skipInfo.ListTypeDescription}) �� ������� {sourceTemplate.Title}");
    }

    private async Task AddNewModelMapRecord()
    {
        await templateStorage.AddModelMapRecord(sourceTemplate.Guid, newModelMapKey, newModelMapValue);

        sourceTemplate.ModelMap[newModelMapKey] = newModelMapValue;
        StateHasChanged();

        toasts.ShowSuccess("���������");

        await logger.Write(LogEntryGroupName.TemplateUpdate, "���������", $"�������������� ������/�������� '{newModelMapKey}' --> '{newModelMapValue}' ��� ������� {sourceTemplate.Title}");
    }

    private async Task RemoveModelMapRecord(string word)
    {
        await templateStorage.RemoveModelMapRecord(sourceTemplate.Guid, word);
        if (sourceTemplate.ModelMap.ContainsKey(word))
        {
            sourceTemplate.ModelMap.Remove(word);
        }
        StateHasChanged();

        toasts.ShowSuccess("���������");
        await logger.Write(LogEntryGroupName.TemplateUpdate, "�������", $"�������������� ������/�������� '{word}' �� ������� {sourceTemplate.Title}");
    }
}
