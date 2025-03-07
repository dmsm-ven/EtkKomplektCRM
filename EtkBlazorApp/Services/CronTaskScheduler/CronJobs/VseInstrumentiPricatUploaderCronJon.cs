using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services.CronTaskScheduler.CronJobs;

public class VseInstrumentiPricatUploaderCronJon : ICronJob
{
    private IPrikatTemplateStorage templateStorage { get; set; }
    private ISettingStorageWriter settingsWriter { get; set; }

    private readonly ISettingStorageReader settingsReader;
    private readonly SystemEventsLogger logger;
    private readonly ReportManager reportManager;

    public VseInstrumentiPricatUploaderCronJon(ISettingStorageReader settingsReader,
        SystemEventsLogger logger,
        ReportManager reportManager)
    {
        this.settingsReader = settingsReader;
        this.logger = logger;
        this.reportManager = reportManager;
    }

    public async Task Run(CancellationToken token = default)
    {
        string filePath = null;

        try
        {
            var options = await GetReportOptions();
            filePath = await reportManager.Prikat.Create(options);
            await logger.WriteSystemEvent(LogEntryGroupName.Prikat, "Успех", "Выгрузка PRICAT для ВсеИнструменты создана по таймеру");
        }
        catch (Exception ex)
        {
            await logger.WriteSystemEvent(LogEntryGroupName.Prikat, "Ошибка", $"Ошибка создания выгрузки PRICAT для ВсеИнструменты по таймеру {ex.Message}");
        }

        try
        {
            await UploadFileToFtpServer(filePath);
            await logger.WriteSystemEvent(LogEntryGroupName.Prikat, "Успех", "Выгрузка PRICAT для ВсеИнструменты отправлена на FTP сервер doclink");
        }
        catch (Exception ex)
        {
            await logger.WriteSystemEvent(LogEntryGroupName.Prikat, "Ошибка", $"Ошибка отправки PRICAT файла для ВсеИнструменты на FTP сервер doclink {ex.Message}");
        }
        finally
        {
            if (filePath != null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    private async Task UploadFileToFtpServer(string filePath)
    {
        string host = await settingsReader.GetValue("vse_instrumenti_ftp_uploader_host");
        string user = await settingsReader.GetValue("vse_instrumenti_ftp_uploader_user");
        string password = await settingsReader.GetValue("vse_instrumenti_ftp_uploader_password");

        if (new[] { host, user, password }.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Host, user или password был пуст");
        }

        using SftpClient client = new(host, 22, user, password);

        try
        {
            client.Connect();
            if (client.IsConnected)
            {
                string fileName = Path.GetFileName(filePath);
                client.UploadFile(File.OpenRead(filePath), $"/in/{fileName}");
                client.Disconnect();
            }
        }
        catch (Exception e) when (e is SshConnectionException || e is SocketException || e is ProxyException)
        {
            throw new Exception($"Error connecting to server: {e.Message}");
        }
        catch (SshAuthenticationException e)
        {
            throw new Exception($"Failed to authenticate: {e.Message}");
        }
        catch (SftpPermissionDeniedException e)
        {
            throw new Exception($"Operation denied by the server: {e.Message}");
        }
        catch (SshException e)
        {
            throw new Exception($"Sftp Error: {e.Message}");
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
            PricatFormat = PricatFormat.Xml
        };

        return options;
    }
}