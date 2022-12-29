using Avalonia.Controls;
using Docnet.Core.Models;
using Docnet.Core;
using PDFTransform.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using PDFTransform.Models;
using System.Collections.ObjectModel;
using OfficeOpenXml;
using System.Threading.Tasks;
using PDFTransform.utils;

namespace PDFTransform.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {

        #region Properties

        private ObservableCollection<Acta> _actas;
        public ObservableCollection<Acta> Actas {
            get => _actas;
            set => this.RaiseAndSetIfChanged(ref _actas, value);
        }

        private string? _documentName;
        public string DocumentName {
            get => _documentName!;
            set => this.RaiseAndSetIfChanged(ref _documentName, value);
        }

        private string? _url;
        public string Url
        {
            get => _url!;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }

        private decimal _progress;
        public decimal Progress
        {
            get => _progress!;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private bool _progressIsVisible;
        public bool ProgressIsVisible
        {
            get => _progressIsVisible!;
            set => this.RaiseAndSetIfChanged(ref _progressIsVisible, value);
        }

        private bool _isNotDownloading;
        public bool IsNotDownloading
        {
            get => _isNotDownloading!;
            set => this.RaiseAndSetIfChanged(ref _isNotDownloading, value);
        }

        public ReactiveCommand<Unit, Unit> LoadDocument { get; }
        public ReactiveCommand<Unit, Unit> DownloadDocument { get; }
        public ReactiveCommand<Unit, Unit> ConvertDocument { get; }

        public MainWindow main;

        #endregion

        public MainWindowViewModel()
        {
            DocumentName = "file.zip";
            main = new MainWindow();
            _actas = new ObservableCollection<Acta>();
            Url = "";

            #region Commands

            LoadDocument = ReactiveCommand.Create(OnLoadDocument);
            DownloadDocument = ReactiveCommand.Create(OnDownloadDocument);
            ConvertDocument = ReactiveCommand.Create(OnConvertDocument);

            ProgressIsVisible = false;
            IsNotDownloading = true;

            #endregion
        }

        public void OnLoadDocument()
        {
            ShowFileDialog();
        }

        public async void OnDownloadDocument()
        {
            Actas = new ObservableCollection<Acta>();

            List<Acta> actas = new List<Acta>();
            Request req = new Request();

            Progress = 0;

            ProgressIsVisible = true;
            IsNotDownloading = false;

            _ = await req.GetActas(Url, x => Progress = x, true);

            foreach (byte[] entry in req._pdfs)
            {
                Acta acta = GetResults(entry);
                if (Heuristic.CanAdd(actas, acta))
                {
                    actas.Add(acta);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(0.275));

            ProgressIsVisible = false;
            IsNotDownloading = true;

            Actas = new ObservableCollection<Acta>(actas);
        }

        private async void ShowFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            FileDialogFilter filters = new FileDialogFilter()
            {
                Name = "Archivos comprimidos",
                Extensions = { "zip" }
            };

            dialog.Filters?.Add(filters);
            dialog.AllowMultiple = false;

            string[]? files = await dialog.ShowAsync(main);

            if(files != null)
            {
                DocumentName = Path.GetFileName(files[0]);
                ReadZip(files[0]);
            }
        }

        private void ReadZip(string path)
        {
            List<Acta> actas = new List<Acta>();

            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        Acta acta = GetResults(entry);
                        if(Heuristic.CanAdd(actas, acta)) {
                            actas.Add(acta);
                        }

                    }
                }
            }

            Actas = new ObservableCollection<Acta>(actas);
        }

        private Acta GetResults(ZipArchiveEntry entry)
        {
            Acta acta = new Acta();
            string text = "";

            Stream stream = entry.Open();
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            using (var docReader = DocLib.Instance.GetDocReader(bytes, new PageDimensions()))
            {

                for (var i = 0; i < docReader.GetPageCount(); i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        text = pageReader.GetText();

                        string[] content = text.Split("\r\n");

                        if (Heuristic.IsActa(content[0]))
                        {
                            acta = Heuristic.GetData(content, acta);
                        }
                    }
                }

            }

            return acta;
        }

        private Acta GetResults(byte[] bytes)
        {
            Acta acta = new Acta();
            string text = "";

            using (var docReader = DocLib.Instance.GetDocReader(bytes, new PageDimensions()))
            {

                for (var i = 0; i < docReader.GetPageCount(); i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        text = pageReader.GetText();

                        string[] content = text.Split("\r\n");

                        if (Heuristic.IsActa(content[0]))
                        {
                            acta = Heuristic.GetData(content, acta);
                        }

                    }
                }

            }

            return acta;
        }

        public async void OnConvertDocument()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            FileDialogFilter filters = new FileDialogFilter()
            {
                Name = "Hojas de cálculo",
                Extensions = { "xlsx" }
            };

            dialog.Filters?.Add(filters);

            string file = await dialog.ShowAsync(main);

            if(file != null)
            {
                GenerateFile(file);
            }
            
        }

        public async void GenerateFile(string path)
        {
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("INDICADORES");

            int row = 1;

            worksheet.Cells[row, 1].Value = "DOCENTE";
            worksheet.Cells[row, 2].Value = "ASIGNATURA";
            worksheet.Cells[row, 3].Value = "GRUPO";
            worksheet.Cells[row, 4].Value = "ALUMNOS INSCRITOS (EN ACTA)";
            worksheet.Cells[row, 5].Value = "ASESORÍAS IMPARTIDAS DE LA ASIGNATURA";
            worksheet.Cells[row, 6].Value = "TOTAL DE APROBADOS EN CURSO NORMAL";
            worksheet.Cells[row, 7].Value = "TOTAL DE ASESORIAS EN FINALES";
            worksheet.Cells[row, 8].Value = "TOTAL DE ASESORÍAS EN REGULARIZACIÓN";
            worksheet.Cells[row, 9].Value = "TOTAL DE ASESORÍAS EN EXTRAORDINARIO";
            worksheet.Cells[row, 10].Value = "APROBADOS (EN ACTA)";
            worksheet.Cells[row, 11].Value = "REPROBADOS (EN ACTA)";
            worksheet.Cells[row, 12].Value = "BAJAS";
            worksheet.Cells[row, 13].Value = "ALUMNOS QUE ASISTIERON A CLASE PERO QUE NO ESTÁN EN LISTA (NO INSCRITOS)DOCENTE";
            worksheet.Cells[row, 14].Value = "PROMEDIO GRUPAL (EN ACTA)";
            worksheet.Cells[row, 15].Value = "OBSERVACIONES";

            Progress = 0;
            ProgressIsVisible = true;

            foreach (Acta acta in Actas)
            {
                row++;

                worksheet.Cells[row, 1].Value = acta.teacher;
                worksheet.Cells[row, 2].Value = acta.course;
                worksheet.Cells[row, 3].Value = acta.group;
                worksheet.Cells[row, 4].Value = acta.numStudents;
                worksheet.Cells[row, 10].Value = acta.approved;
                worksheet.Cells[row, 11].Value = acta.failed;
                worksheet.Cells[row, 14].Value = acta.average.ToString("0.00");
                worksheet.Cells.AutoFitColumns(0);

                await Task.Delay(TimeSpan.FromSeconds(0.025));
                Progress = ((row - 1) * 100) / Actas.Count;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            ProgressIsVisible = false;

            var fi = new FileInfo(path);
            package.SaveAs(fi);
        }
    }

    
}
