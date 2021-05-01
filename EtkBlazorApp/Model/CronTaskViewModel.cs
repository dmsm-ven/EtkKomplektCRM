using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class CronTaskViewModel
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        
        [StringLength(64, ErrorMessage = "Необходимо ввести заголовок задачи", MinimumLength = 2)]
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string PriceListGuid { get; set; }        
        public bool IsEnabled { get; set; }
        public TimeSpan ExecTime { get; set; }
        public DateTime? LastExec { get; set; }
        public bool? LastExecResult { get; set; }


        private DateTime executionDateTime = new DateTime();
        public DateTime ExecutionDateTime
        {
            get => executionDateTime;
            set
            {
                executionDateTime = value;
                ExecTime = executionDateTime.TimeOfDay;
            }
        }

        public TimeSpan NextExecutionLeft => ExecTime < DateTime.Now.TimeOfDay ?
                         (DateTime.Now.AddDays(1).Date.AddTicks(ExecTime.Ticks) - DateTime.Now) :
                         (ExecTime - DateTime.Now.TimeOfDay);

        public int NextExecutionPercentLeft => (int)Math.Round(100 - (100d * (NextExecutionLeft.TotalSeconds / TimeSpan.FromHours(24).TotalSeconds)));
    }
}
