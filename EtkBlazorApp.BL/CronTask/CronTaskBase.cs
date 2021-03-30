using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class CronTaskBase
    {
        public CronTaskPrefix Prefix { get; }
        public bool IsDoneToday { get; protected set; }
        protected CronTaskManager Manager { get; private set; }

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

        public void SetManager(CronTaskManager manager)
        {
            Manager = manager;
        }

        protected abstract Task Run();

        protected string GetTemplateGuid(Type priceListTemplateType)
        {
            var id = ((PriceListTemplateDescriptionAttribute)priceListTemplateType
                .GetCustomAttributes(typeof(PriceListTemplateDescriptionAttribute), false)
                .FirstOrDefault())
                .Guid;

            return id;
        }
    }
}
