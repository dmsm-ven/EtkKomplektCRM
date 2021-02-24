using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class SymmetronScheduleTask : ScheduleTaskBase
    {
        public SymmetronScheduleTask() : base(ScheduleTask.Symmetron) { }

        protected override async Task Run()
        {          
            //var imapServer = await settings.GetValue("task_symmetron_imap_server");
            //var imapPort = await settings.GetValue("task_symmetron_imap_port");
            //if (string.IsNullOrWhiteSpace(imapPort))
            //{
            //    imapPort = "143";
            //}
            //var email = await settings.GetValue("task_symmetron_login");
            //var password = await settings.GetValue("task_symmetron_password");

            //var tempFile = await MyImapClient.DownloadLastSymmetronPriceListFromMail(imapServer, imapPort, email, password);

            //await priceListManager.LoadPriceList(new SymmetronPriceListTemplate() { FileName = tempFile });

            //await updateManager.UpdatePriceAndStock(priceListManager.PriceLines, clearStockBeforeUpdate: false);

            //if (File.Exists(tempFile))
            //{
            //    File.Delete(tempFile);
            //}
        }
    }
}
