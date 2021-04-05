﻿using EtkBlazorApp.BL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public abstract class CronTaskBase
    {
        public CronTaskPrefix Prefix { get; }
        public bool IsDoneToday { get; protected set; }
        protected CronTaskService service { get; private set; }

        public CronTaskBase(CronTaskPrefix prefix)
        {
            Prefix = prefix;
        }

        public async Task Execute()
        {
            try
            {
                await Run();
                //Ставим флаг что задание выполнено (даже в случае ошибки, что бы не вызывать выполнение каждые раз, даже если не получилось выполнить)
                IsDoneToday = true;
            }
            catch
            {
                throw;
            }
            
        }

        public void Reset()
        {
            IsDoneToday = false;
        }

        protected abstract Task Run();

        protected string GetTemplateGuid(Type priceListTemplateType)
        {
            var id = ((PriceListTemplateGuidAttribute)priceListTemplateType
                .GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .FirstOrDefault())
                .Guid;

            return id;
        }

        internal void SetService(CronTaskService cronTaskService)
        {
            service = cronTaskService;
        }
    }
}
