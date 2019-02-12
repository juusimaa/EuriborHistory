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
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public SeriesCollection SeriesCollection { get; set; }

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

        Func<double, string> _formatter;
        public Func<double, string> Formatter
        {
            get => _formatter;
            set
            {
                _formatter = value;
                RaisePropertyChanged(nameof(Formatter));
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {            
            var dayConfig = Mappers.Xy<DataItem>()
                .X(model => (double)model.Date.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(model => model.Value);
            SeriesCollection = new SeriesCollection(dayConfig);
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
            if (!data.Any())
            {
                return;
            }            

            var lineSeriesOneWeek = new LineSeries { Title = "1 week", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesOneMonth = new LineSeries { Title = "1 month", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesThreeMonth = new LineSeries { Title = "3 month", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesSixMonth = new LineSeries { Title = "6 month", LineSmoothness = 1, PointGeometry = null };
            var lineSeriesTwelveMonth = new LineSeries { Title = "12 month", LineSmoothness = 1, PointGeometry = null };

            var oneWeekData = data.Where(d => d.Period == Enums.EuriborPeriod.OneWeek);
            var oneMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.OneMonth);
            var threeMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.ThreeMonths);
            var sixMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.SixMonths);
            var twelveMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.TwelveMonths);

            MaxYValue = Math.Round(data.Where(d => !double.IsNaN(d.Value)).Max(v => v.Value) + 0.1, 1);
            MinYValue = Math.Round(data.Where(d => !double.IsNaN(d.Value)).Min(v => v.Value) - 0.1, 1);

            var values1w = new ChartValues<DataItem>();
            values1w.AddRange(oneWeekData);
            lineSeriesOneWeek.Values = values1w;

            var values1m = new ChartValues<DataItem>();
            values1m.AddRange(oneMonthData);
            lineSeriesOneMonth.Values = values1m;

            var values3m = new ChartValues<DataItem>();
            values3m.AddRange(threeMonthData);
            lineSeriesThreeMonth.Values = values3m;

            var values6m = new ChartValues<DataItem>();
            values6m.AddRange(sixMonthData);
            lineSeriesSixMonth.Values = values6m;

            var values12m = new ChartValues<DataItem>();
            values12m.AddRange(twelveMonthData);
            lineSeriesTwelveMonth.Values = values12m;

            SeriesCollection.Add(lineSeriesOneWeek);
            SeriesCollection.Add(lineSeriesOneMonth);
            SeriesCollection.Add(lineSeriesThreeMonth);
            SeriesCollection.Add(lineSeriesSixMonth);
            SeriesCollection.Add(lineSeriesTwelveMonth);

            Formatter = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToShortDateString();
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        public ICommand DownloadCommand { get; }
    }
}