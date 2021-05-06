namespace EtkBlazorApp.BL
{
    public class ImapEmailSearchCriteria
    {
        public string Subject { get; set; }
        public string Sender { get; set; }
        public string FileNamePattern { get; set; }
        public int MaxOldInDays { get; set; }
    }
}
