using System;

namespace EtkBlazorApp.DataAccess.Model
{
    public class LogEntryEntity
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public DateTime DateTime { get; set; }
    }
}
