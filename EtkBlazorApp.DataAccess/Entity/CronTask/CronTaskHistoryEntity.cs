using System;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class CronTaskHistoryEntity 
    {
        public int task_result_id { get; set; }

        public int task_id { get; set; }
        public string name { get; set; }

        public DateTime date_time { get; set; }
        public CronTaskExecResult? exec_result { get; set; }
    }
}
