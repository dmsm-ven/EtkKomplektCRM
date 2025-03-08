using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask;

public class VseInstrumentiPricatUploaderCronTask : CronTask
{
    private readonly ISettingStorageReader settingsReader;
    private readonly SystemEventsLogger logger;
    private readonly ReportManager reportManager;
    private readonly EncryptHelper encryptHelper;

    public VseInstrumentiPricatUploaderCronTask(ISettingStorageReader settingsReader,
        SystemEventsLogger logger,
        ReportManager reportManager,
        EncryptHelper encryptHelper)
    {
        this.settingsReader = settingsReader;
        this.logger = logger;
        this.reportManager = reportManager;
        this.encryptHelper = encryptHelper;
    }

    public async Task Run(CronTaskEntity taskInfo, bool forced)
    {
        string filePath = null;

        try
        {
            var options = await GetReportOptions();
            filePath = await reportManager.Prikat.Create(options);
        }
        catch (Exception ex)
        {
            await logger.WriteSystemEvent(LogEntryGroupName.Prikat, "Ошибка", $"Ошибка создания выгрузки PRICAT для ВсеИнструменты по таймеру {ex.Message}");
        }

        try
        {
            await UploadFileToFtpServer(filePath);
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
        string section = "vse_instrumenti_ftp_uploader";

        string host = await settingsReader.GetValue($"{section}_host");
        string user = await settingsReader.GetValue($"{section}_user");
        string password = encryptHelper.Decrypt(await settingsReader.GetValue($"{section}_password"));

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