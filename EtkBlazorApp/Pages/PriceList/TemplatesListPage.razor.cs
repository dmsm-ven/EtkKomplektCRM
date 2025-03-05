﻿using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.Components.Controls;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using EtkBlazorApp.Model.PriceListTemplate;
using EtkBlazorApp.Services;
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

namespace EtkBlazorApp.Pages.PriceList;

//TODO: есть ошибка, - при смене у прайс-листа типа его загрузки (email | URL и т.д.), из таблиц не удаляется email_search_critera, соответсвенно не правильн отрабаывают Enable статус кнопок
//TODO: разбить класс на мелкие фрагменты, получился слишком сложным 
public partial class TemplatesListPage
{
    private static readonly NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();

    const int MAX_UPLOAD_FILE_SIZE = 32_000_000; // 32 мб размер максимально допустимого файла
    const int UPLOAD_BUFFER_SIZE = 64_000; // Порциями по 64кб загружаем файл с отображением прогресса

    [Inject] public PriceListManager priceListManager { get; set; }
    [Inject] public UserLogger logger { get; set; }
    [Inject] public NavigationManager navManager { get; set; }
    [Inject] public RemoteTemplateFileLoaderFactory remoteTemplateFileLoaderFactory { get; set; }
    [Inject] public ReportManager reportManager { get; set; }
    [Inject] public IPriceListTemplateStorage templateStorage { get; set; }
    [Inject] public IMapper mapper { get; set; }
    [Inject] public IJSRuntime js { get; set; }
    [Inject] public IToastService toast { get; set; }

    FileLoadProgress? uploadProgress;
    CustomDataDialog exportPriceDialog;
    ManufacturersCheckList exportPriceManufacturerList;
    Dictionary<string, List<PriceListTemplateItemViewModel>> templates = null;
    Dictionary<string, List<PriceListTemplateItemViewModel>> filteredTemplates;
    PriceListTemplateItemViewModel selectedTemplate = null;
    PriceListTemplateItemViewModel editingTemplate = null;
    string searchPhrase = String.Empty;

    bool etkPriceListDownloadInProgress = false;
    bool isIntermediateProgress = false;
    bool isFileUploading = false;

    bool isBusy => isIntermediateProgress || isFileUploading;
    bool selectedTemplateHasEmailSource => !string.IsNullOrWhiteSpace(selectedTemplate?.EmailSearchCriteria_Sender);
    bool selectedTemplateHasRemoteUri => (selectedTemplate?.RemoteUrlMethodName != null) && !selectedTemplateHasEmailSource;

    protected override async Task OnInitializedAsync()
    {
        var source = await templateStorage.GetPriceListTemplates();
        templates = mapper.Map<IEnumerable<PriceListTemplateItemViewModel>>(source)
            .GroupBy(template => template.GroupName ?? "<Без группы>")
            .OrderBy(g => g.Key)
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
            toast.ShowError($"{selectedTemplate.Title} Файл недоступен");
        }
        catch (Exception ex)
        {
            toast.ShowError($"{selectedTemplate.Title} Не удалось загрузить файл: {selectedTemplate.RemoteUrl + ". Ошибка: " + ex.Message}");
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
            toast.ShowSuccess($"{selectedTemplate.Title} Файл считан ({readedLines} строк)");
            nlog.Info("Загрузка прайс-листа {title} выполнена", selectedTemplate.Title);
        }
        catch (NotFoundedPriceListTemplateException)
        {
            await logger.Write(LogEntryGroupName.PriceListTemplateLoad, "Ошибка загрузки", $"Шаблон для '{selectedTemplate.Title}' не реализован'");
            toast.ShowError($"{selectedTemplate.Title} Шаблон не реализован!");
            nlog.Warn("Ошибка загрузки прайс-листа {title} выполнена", selectedTemplate.Title);
        }
        catch (Exception ex)
        {
            await logger.Write(LogEntryGroupName.PriceListTemplateLoad, "Ошибка загрузки", $"Ошибка загрузки файла с шаблоном {selectedTemplate.Title}. Ошибка: {ex.Message}. Детали ошибки: {ex.StackTrace}");
            toast.ShowError($"{selectedTemplate.Title} ошибка: {ex.Message}");
            nlog.Warn("Ошибка загрузки файла с шаблоном {title}. StackTrace: {stackTrace}", selectedTemplate.Title, ex.StackTrace);
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
        if (string.IsNullOrWhiteSpace(searchPhrase))
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

        return false;
    }

    private bool InvalidateFileSize(long fileSize)
    {
        if (fileSize >= MAX_UPLOAD_FILE_SIZE)
        {
            string maxSizeString = ((double)MAX_UPLOAD_FILE_SIZE / 1E6).ToString("F2") + " МБ";
            string currentSizeString = ((double)fileSize / 1E6).ToString("F2") + " МБ";

            toast.ShowError($"Ошибка. Размер файла слишком большой - Максимальный: {maxSizeString} - Текущий: {currentSizeString}");
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
        catch (Exception ex)
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
