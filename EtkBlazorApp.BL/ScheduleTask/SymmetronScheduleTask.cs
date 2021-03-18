using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class SymmetronScheduleTask : ScheduleTaskBase
    {
        public SymmetronScheduleTask() : base(CronTask.Symmetron) { }

        protected override async Task Run()
        {
            await Task.Delay(TimeSpan.FromSeconds(45));

            //var imapServer = await Manager.settings.GetValue("task_symmetron_imap_server");
            //var imapPort = await Manager.settings.GetValue("task_symmetron_imap_port");
            //if (string.IsNullOrWhiteSpace(imapPort))
            //{
            //    imapPort = "143";
            //}
            //var email = await Manager.settings.GetValue("task_symmetron_login");
            //var password = await Manager.settings.GetValue("task_symmetron_password");

            //var tempFile = await MyImapClient.DownloadLastSymmetronPriceListFromMail(imapServer, imapPort, email, password);

            //await Manager.priceListManager.LoadPriceList(new SymmetronPriceListTemplate() { FileName = tempFile });

            //await Manager.updateManager.UpdatePriceAndStock(Manager.priceListManager.PriceLines, clearStockBeforeUpdate: false);

            //if (File.Exists(tempFile))
            //{
            //    File.Delete(tempFile);
            //}
        }
    }
}
