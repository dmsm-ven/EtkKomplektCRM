using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Controls;
using EtkBlazorApp.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static EtkBlazorApp.BL.EtkKomplektReportGenerator;

namespace EtkBlazorApp.Pages
{
    public partial class PriceListLoadPage
    {
        const int MAX_UPLOAD_FILE_SIZE = 15_000_000; // 15 мб
        const int UPLOAD_BUFFER_SIZE = 64_000; // 64 кб

        FileLoadProgress? uploadProgress;
        CustomDataDialog exportPriceDialog;
        ManufacturersCheckList exportPriceManufacturerList;

        Dictionary<string, List<PriceListTemplateItemViewModel>> templates = null;
        Dictionary<string, List<PriceListTemplateItemViewModel>> filteredTemplates;
        PriceListTemplateItemViewModel selectedTemplate = null;
        PriceListTemplateItemViewModel editingTemplate = null;

        bool filterHasUri = false;
        bool filterFromEmail = false;
        bool etkPriceListDownloadInProgress = false;
        bool isIntermediateProgress = false;
        bool isFileUploading = false;
        bool isBusy => isIntermediateProgress || isFileUploading;
        bool selectedTemplateHasEmailSource => !string.IsNullOrWhiteSpace(selectedTemplate?.EmailSearchCriteria_Sender);
        bool selectedTemplateHasRemoteUri => (selectedTemplate?.RemoteUrlMethodName != null) && !selectedTemplateHasEmailSource;
        string searchPhrase;

        protected override async Task OnInitializedAsync()
        {          
            templates = (await templateStorage.GetPriceListTemplates())
                .Select(t => new PriceListTemplateItemViewModel(t.id)
                {
                    Description = t.description,
                    Discount = t.discount,
                    GroupName = t.group_name,
                    Image = t.image,
                    RemoteUrl = t.remote_uri,
                    RemoteUrlMethodName = t.remote_uri_method_name,
                    EmailSearchCriteria_Sender = t.email_criteria_sender,
                    Title = t.title,
                    Nds = t.nds
                })
                .GroupBy(template => template.GroupName ?? "<Без группы>")
                .OrderBy(g => g.Key == "Symmetron" ? 0 : 1)
                .ThenBy(g => g.Key)
                .ToDictionary(i => i.Key, i => i.ToList());

            filteredTemplates = templates;
            
        }

        private async Task UploadSelectedPriceListTemplateFile(InputFileChangeEventArgs e)
        {
            if (!InvalidateFileSize(e.File.Size)) { return; }

            isFileUploading = true;

            var indicator = new Progress<FileLoadProgress>(v => { uploadProgress = v; StateHasChanged(); });
            uploadProgress = FileLoadProgress.Started;

            using (var ms = await e.File.OpenReadStream(MAX_UPLOAD_FILE_SIZE).ToMemoryStreamWithProgress(UPLOAD_BUFFER_SIZE, (int)e.File.Size, indicator))
            {
                isFileUploading = false;
                StateHasChanged();

                await LoadTemplateFromStream(ms, e.File.Name);
            }

            uploadProgress = null;
        }

        private async Task LoadRemoteUriTemplate()
        {
            isIntermediateProgress = true;
            StateHasChanged();



            try
            {
                IRemoteTemplateFileLoader loader = remoteTemplateFileLoaderFactory.GetMethod(
                    selectedTemplate.RemoteUrl,
                    selectedTemplate.RemoteUrlMethodName,
                    selectedTemplate.Guid);

                var fileInfo = await loader.GetFile();
                using (var ms = new MemoryStream(fileInfo.Bytes))
                {
                    await LoadTemplateFromStream(ms, fileInfo.FileName);
                }
            }
            catch (WebException webEx) when (webEx.Status == WebExceptionStatus.ProtocolError)
            {
                toast.ShowError("Файл недоступен", selectedTemplate.Title);
            }
            catch (Exception ex)
            {
                toast.ShowError("Не удалось загрузить файл: " + selectedTemplate.RemoteUrl + ". Ошибка: " + ex.Message, selectedTemplate.Title);
            }
            finally
            {
                isIntermediateProgress = false;
            }

        }

        private async Task LoadTemplateFromStream(Stream stream, string fileName)
        {
            isIntermediateProgress = true;
            StateHasChanged();

            try
            {
                await priceListManager.UploadTemplate(selectedTemplate.Type, stream, fileName);
                int readedLines = priceListManager.LoadedFiles.Last().ReadedPriceLines.Count;

                await logger.Write(LogEntryGroupName.PriceListTemplateLoad, "Файл загружен", $"Загружен прайс-лист '{selectedTemplate.Title} ({readedLines} строк)'");
                toast.ShowSuccess($"Файл считан ({readedLines} строк)", selectedTemplate.Title);
            }
            catch (NotFoundedPriceListTemplateException)
            {
                await logger.Write(LogEntryGroupName.PriceListTemplateLoad, "Ошибка загрузки", $"Шаблон для '{selectedTemplate.Title}' не реализован'");
                toast.ShowError($"Шаблон не реализован!", selectedTemplate.Title);
            }
            catch (Exception ex)
            {
                await logger.Write(LogEntryGroupName.PriceListTemplateLoad, "Ошибка загрузки", $"Ошибка загрузки файла с шаблоном {selectedTemplate.Title}. Ошибка: {ex.Message}");
                toast.ShowError(ex.Message, selectedTemplate.Title);
            }
            finally
            {
                isIntermediateProgress = false;
                selectedTemplate = null;
                StateHasChanged();
            }

        }

        private void SelectedTemplateChanged(PriceListTemplateItemViewModel e)
        {
            editingTemplate = null;
            selectedTemplate = e;
        }

        private async Task SaveTemplateDiscount()
        {
            await templateStorage.ChangePriceListTemplateDiscount(selectedTemplate.Guid, selectedTemplate.Discount);
            editingTemplate = null;
            toast.ShowSuccess("Скидка изменена");
            await logger.Write(LogEntryGroupName.TemplateUpdate, "Обновлено", $"Скидка для шаблона {selectedTemplate.Title} ({selectedTemplate.Guid}) изменена на {selectedTemplate.Discount.ToString("G29")}");
        }

        private void ApplyFilter(KeyboardEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchPhrase) && !filterHasUri && !filterFromEmail)
            {
                filteredTemplates = templates;
            }
            else
            {
                searchPhrase = searchPhrase ?? string.Empty;

                filteredTemplates = templates
                    .Where(g => FilterPredicate(g.Value))
                    .ToDictionary(g => g.Key, g => g.Value);
            }
        }

        private bool FilterPredicate(List<PriceListTemplateItemViewModel> items)
        {
            bool isMatch = false;

            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                bool hasTitle = items.Any(t => t.Title.IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) >= 0);
                bool hasDescription = items.Any(t => t.Title.IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) >= 0);

                isMatch = hasTitle || hasDescription;
                if (isMatch)
                {
                    return true;
                }
            }
            if (filterHasUri)
            {
                isMatch = items.Any(t => !string.IsNullOrEmpty(t.RemoteUrl));
                if (isMatch)
                {
                    return true;
                }
            }
            if (filterFromEmail)
            {
                isMatch = items.Any(t => !string.IsNullOrEmpty(t.EmailSearchCriteria_Sender));
                if (isMatch)
                {
                    return true;
                }
            }

            return false;
        }

        private bool InvalidateFileSize(long fileSize)
        {
            if (fileSize >= MAX_UPLOAD_FILE_SIZE)
            {
                string maxSizeString = ((double)MAX_UPLOAD_FILE_SIZE / 1E6).ToString("F2") + " МБ";
                string currentSizeString = ((double)fileSize / 1E6).ToString("F2") + " МБ";

                toast.ShowError($"Размер файла слишком большой - Максимальный: {maxSizeString} - Текущий: {currentSizeString}", "Ошибка"); ;
                return false;
            }
            return true;
        }

        private async Task ExportEtkPriceDialogStatusChanged(bool status)
        {
            await DownloadEtkPriceList();
        }

        private async Task DownloadEtkPriceList()
        {
            
            StateHasChanged();

            var options = new EtkKomplektPriceListExportOptions()
            {
                AllowedManufacturers = exportPriceManufacturerList?.CheckedManufacturerIds
            };

            if (options.AllowedManufacturers == null || options.AllowedManufacturers.Count == 0) { return; }

            etkPriceListDownloadInProgress = true;
            await Task.Delay(TimeSpan.FromSeconds(1));
            string filePath = null;

            try
            {

                

                filePath = await reportManager.EtkPricelist.Create(options);

                await js.InvokeAsync<object>("saveAsFile", Path.GetFileName(filePath), Convert.ToBase64String(File.ReadAllBytes(filePath)));
            }
            catch(Exception ex) 
            {
                toast.ShowError(ex.Message);
            }
            finally
            {
                if (filePath != null && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                etkPriceListDownloadInProgress = false;
            }
        }
    }
}
