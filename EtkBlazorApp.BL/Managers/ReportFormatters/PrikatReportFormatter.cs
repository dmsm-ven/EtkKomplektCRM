using System.IO;

namespace EtkBlazorApp.BL.Managers
{
    public class PrikatReportFormatter
    {
        public string Create(bool removeEmptyStock)
        {
            string filePath = Path.GetTempFileName() + ".csv";

            using (var fs = File.Create(filePath))
            {
                using(var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("Header;");
                    sw.WriteLine("Line1;");
                    sw.WriteLine("Line2;");
                    sw.WriteLine("Line3;");
                    sw.WriteLine("Line4;");
                }
            }

            return filePath;
        }
    }
}
