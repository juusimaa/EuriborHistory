using EuriborHistory.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Windows.Input;

namespace EuriborHistory.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private bool _isDownloading;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                });

            DownloadCommand = new RelayCommand(ExecuteDownloadCommand, CanExecuteDownloadCommand);
        }

        private bool CanExecuteDownloadCommand()
        {
            return !_isDownloading;
        }

        private async void ExecuteDownloadCommand()
        {
            _isDownloading = true;
            var webClient = new WebClient();

            await webClient.DownloadFileTaskAsync(@"https://www.emmi-benchmarks.eu/assets/modules/rateisblue/file_processing/publication/processed/hist_EURIBOR_2019.csv",
                "2019.csv");

            _isDownloading = false;

            ParseCsv();
        }

        private void ParseCsv()
        {
            using (TextFieldParser parser = new TextFieldParser("2019.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        //TODO: Process field
                    }
                }
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        public ICommand DownloadCommand { get; }
    }
}