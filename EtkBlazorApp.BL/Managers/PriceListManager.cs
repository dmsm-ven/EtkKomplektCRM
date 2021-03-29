using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class PriceListManager
    {
        public List<PriceLine> PriceLines { get; }
        public List<LoadedFileData> LoadedFiles { get; }

        private readonly IPriceLineLoadCorrelator correlator;

        public PriceListManager(IPriceLineLoadCorrelator correlator)
        {
            PriceLines = new List<PriceLine>();
            LoadedFiles = new List<LoadedFileData>();
            this.correlator = correlator;
        }

        public async Task UploadTemplate(Type templateType, Stream stream)
        {
            if (templateType == null) { throw new ArgumentNullException(nameof(templateType)); }

            string fileName = Path.GetTempFileName();
            using (var fs = File.Create(fileName))
            {
                await stream.CopyToAsync(fs);
            }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, fileName);

                var data = await templateInstance.ReadPriceLines(null);
                AddNewPriceLines(data);

                var fileData = new LoadedFileData(templateInstance)
                {
                    RecordsInFile = data.Count,
                    TemplateName = templateInstance.GetType().Name
                };

                LoadedFiles.Add(fileData);
            }
            catch(Exception ex)
            {

            }
            finally
            {
                File.Delete(fileName);
            }       
        }

        public async Task<List<PriceLine>> ReadTemplateLines(Type templateType, Stream downloadFileStream, long streamLength)
        {
            if (templateType == null) { throw new ArgumentNullException(nameof(templateType)); }

            List<PriceLine> list = new List<PriceLine>();

            string fileName = Path.GetTempFileName();
            using (var fs = File.Create(fileName))
            {
                var buffer = new byte[streamLength];
                await downloadFileStream.ReadAsync(buffer);
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }

            //Скачали данные из потока и записали в временный файл
            if (!File.Exists(fileName)) { return list; }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, fileName);

                list = await templateInstance.ReadPriceLines(null);
                return list;
            }
            catch
            {
                throw;
            }
        }

        public void RemovePriceList(LoadedFileData data)
        {
            LoadedFiles.Remove(data);
            PriceLines.RemoveAll(line => line.Template == data.Template);
        }

        public void RemovePriceList(Type templateType)
        {
            PriceLines.RemoveAll(line => line.Template.GetType() == templateType);
            LoadedFiles.Remove(LoadedFiles.FirstOrDefault(lf => lf.Template.GetType() == templateType));
        }

        public List<PriceLine> PriceLinesOfTemplate(Type templateType)
        {
            return PriceLines.OrderBy(pl => pl.Template).Where(line => line.Template.GetType() == templateType).ToList();
        }

        public List<PriceLine> PriceLinesOfManufacturer(string manufacturer)
        {
            return PriceLines.OrderBy(pl => pl.Manufacturer).Where(line => line.Manufacturer.Equals(manufacturer, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void AddNewPriceLines(List<PriceLine> newLines)
        {
            if (PriceLines.Count == 0)
            {
                PriceLines.AddRange(newLines);
                return;
            }

            foreach (var line in newLines)
            {
                var linkedLine = correlator.FindCorrelation(line, PriceLines);

                if (linkedLine != null)
                {
                    if (line.Price.HasValue)
                    {
                        linkedLine.Price = line.Price;
                    }
                    if (line.Quantity.HasValue)
                    {
                        linkedLine.Quantity = line.Quantity;
                    }
                    continue;
                }

                PriceLines.Add(line);
            }
        }
    }
}
