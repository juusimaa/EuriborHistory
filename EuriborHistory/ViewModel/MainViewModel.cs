//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\ViewModel\MainViewModel.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using EuriborHistory.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
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

            Task.Run(LoadDataAsync);
        }

        private bool CanExecuteDownloadCommand() => true;

        private void CreateSeries(EuriborPeriod period, IEnumerable<DataItem> data)
        {
            var filteredData = data.Where(d => d.Period == period);
            var series = new LineSeries { Title = period.DescriptionAttr(), StrokeThickness = 1 };

            foreach(var item in filteredData)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }

            Model.Series.Add(series);
        }

        private void ExecuteAllCommand() => SetupAxes(_dataService.GetMinDate(), _dataService.GetMaxDate());

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

        private async Task LoadDataAsync() => await Task.Run(() =>
                                              {
                                                  Model.Series.Clear();

                                                  _dataService.LoadData();

                                                  _dataService.GetData((item, error) =>
                                                  {
                                                      if(error != null)
                                                      {
                                                          // Report error here
                                                          return;
                                                      }
                                                      PopulateSeries(item);
                                                  });
                                              });

        private void PopulateSeries(List<DataItem> data)
        {
            if(!data.Any())
            {
                return;
            }

            foreach(EuriborPeriod period in (EuriborPeriod[])Enum.GetValues(typeof(EuriborPeriod)))
            {
                if (period == EuriborPeriod.NotDefined)
                {
                    continue;
                }
                CreateSeries(period, data);
            }

            Model.Title = $"Euribor {_dataService.GetMinDate().Year} - {_dataService.GetMaxDate().Year}";
            Model.InvalidatePlot(true);
        }

        private void SetupAxes(DateTime startDate, DateTime endDate)
        {
            Model.Axes[0].Minimum = DateTimeAxis.ToDouble(startDate);
            Model.Axes[0].Maximum = DateTimeAxis.ToDouble(endDate);
            Model.Axes[1].Minimum = _dataService.GetMinValue(startDate, endDate);
            Model.Axes[1].Maximum = _dataService.GetMaxValue(startDate, endDate);
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

        public PlotModel Model { get; }
    }
}