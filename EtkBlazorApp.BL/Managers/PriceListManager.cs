using EtkBlazorApp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
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

        public async Task UploadPriceList(Type templateType, Stream downloadFileStream, long streamLength, CancellationToken? token = null)
        {
            if (templateType == null) { return; }

            string fileName = Path.GetTempFileName();
            using (var fileStream = File.Create(fileName))
            {
                var buffer = new byte[streamLength];
                await downloadFileStream.ReadAsync(buffer);
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }

            //Скачали данные из потока и записали в временный файл
            if (!File.Exists(fileName)) { return; }
            
            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, fileName );

                var data = await templateInstance.ReadPriceLines(token);
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

        public void RemovePriceList(LoadedFileData data)
        {
            LoadedFiles.Remove(data);
            PriceLines.RemoveAll(line => line.Template == data.Template);
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
