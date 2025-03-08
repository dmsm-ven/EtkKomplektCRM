using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask
{
    /// <summary>
    /// Тип задачи - Загрузка прайс-листа из его удаленного источника (email, URL, FTP или др. как он настроен)
    /// </summary>
    public class LoadRemotePriceListCronTask : CronTask
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Type templateType;
        private readonly IPriceListTemplateStorage templates;
        private readonly RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory;
        private readonly PriceListManager priceListManager;
        private readonly ProductsPriceAndStockUpdateManager updateManager;

        public LoadRemotePriceListCronTask(Type templateType,
            IPriceListTemplateStorage templates,
            RemoteTemplateFileLoaderFactory remoteTemplateLoaderFactory,
            PriceListManager priceListManager,
            ProductsPriceAndStockUpdateManager updateManager)
        {
            this.templateType = templateType;
            this.templates = templates;
            this.remoteTemplateLoaderFactory = remoteTemplateLoaderFactory;
            this.priceListManager = priceListManager;
            this.updateManager = updateManager;
        }

        public async Task Run(CronTaskEntity taskInfo, bool forced)
        {
            try
            {
                var templateGuid = templateType.GetPriceListGuidByType();
                var templateInfo = await templates.GetPriceListTemplateById(templateGuid);

                var loader = remoteTemplateLoaderFactory.GetMethod(templateInfo.remote_uri, templateInfo.remote_uri_method_name, templateGuid);
                var response = await loader.GetFile();

                if (!forced && taskInfo?.last_exec_file_size != null && response.Bytes.Length == (int)taskInfo.last_exec_file_size)
                {
                    throw new CronTaskSkipException();
                }

                using (var ms = new MemoryStream(response.Bytes))
                {
                    var lines = await priceListManager.ReadTemplateLines(templateType, ms, response.FileName);

                    await updateManager.UpdatePriceAndStock(lines);

                    taskInfo.last_exec_file_size = response.Bytes.Length;
                }
            }
            catch (CronTaskSkipException)
            {
                throw;
            }
            catch
            {
                throw;
            }
        }
    }
}
