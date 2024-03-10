using EtkBlazorApp.BL.CronTask;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess.Entity;
using NLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask
{
    /// <summary>
    /// Тип задачи - Загрузка прайс-листа из его удаленного источника (email, URL, FTP или др. как он настроен)
    /// </summary>
    public class LoadRemotePriceListCronTask : CronTaskBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Type templateType;

        public LoadRemotePriceListCronTask(Type templateType, CronTaskService service, int taskId) : base(service, taskId)
        {
            this.templateType = templateType;
        }

        public override async Task Run(CronTaskEntity taskInfo)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                logger.Trace("Запуск выполнения задачи {taskName}", taskInfo?.name);

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

                logger.Info("Конец выполнения задачи {taskName}. Длительность выполнения: {elapsed} сек.",
                    taskInfo?.name, sw.Elapsed.TotalSeconds.ToString("F2", new CultureInfo("en-EN")));
            }
            catch (CronTaskSkipException)
            {
                logger.Info("Задача {taskName} пропущена, т.к. данный файл уже был загружен прежде", taskInfo.name);
            }
            catch (Exception ex)
            {
                logger.Warn("Ошибка выполнения задачи {taskName}. Выполнение длилось: {elapsed}. Message: {msg}",
                    taskInfo?.name, sw.Elapsed.TotalSeconds.ToString("F2", new CultureInfo("en-EN")), ex.Message);
                throw;
            }
        }
    }
}
