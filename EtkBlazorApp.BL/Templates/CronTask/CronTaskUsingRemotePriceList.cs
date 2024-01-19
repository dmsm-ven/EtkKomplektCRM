using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    /// <summary>
    /// Тип задачи - Загрузка прайс-листа из его удаленного источника (email, URL, FTP или др. как он настроен)
    /// </summary>
    public class LoadRemotePriceListCronTask : CronTaskBase
    {
        private readonly Type templateType;

        public LoadRemotePriceListCronTask(Type templateType, CronTaskService service, int taskId) : base(service, taskId)
        {
            this.templateType = templateType;
        }

        public override async Task Run(CronTaskEntity taskInfo)
        {
            try
            {
                var templateGuid = templateType.GetPriceListGuidByType();
                var templateInfo = await service.templates.GetPriceListTemplateById(templateGuid);

                var loader = service.remoteTemplateLoaderFactory.GetMethod(templateInfo.remote_uri, templateInfo.remote_uri_method_name, templateGuid);
                var response = await loader.GetFile();

                if (response.Bytes.Length == taskInfo.last_exec_file_size)
                {
                    throw new CronTaskSkipException();
                }

                using (var ms = new MemoryStream(response.Bytes))
                {
                    var lines = await service.priceListManager.ReadTemplateLines(templateType, ms, response.FileName);

                    await service.updateManager.UpdatePriceAndStock(lines);

                    taskInfo.last_exec_file_size = response.Bytes.Length;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
