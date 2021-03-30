using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class SymmetronCronTask : CronTaskBase
    {
        public SymmetronCronTask() : base(CronTaskPrefix.Symmetron) { }

        protected override async Task Run()
        {
            var imapServer = await Manager.settings.GetValue("task_symmetron_imap_server");
            var imapPort = await Manager.settings.GetValue("task_symmetron_imap_port");
            if (string.IsNullOrWhiteSpace(imapPort))
            {
                imapPort = "143";
            }
            var email = await Manager.settings.GetValue("task_symmetron_login");
            var password = await Manager.settings.GetValue("task_symmetron_password");

            var tempFile = await EmailImapClient.DownloadLastSymmetronPriceListFromMail(imapServer, imapPort, email, password);

            var templateType = typeof(PristPriceListTemplate);

            using (var fs = new FileStream(tempFile, FileMode.Open))
            {
                var lines = await Manager.priceListManager.ReadTemplateLines(templateType, fs);
                await Manager.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }   
        }
    }
}
