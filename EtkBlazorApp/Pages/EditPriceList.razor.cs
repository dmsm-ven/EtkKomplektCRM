using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages
{
    public partial class EditPriceList : ComponentBase
    {
        [Parameter] public string TemplateGuid { get; set; } = string.Empty;

        FtpFileSelectDialog imageSelectDialog;
        DeleteConfirmDialog deleteDialog;

        PriceListTemplateItemViewModel item;

        List<PriceListTemplateRemoteUriMethodEntity> remoteUriLoadMethods;
        List<string> groupNames;
        List<string> guidList;
        List<string> alreadyUsedGuids;

        bool createNew = false;
        bool expandedQuantityMap = false;
        bool expandedManufacturerMap = false;
        bool expandedSkipList = false;

        protected override async Task OnInitializedAsync()
        {
            if (TemplateGuid != null)
            {
                var entity = await templateStorage.GetPriceListTemplateById(TemplateGuid);

                item = new PriceListTemplateItemViewModel(entity.id)
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
                    Cridentials_Password = entity.credentials_password
                };
            }
            else
            {
                TemplateGuid = string.Empty;
                item = new PriceListTemplateItemViewModel(TemplateGuid);
                createNew = true;
            }

            remoteUriLoadMethods = await templateStorage.GetPricelistTemplateRemoteLoadMethods();
            groupNames = await templateStorage.GetPriceListTemplatGroupNames();
            alreadyUsedGuids = (await templateStorage.GetPriceListTemplates()).Select(t => t.id).ToList();

            guidList = typeof(IPriceListTemplate).Assembly.GetTypes()
                .Select(type => type.GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .OfType<PriceListTemplateGuidAttribute>().FirstOrDefault())
                .Where(a => a != null)
                .Select(a => a.Guid)
                .OrderBy(g => g == item.Guid ? 0 : 1)
                .ThenBy(g => alreadyUsedGuids.Contains(g) ? 1 : 0)
                .ThenBy(g => g)
                .ToList();
        }

        string buttonActionName => string.IsNullOrWhiteSpace(TemplateGuid) ? "Создать" : "Сохранить изменения";
        bool showEmailPatternBox => item.RemoteUrlMethodName == "EmailAttachment";

        private void LoadMethodChanged(ChangeEventArgs e)
        {
            string id = e?.Value?.ToString();
            var method = remoteUriLoadMethods.FirstOrDefault(m => m.id.ToString().Equals(id));
            item.RemoteUrlMethodId = method?.id;
            item.RemoteUrlMethodName = method?.name;
            StateHasChanged();
        }

        private async Task ImageFileChanged(string selectedFilePath)
        {
            if (selectedFilePath != null)
            {
                item.Image = selectedFilePath;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task ValidSubmit()
        {
            var entity = new PriceListTemplateEntity()
            {
                id = item.Guid,
                description = item.Description,
                title = item.Title,
                discount = item.Discount,
                image = item.Image,
                nds = item.Nds,
                group_name = item.GroupName,
                remote_uri = item.RemoteUrl,
                remote_uri_method_id = item.RemoteUrlMethodId,
                credentials_login = item.Cridentials_Login,
                credentials_password = item.Cridentials_Password,
                email_criteria_sender = item.EmailSearchCriteria_Sender,
                email_criteria_subject = item.EmailSearchCriteria_Subject,
                email_criteria_file_name_pattern = item.EmailSearchCriteria_FileNamePattern,
                email_criteria_max_age_in_days = item.EmailSearchCriteria_MaxAgeInDays
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
                toasts.ShowInfo("Шаблон обновлен", item.Title);
                await logger.Write(LogEntryGroupName.TemplateUpdate, "Шаблон обновлен", $"Обновление шаблона '{item.Title}' ({item.Guid})");
            }
            catch (Exception ex)
            {
                toasts.ShowInfo("Ошибка обновления" + ex.Message, item.Title);
                await logger.Write(LogEntryGroupName.TemplateUpdate, "Ошибка обновления", $"Ошибка обновления '{item.Title}' ({item.Guid}). {ex.Message}");
            }
        }

        private async Task CreateNewTemplate(PriceListTemplateEntity entity)
        {
            try
            {
                await templateStorage.CreatePriceList(entity);
                toasts.ShowInfo("Шаблон добавлен", item.Title);
                await logger.Write(LogEntryGroupName.TemplateUpdate, "Шаблон создан", $"Добавление шаблона '{item.Title}' ({item.Guid})");
                navManager.NavigateTo("/load-price-list");
            }
            catch (Exception ex)
            {
                toasts.ShowInfo("Ошибка создания" + ex.Message, item.Title);
                await logger.Write(LogEntryGroupName.TemplateUpdate, "Ошибка создания", $"Ошибка добавления шаблона '{item.Title}' ({item.Guid}). {ex.Message}");
            }
        }

        private async Task DeleteConfirmChanged(bool dialogResult)
        {
            if (dialogResult == true)
            {
                await templateStorage.DeletePriceList(item.Guid);

                await logger.Write(LogEntryGroupName.TemplateUpdate, "Удаление шаблон", $"Шаблон '{item.Title}' ({item.Guid}) удален");
                toasts.ShowInfo(item.Title, "Шаблон удален");
                navManager.NavigateTo("/load-price-list");
            }
        }

    }
}
