//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\ViewModel\MainViewModel.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
        private PlotModel _model;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            Model = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 0),
                Title = "Euribor",
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromAColor(0, OxyColors.Transparent),
                LegendTitle = "Years",
                LegendBorder = OxyColors.Transparent
            };
            SetupPlotModel();
            _dataService = dataService;

            LastWeekCommand = new RelayCommand(ExecuteLastWeekCommand, CanExecuteDownloadCommand);
            LastMonthCommand = new RelayCommand(ExecuteLastMonthCommand, CanExecuteDownloadCommand);
            LastYearCommand = new RelayCommand(ExecuteLastYearCommand, CanExecuteDownloadCommand);
            AllCommand = new RelayCommand(ExecuteAllCommand, CanExecuteDownloadCommand);

            LoadData();
        }

        private bool CanExecuteDownloadCommand() => true;

        private void ExecuteAllCommand()
        {
            SetupAxes(_dataService.GetMinDate(), _dataService.GetMaxDate());
        }

        private void ExecuteLastMonthCommand()
        {
            var startDate = DateTime.Now.AddMonths(-1);
            SetupAxes(startDate, _dataService.GetMaxDate());
        }

        private void ExecuteLastWeekCommand()
        {
            var startDate = DateTime.Now.AddDays(-7);            
            SetupAxes(startDate, _dataService.GetMaxDate());
        }

        private void ExecuteLastYearCommand()
        {
            var startDate = DateTime.Now.AddYears(-1);
            SetupAxes(startDate, _dataService.GetMaxDate());            
        }

        private void SetupAxes(DateTime startDate, DateTime endDate)
        {
            Model.Axes[0].Minimum = DateTimeAxis.ToDouble(startDate);
            Model.Axes[0].Maximum = DateTimeAxis.ToDouble(endDate);
            Model.Axes[1].Minimum = _dataService.GetMinValue(startDate, endDate);
            Model.Axes[1].Maximum = _dataService.GetMaxValue(startDate, endDate);
            Model.InvalidatePlot(true);
        }

        private async void LoadData()
        {
            Model.Series.Clear();

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
        }

        private void PopulateSeries(List<DataItem> data)
        {
            if (!data.Any())
            {
                return;
            }

            var oneWeekData = data.Where(d => d.Period == Enums.EuriborPeriod.OneWeek);
            var oneMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.OneMonth);
            var threeMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.ThreeMonths);
            var sixMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.SixMonths);
            var twelveMonthData = data.Where(d => d.Period == Enums.EuriborPeriod.TwelveMonths);

            var lineSeries1w = new LineSeries { Title = "1 week", StrokeThickness = 1 };
            var lineSeries1m = new LineSeries { Title = "1 month", StrokeThickness = 1 };
            var lineSeries3m = new LineSeries { Title = "3 months", StrokeThickness = 1 };
            var lineSeries6m = new LineSeries { Title = "6 months", StrokeThickness = 1 };
            var lineSeries12m = new LineSeries { Title = "12 months", StrokeThickness = 1 };

            foreach (var item in oneWeekData)
            {
                lineSeries1w.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }
            foreach (var item in oneMonthData)
            {
                lineSeries1m.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }
            foreach (var item in threeMonthData)
            {
                lineSeries3m.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }
            foreach (var item in sixMonthData)
            {
                lineSeries6m.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }
            foreach (var item in twelveMonthData)
            {
                lineSeries12m.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }

            Model.Series.Add(lineSeries1w);
            Model.Series.Add(lineSeries1m);
            Model.Series.Add(lineSeries3m);
            Model.Series.Add(lineSeries6m);
            Model.Series.Add(lineSeries12m);

            Model.Title = $"Euribor {_dataService.GetMinDate().Year} - {_dataService.GetMaxDate().Year}";

            Model.InvalidatePlot(true);
        }

        private void SetupPlotModel()
        {
            var dateAxis = new DateTimeAxis
            {
                Key = "DateAxis",
                Position = AxisPosition.Bottom,
                StringFormat = "MM/dd/yyyy",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Date"
            };
            var valueAxis = new LinearAxis
            {
                Key = "RateAxis",
                Position = AxisPosition.Left,
                Title = "Rate"
            };

            Model.Axes.Add(dateAxis);
            Model.Axes.Add(valueAxis);
        }

        public ICommand AllCommand { get; }

        public ICommand LastMonthCommand { get; }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        public ICommand LastWeekCommand { get; }

        public ICommand LastYearCommand { get; }

        public PlotModel Model
        {
            get => _model;
            set
            {
                _model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }
    }
}