using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var tempFile = await MyImapClient.DownloadLastSymmetronPriceListFromMail(imapServer, imapPort, email, password);

            using (var fs = new FileStream(tempFile, FileMode.Open))
            {
                await Manager.priceListManager.UploadPriceList(typeof(SymmetronPriceListTemplate), fs, fs.Length);
            }

            await Manager.updateManager.UpdatePriceAndStock(Manager.priceListManager.PriceLines, clearStockBeforeUpdate: false);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
