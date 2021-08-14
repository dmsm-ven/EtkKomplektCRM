using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class CronTaskViewModel
    {
        [Required(ErrorMessage = "Необходимо указать тип задачи")]
        public string TypeName { get; set; }
        
        [StringLength(64, ErrorMessage = "Необходимо ввести заголовок задачи", MinimumLength = 2)]
        [Required]
        public string Title { get; set; }
     
        public string PriceListGuid { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59", ErrorMessage = "Время должно быть в диапазоне 00:00:00 - 23:59:59")]
        public TimeSpan ExecTime { get; set; }

        public int Id { get; set; }

        public CronTaskType TypeId { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public DateTime? LastExec { get; set; }
        public CronTaskExecResult? LastExecResult { get; set; }

        private DateTime executionDateTime = new DateTime();
        public DateTime ExecutionDateTime
        {
            get => executionDateTime;
            set
            {
                ExecTime = value.TimeOfDay;
                executionDateTime = value;               
            }
        }

        public TimeSpan NextExecutionLeft => ExecTime < DateTime.Now.TimeOfDay ?
                         (DateTime.Now.AddDays(1).Date.AddTicks(ExecTime.Ticks) - DateTime.Now) :
                         (ExecTime - DateTime.Now.TimeOfDay);

        public int NextExecutionPercentLeft => (int)Math.Round(100 - (100d * (NextExecutionLeft.TotalSeconds / TimeSpan.FromHours(24).TotalSeconds)));
    }
}
