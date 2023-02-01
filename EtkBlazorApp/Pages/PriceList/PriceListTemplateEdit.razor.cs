using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.PriceList;

public partial class PriceListTemplateEdit : ComponentBase
{
    [Inject] public IPriceListTemplateStorage templateStorage { get; set; }
    [Inject] public IStockStorage stockStorage { get; set; }
    [Inject] public ISettingStorage settings { get; set; }
    [Inject] public IToastService toasts { get; set; }
    [Inject] public UserLogger logger { get; set; }
    [Inject] public NavigationManager navManager { get; set; }

    [Parameter] public string TemplateGuid { get; set; } = string.Empty;

    FtpFileSelectDialog imageSelectDialog;
    DeleteConfirmDialog deleteDialog;
    PriceListTemplateItemViewModel sourceTemplate;

    List<PriceListTemplateRemoteUriMethodEntity> remoteUriLoadMethods;
    List<string> groupNames;
    List<string> guidList;
    List<string> alreadyUsedGuids;

    StockPartnerEntity linkedStock;
    SkipManufacturerListType newSkipManufacturerListType = SkipManufacturerListType.black_list;
    ManufacturerEntity newSkipManufacturerItem;
    ManufacturerEntity newManufacturerMapRecordItem;
    ManufacturerEntity newDiscountMapRecordItem;

    string newManufacturerMapRecordWord;
    string newQuantityMapRecordWord;
    decimal newDiscountMapValue;
    int newQuantityMapRecordValue;

    bool createNew = false;
    bool expandedQuantityMap = false;
    bool expandedManufacturerMap = false;
    bool expandedDiscountMap = false;
    bool expandedSkipList = false;

    bool showEmailPatternBox
    {
        get => sourceTemplate.RemoteUrlMethodName == "EmailAttachment";
    }
    bool addNewManufacturerMapButtonDisabled
    {
        get
        {
            return string.IsNullOrWhiteSpace(newManufacturerMapRecordWord) ||
                newManufacturerMapRecordItem == null ||
                newManufacturerMapRecordItem.name.Equals(newManufacturerMapRecordWord, StringComparison.OrdinalIgnoreCase);
        }
    }
    bool skipManufacturerAddNewRecordButtonDisabled
    {
        get
        {
            return newSkipManufacturerItem == null ||
                sourceTemplate.ManufacturerSkipList.Any(i => i.manufacturer_id == newSkipManufacturerItem.manufacturer_id || i.ListType != newSkipManufacturerListType);
        }
    }
    string buttonActionName
    {
        get => string.IsNullOrWhiteSpace(TemplateGuid) ? "Создать" : "Сохранить изменения";
    }

    protected override async Task OnInitializedAsync()
    {
        if (TemplateGuid != null)
        {
            var entity = await templateStorage.GetPriceListTemplateById(TemplateGuid);

            //TODO: переделать на инициализацию через AutoMapper
            sourceTemplate = new PriceListTemplateItemViewModel(entity.id)
            {
                Title = entity.title,
                Description = entity.description,
                Discount = entity.discount,
                GroupName = entity.group_name,
                Image = entity.image,
                Nds = entity.nds,
                RemoteUrl = entity.remote_uri,
                RemoteUrlMethodId = entity.remote_uri_method_id,
                RemoteUrlMethodName = entity.remote_uri_method_name,
                EmailSearchCriteria_FileNamePattern = entity.email_criteria_file_name_pattern,
                EmailSearchCriteria_Sender = entity.email_criteria_sender,
                EmailSearchCriteria_MaxAgeInDays = entity.email_criteria_max_age_in_days,
                EmailSearchCriteria_Subject = entity.email_criteria_subject,
                Cridentials_Login = entity.credentials_login,
                Cridentials_Password = entity.credentials_password,
                LinkedStockId = entity.stock_partner_id,
                QuantityMap = entity.quantity_map.ToDictionary(i => i.text, i => i.quantity),
                ManufacturerNameMap = entity.manufacturer_name_map.ToDictionary(i => i.text, i => i.name),
                ManufacturerDiscountMap = entity.manufacturer_discount_map
                    .Select(i => new ManufacturerDiscountItemViewModel()
                    {
                        manufacturer_id = i.manufacturer_id,
                        manufacturer_name = i.name,
                        discount = i.discount
                    }).ToList()
                ,
                ManufacturerSkipList = entity.manufacturer_skip_list
                    .Select(e => new ManufacturerSkipItemViewModel()
                    {
                        ListType = Enum.Parse<SkipManufacturerListType>(e.list_type),
                        manufacturer_id = e.manufacturer_id,
                        Name = e.name
                    }).ToList()
            };
        }
        else
        {
            TemplateGuid = string.Empty;
            sourceTemplate = new PriceListTemplateItemViewModel(TemplateGuid);
            createNew = true;
        }

        linkedStock = sourceTemplate.LinkedStockId.HasValue ?
            new StockPartnerEntity() { stock_partner_id = sourceTemplate.LinkedStockId.Value } :
            null;

        remoteUriLoadMethods = await templateStorage.GetPricelistTemplateRemoteLoadMethods();
        groupNames = await templateStorage.GetPriceListTemplatGroupNames();
        alreadyUsedGuids = (await templateStorage.GetPriceListTemplates()).Select(t => t.id).ToList();

        guidList = typeof(IPriceListTemplate).Assembly.GetTypes()
                .Select(type => type.GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .OfType<PriceListTemplateGuidAttribute>().FirstOrDefault())
                .Where(a => a != null)
                .Select(a => a.Guid)
                .OrderBy(g => g == sourceTemplate.Guid ? 0 : 1)
                .ThenBy(g => alreadyUsedGuids.Contains(g) ? 1 : 0)
                .ThenBy(g => g)
                .ToList();
    }

    private void LoadMethodChanged(ChangeEventArgs e)
    {
        string id = e?.Value?.ToString();
        var method = remoteUriLoadMethods.FirstOrDefault(m => m.id.ToString().Equals(id));
        sourceTemplate.RemoteUrlMethodId = method?.id;
        sourceTemplate.RemoteUrlMethodName = method?.name;
        StateHasChanged();
    }

    private async Task ImageFileChanged(string selectedFilePath)
    {
        if (selectedFilePath != null)
        {
            sourceTemplate.Image = selectedFilePath;
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private async Task ValidSubmit()
    {
        var entity = new PriceListTemplateEntity()
        {
            id = sourceTemplate.Guid,
            description = sourceTemplate.Description,
            title = sourceTemplate.Title,
            discount = sourceTemplate.Discount,
            image = sourceTemplate.Image,
            nds = sourceTemplate.Nds,
            group_name = sourceTemplate.GroupName,
            remote_uri = sourceTemplate.RemoteUrl,
            remote_uri_method_id = sourceTemplate.RemoteUrlMethodId,
            credentials_login = sourceTemplate.Cridentials_Login,
            credentials_password = sourceTemplate.Cridentials_Password,
            email_criteria_sender = sourceTemplate.EmailSearchCriteria_Sender,
            email_criteria_subject = sourceTemplate.EmailSearchCriteria_Subject,
            email_criteria_file_name_pattern = sourceTemplate.EmailSearchCriteria_FileNamePattern,
            email_criteria_max_age_in_days = sourceTemplate.EmailSearchCriteria_MaxAgeInDays,
            stock_partner_id = linkedStock?.stock_partner_id
        };

        if (createNew)
        {
            await CreateNewTemplate(entity);
        }
        else
        {
            await SaveChanges(entity);
        }
    }

    private async Task SaveChanges(PriceListTemplateEntity entity)
    {
        try
        {
            await templateStorage.UpdatePriceList(entity);
            toasts.ShowInfo($"Шаблон обновлен: {sourceTemplate.Title}");
            await logger.Write(LogEntryGroupName.TemplateUpdate, "Шаблон обновлен", $"Обновление шаблона '{sourceTemplate.Title}' ({sourceTemplate.Guid})");
        }
        catch (Exception ex)
        {
            toasts.ShowInfo($"{sourceTemplate.Title} - Ошибка обновления: {ex.Message}");
            await logger.Write(LogEntryGroupName.TemplateUpdate, "Ошибка обновления", $"Ошибка обновления '{sourceTemplate.Title}' ({sourceTemplate.Guid}). {ex.Message}");
        }
    }

    private async Task CreateNewTemplate(PriceListTemplateEntity entity)
    {
        try
        {
            await templateStorage.CreatePriceList(entity);
            toasts.ShowInfo($"Шаблон добавлен: {sourceTemplate.Title}");
            await logger.Write(LogEntryGroupName.TemplateUpdate, "Шаблон создан", $"Добавление шаблона '{sourceTemplate.Title}' ({sourceTemplate.Guid})");
            navManager.NavigateTo("/price-list");
        }
        catch (Exception ex)
        {
            toasts.ShowInfo($"{sourceTemplate.Title} - Ошибка создания: {ex.Message}");
            await logger.Write(LogEntryGroupName.TemplateUpdate, "Ошибка создания", $"Ошибка добавления шаблона '{sourceTemplate.Title}' ({sourceTemplate.Guid}). {ex.Message}");
        }
    }

    private async Task DeleteConfirmChanged(bool dialogResult)
    {
        if (dialogResult == true)
        {
            await templateStorage.DeletePriceList(sourceTemplate.Guid);

            await logger.Write(LogEntryGroupName.TemplateUpdate, "Удаление шаблон", $"Шаблон '{sourceTemplate.Title}' ({sourceTemplate.Guid}) удален");
            toasts.ShowInfo($"Шаблон удален: {sourceTemplate.Title}");
            navManager.NavigateTo("/price-list");
        }
    }

    #region Дополнительные настройки шаблона
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

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Скидка у бренда в прайс-листе'{newDiscountMapRecordItem.name}' --> '{newDiscountMapValue}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveDiscountMapRecord(ManufacturerDiscountItemViewModel data)
    {
        await templateStorage.RemoveDiscountMapRecord(sourceTemplate.Guid, data.manufacturer_id);
        sourceTemplate.ManufacturerDiscountMap.Remove(data);
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Убрано", $"Наценка для '{data.manufacturer_name}' из шаблона {sourceTemplate.Title}");
    }

    private async Task AddManufacturerMapRecord()
    {
        await templateStorage.AddManufacturerMapRecord(sourceTemplate.Guid, newManufacturerMapRecordWord, newManufacturerMapRecordItem.manufacturer_id);

        sourceTemplate.ManufacturerNameMap[newManufacturerMapRecordWord] = newManufacturerMapRecordItem.name;
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Преобразование названия бренда '{newManufacturerMapRecordWord}' --> '{newManufacturerMapRecordItem.name}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveManufacturerMapRecord(string word)
    {
        await templateStorage.RemoveManufacturerMapRecord(sourceTemplate.Guid, word);
        sourceTemplate.ManufacturerNameMap.Remove(word);
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Убрано", $"Преобразование названия бренда '{word}' из шаблона {sourceTemplate.Title}");
    }

    private async Task AddNewQuantityMapRecord()
    {
        await templateStorage.AddQuantityMapRecord(sourceTemplate.Guid, newQuantityMapRecordWord, newQuantityMapRecordValue);
        sourceTemplate.QuantityMap[newQuantityMapRecordWord] = newQuantityMapRecordValue;
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Преобразование остатков '{newQuantityMapRecordWord}' --> '{newQuantityMapRecordValue}' для шаблона {sourceTemplate.Title}");
    }

    private async Task RemoveQuantityMapRecord(string word)
    {
        await templateStorage.RemoveQuantityMapRecord(sourceTemplate.Guid, word);
        sourceTemplate.QuantityMap.Remove(word);
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Убрано", $"Преобразование остатков '{word}' из шаблона {sourceTemplate.Title}");
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

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Добавлено", $"Исключение бренда '{newSkipManufacturerItem.name}' ({skipItem.ListTypeDescription}) для шаблона {sourceTemplate.Title}");

    }

    private async Task RemoveSkipManufacturerRecord(ManufacturerSkipItemViewModel skipInfo)
    {
        await templateStorage.RemoveSkipManufacturerRecord(sourceTemplate.Guid, skipInfo.manufacturer_id, skipInfo.ListType.ToString());
        sourceTemplate.ManufacturerSkipList.Remove(skipInfo);
        StateHasChanged();

        await logger.Write(LogEntryGroupName.TemplateUpdate, "Убрано", $"Исключение бренда '{skipInfo.Name}' ({skipInfo.ListTypeDescription}) из шаблона {sourceTemplate.Title}");
    }
    #endregion
}

