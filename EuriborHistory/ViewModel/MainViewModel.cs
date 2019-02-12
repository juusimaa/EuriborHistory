//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\ViewModel\MainViewModel.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private string[] _labels;

        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                RaisePropertyChanged(nameof(Labels));
            }
        }

        private double _maxYValue;
        public double MaxYValue
        {
            get => _maxYValue;
            set
            {
                _maxYValue = value;
                RaisePropertyChanged(nameof(MaxYValue));
            }
        }

        private double _minYValue;
        public double MinYValue
        {
            get => _minYValue;
            set
            {
                _minYValue = value;
                RaisePropertyChanged(nameof(MinYValue));
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            SeriesCollection = new SeriesCollection();
            _dataService = dataService;
            DownloadCommand = new RelayCommand(ExecuteDownloadCommand, CanExecuteDownloadCommand);
        }

        private bool CanExecuteDownloadCommand() => !_isDownloading;

        private async void ExecuteDownloadCommand()
        {
            _isDownloading = true;
            SeriesCollection.Clear();

            await _dataService.LoadDataAsync();

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
            if(!data.Any())
            {
                return;
            }

            var lineSeriesOneWeek = new LineSeries { Title = "1 week", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesOneMonth = new LineSeries { Title = "1 month", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesThreeMonth = new LineSeries { Values = new ChartValues<decimal>(), Title = "3 month" };
            var lineSeriesSixMonth = new LineSeries { Values = new ChartValues<decimal>(), Title = "6 month" };
            var lineSeriesTwelveMonth = new LineSeries { Values = new ChartValues<decimal>(), Title = "12 month" };

            Labels = data.Select(d => d.Date.ToShortDateString()).Distinct().ToArray();

            var oneWeekData = data.Where(d => d.Period == Enums.EuriborPeriod.OneWeek);
            var oneMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.OneMonth);
            var threeMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.ThreeMonths);
            var sixMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.SixMonths);
            var twelveMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.TwelveMonths);

            MaxYValue = (double)data.Max(d => d.Value) + 0.001;
            MinYValue = (double)data.Min(d => d.Value) - 0.001;

            var values1w = new ChartValues<double>();
            values1w.AddRange(oneWeekData.Select(d => d.Value));
            lineSeriesOneWeek.Values = values1w;

            var values1m = new ChartValues<double>();
            values1m.AddRange(oneMonthData.Select(d => d.Value));
            lineSeriesOneMonth.Values = values1m;

            //foreach (var item in oneMonthData.Select(d => d.Value))
            //{
            //    lineSeriesOneMonth.Values.Add(item);
            //}
            //foreach (var item in threeMonthData.Select(d => d.Value))
            //{
            //    lineSeriesThreeMonth.Values.Add(item);
            //}
            //foreach (var item in sixMonthData.Select(d => d.Value))
            //{
            //    lineSeriesSixMonth.Values.Add(item);
            //}
            //foreach (var item in twelveMonthData.Select(d => d.Value))
            //{
            //    lineSeriesTwelveMonth.Values.Add(item);
            //}

            SeriesCollection.Add(lineSeriesOneWeek);
            SeriesCollection.Add(lineSeriesOneMonth);
            SeriesCollection.Add(lineSeriesThreeMonth);
            SeriesCollection.Add(lineSeriesSixMonth);
            SeriesCollection.Add(lineSeriesTwelveMonth);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        public ICommand DownloadCommand { get; }
    }
}