using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using PDFTransform.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PDFTransform.utils
{
    public class Request
    {
        private int processed { get; set; }
        private int entries { get; set; }
        public List<byte[]> _pdfs { get; set; }
        private DriveService _service { get; set; }

        public Request() {
            _pdfs = new List<byte[]>();
            processed = 0;
            entries = 0;

            string jsonServiceAccount = @"";

            GoogleCredential credential = GoogleCredential
                    .FromJson(jsonServiceAccount)
                    .CreateScoped(DriveService.Scope.Drive);

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public async Task<List<DriveEntry>> DriveSearchFiles(string folderId)
        {
            List<DriveEntry> entries = new List<DriveEntry>();

            var request = _service.Files.List();
            request.Q = string.Format("'{0}' in parents", folderId);

            try
            {
                var result = await Task.Run(
                                            () => request.Execute()
                                        );

                foreach (var file in result.Files)
                {
                    DriveEntry entry = new DriveEntry();
                    entry.Id = file.Id.ToString();
                    entry.Type = file.MimeType.ToString();

                    entries.Add(entry);
                };
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return entries;
        }

        public MemoryStream DriveDownloadFile(string fileId)
        {
            try
            {
                
                var request = _service.Files.Get(fileId);
                var stream = new MemoryStream();

                request.Download(stream);

                return stream;
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    Console.WriteLine("Credentials Not found");
                }
                else
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<string> GetActas(string folder, Action<decimal> setter, bool trace)
        {
            List<DriveEntry> entries = await DriveSearchFiles(folder);

            if(entries.Count > 0)
            {
                foreach (DriveEntry entry in entries)
                {
                    if (entry.Type.Equals("application/pdf"))
                    {
                        string id = entry.Id;

                        byte[] byteArray = await Task.Run(
                            () => DriveDownloadFile(id).ToArray()
                        );

                        _pdfs.Add(byteArray);
                    }
                    else if (entry.Type.Equals("application/vnd.google-apps.folder"))
                    {
                        string id = entry.Id;
                        _ = await GetActas(id, setter, false);
                    }

                    if (trace)
                    {
                        processed++;
                        setter((processed * 100) / entries.Count());
                    }

                }
            }

            return "";

        }
    }

}
