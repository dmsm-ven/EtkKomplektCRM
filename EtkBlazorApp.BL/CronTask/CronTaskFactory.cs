using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public class CronTaskFactory
    {
        private readonly CronTaskService service;

        public CronTaskFactory(CronTaskService service)
        {
            this.service = service;
        }

        public CronTaskBase Create(string taskTypeName, string parameter)
        {
            Type linkedPriceListType = null;
            int taskId = 0;

            switch (taskTypeName)
            {
                case "Одиночный прайс-лист": 
                    return new CronTaskUsingRemotePriceList(linkedPriceListType, service, taskId);
            }

            throw new ArgumentException(taskTypeName + " не реализован");
        }
    }
}
