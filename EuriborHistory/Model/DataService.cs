//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataService.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EuriborHistory.Model
{
    /// <summary>
    /// TODO: Use System.IO.Abstractions
    /// </summary>
    public class DataService : IDataService
    {
        private string _downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() +
            "\\Euribor History";
        private const string _urlBase = @"https://www.emmi-benchmarks.eu/assets/modules/rateisblue/file_processing/publication/processed/hist_EURIBOR_";
        private const string _filename2019 = "2019.csv";
        private const string _filename2018 = "2018.csv";
        private const string _filename2017 = "2017.csv";
        private const string _filename2016 = "2016.csv";
        private const string _filename2015 = "2015.csv";

        public void GetData(Action<List<DataItem>, Exception> callback) => callback(_data, null);

        private List<DataItem> _data = new List<DataItem>();

        public async Task LoadDataAsync()
        {
            _data.Clear();

            if (!Directory.Exists(_downloadPath))
            {
                Directory.CreateDirectory(_downloadPath);
            }

            await DownloadFile(_filename2019);
            await DownloadFile(_filename2018);
            await DownloadFile(_filename2017);
            await DownloadFile(_filename2016);
            await DownloadFile(_filename2015);

            await ParseCsv(Path.Combine(_downloadPath, _filename2015));
            await ParseCsv(Path.Combine(_downloadPath, _filename2016));
            await ParseCsv(Path.Combine(_downloadPath, _filename2017));
            await ParseCsv(Path.Combine(_downloadPath, _filename2018));
            await ParseCsv(Path.Combine(_downloadPath, _filename2019));
        }

        private async Task DownloadFile(string filename)
        {
            if (DateTime.Now - File.GetCreationTime(Path.Combine(_downloadPath, filename)) > TimeSpan.FromDays(1))
            {
                var webClient = new WebClient();
                await webClient.DownloadFileTaskAsync(_urlBase + filename,
                                                      Path.Combine(_downloadPath, filename));
            }
        }

        private async Task ParseCsv(string file)
        {
            await Task.Run(() =>
            {
                using(TextFieldParser parser = new TextFieldParser(file))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    var dates = parser.ReadFields();

                    while(!parser.EndOfData)
                    {
                        var set = parser.ReadFields();

                        switch(ParsePeriod(set[0]))
                        {
                            case EuriborPeriod.OneWeek:
                                _data.AddRange(ParseData(dates, set, EuriborPeriod.OneWeek));
                                break;

                            case EuriborPeriod.OneMonth:
                                _data.AddRange(ParseData(dates, set, EuriborPeriod.OneMonth));
                                break;
                        }
                    }

                    //var oneWeekData = parser.ReadFields();
                    //var oneMonthData = parser.ReadFields();
                    //var threeMonthData = parser.ReadFields();
                    //var sixMonthData = parser.ReadFields();
                    //var twelveMonthData = parser.ReadFields();

                    //_data.AddRange(ParseData(dates, oneWeekData, EuriborPeriod.OneWeek));
                    //_data.AddRange(ParseData(dates, oneMonthData, EuriborPeriod.OneMonth));
                    //_data.AddRange(ParseData(dates, threeMonthData, EuriborPeriod.ThreeMonths));
                    //_data.AddRange(ParseData(dates, sixMonthData, EuriborPeriod.SixMonths));
                    //_data.AddRange(ParseData(dates, twelveMonthData, EuriborPeriod.TwelveMonths));
                }
            });
        }

        private EuriborPeriod ParsePeriod(string str)
        {
            if (str.StartsWith("1w"))
            {
                return EuriborPeriod.OneWeek;
            }
            if (str.StartsWith("1m"))
            {
                return EuriborPeriod.OneMonth;
            }

            return EuriborPeriod.NotDefined;
        }

        private IEnumerable<DataItem> ParseData(string[] dates, string[] values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            // 1st item is empty or period name so start from index 1
            for(var i = 1; i < dates.Length; i++)
            {
                var item = new DataItem
                {
                    Date = DateTime.Parse(dates[i]),
                    Period = period
                };
                if(double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    if(d == 0) d = double.NaN;
                    item.Value = d;
                }
                results.Add(item);
            }

            return results;
        }
    }
}