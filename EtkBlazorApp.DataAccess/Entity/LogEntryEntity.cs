using System;
using System.ComponentModel;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class LogEntryEntity
    {
        public int id { get; set; }
        public string group_name { get; set; }
        public string user { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public DateTime date_time { get; set; }

    }
}
