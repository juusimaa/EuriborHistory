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

        private bool CanExecuteDownloadCommand()
=> !_isDownloading;

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
            if (!data.Any())
            {
                return;
            }

            var lineSeries = new LineSeries { Values = new ChartValues<decimal>(), Title = "1 week" };
            Labels = data.Select(d => d.Date.ToShortDateString()).Distinct().ToArray();

            var oneWeekData = data.Where(d => d.Period == Enums.EuriborPeriod.OneWeek);
            var values = new ChartValues<decimal>(oneWeekData.Select(d => d.Value));

            MaxYValue = (double)oneWeekData.Max(d => d.Value) + 0.001;
            MinYValue = (double)oneWeekData.Min(d => d.Value) - 0.001;

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