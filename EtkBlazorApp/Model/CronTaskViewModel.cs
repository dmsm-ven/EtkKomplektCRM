using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EtkBlazorApp.Model
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

        public List<TimeSpan> AdditionalExecTime { get; set; } = new List<TimeSpan>();

        private DateTime executionDateTime = new();
        public DateTime ExecutionDateTime
        {
            get => executionDateTime;
            set
            {
                ExecTime = value.TimeOfDay;
                executionDateTime = value;
            }
        }

        public TimeSpan NextExecutionLeft
        {
            get
            {
                var curTime = DateTime.Now.TimeOfDay;
                var now = DateTime.Now;

                if (AdditionalExecTime != null && AdditionalExecTime.Count > 0)
                {
                    var closestTime = (new TimeSpan[] { ExecTime }).Concat(AdditionalExecTime.ToArray())
                        .Select(t => new
                        {
                            diff = Math.Abs(curTime.Ticks - t.Ticks),
                            time = t < curTime ?
                                     DateTime.Now.AddDays(1).Date.AddTicks(t.Ticks) - DateTime.Now :
                                     t - curTime
                        })
                        .OrderBy(t => t.time)
                        .ToArray();

                    return closestTime.First().time;
                }
                else
                {
                    return ExecTime < curTime ?
                         DateTime.Now.AddDays(1).Date.AddTicks(ExecTime.Ticks) - DateTime.Now :
                         ExecTime - curTime;
                }

            }
        }

        public bool IsEmailAttachmentTask { get; init; }
    }
}
