using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.PriceList;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using EtkBlazorApp.Model.PriceListTemplate;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.PriceList;

//TODO: разбить класс, получилось слишком много данных, дополнить маппер и использовать проброс через него
public partial class TemplateEditPage : ComponentBase
{
    [Inject] public IPriceListTemplateStorage templateStorage { get; set; }
    [Inject] public IStockStorage stockStorage { get; set; }
    [Inject] public ISettingStorageReader settings { get; set; }
    [Inject] public IToastService toasts { get; set; }
    [Inject] public IMapper mapper { get; set; }
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

    bool createNew = false;

    bool showEmailPatternBox
    {
        get => sourceTemplate?.RemoteUrlMethodName == "EmailAttachment";
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
            sourceTemplate = mapper.Map<PriceListTemplateItemViewModel>(entity);
        }
        else
        {
            TemplateGuid = string.Empty;
            sourceTemplate = new PriceListTemplateItemViewModel() { Guid = TemplateGuid };
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
}

