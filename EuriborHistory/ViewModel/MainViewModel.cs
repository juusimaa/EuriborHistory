using EuriborHistory.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

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
        private List<DataItem> _data;

        public SeriesCollection SeriesCollection { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            SeriesCollection = new SeriesCollection();                
            _dataService = dataService;
            DownloadCommand = new RelayCommand(ExecuteDownloadCommand, CanExecuteDownloadCommand);
        }

        private bool CanExecuteDownloadCommand()
        {
            return !_isDownloading;
        }

        private void ExecuteDownloadCommand()
        {
            _isDownloading = true;
            _dataService.LoadDataAsync();

            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                    PopulateSeries(item);
                });

            _isDownloading = false;
        }

        private void PopulateSeries(List<DataItem> data)
        {
            var lineSeries = new LineSeries { Values = new ChartValues<decimal>()};

            var oneWeekData = data.Where(d => d.Period == Enums.EuriborPeriod.OneWeek);
            var values = new ChartValues<decimal>(oneWeekData.Select(d => d.Value));

            foreach (var item in values)
            {
                lineSeries.Values.Add(item);    
            }

            SeriesCollection.Add(lineSeries);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        public ICommand DownloadCommand { get; }
    }
}