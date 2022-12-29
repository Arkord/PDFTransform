using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PDFTransform.Views
{
    public partial class MainWindow : Window
    {
        public int Progress { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
        }

        //public void onClickLoad(object sender, RoutedEventArgs args)
        //{
        //    showFileDialog();
        //}

        //public async void showFileDialog()
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    FileDialogFilter filters = new FileDialogFilter()
        //    {
        //        Name = "Archivos comprimidos",
        //        Extensions = { "zip" }
        //    };

        //    dialog.Filters?.Add(filters);
        //    dialog.AllowMultiple = false;

        //    string?[] files = await dialog.ShowAsync(this);

        //}
    }
}
