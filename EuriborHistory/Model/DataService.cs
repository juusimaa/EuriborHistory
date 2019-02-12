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
using System.Linq;
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
        private readonly string[] _fileNames =
        {
            "2011.csv",
            "2012.csv",
            "2013.csv",
            "2014.csv",
            "2015.csv",
            "2016.csv",
            "2017.csv",
            "2018.csv",
            "2019.csv"
        };

        public void GetData(Action<List<DataItem>, Exception> callback) => callback(_data, null);

        private List<DataItem> _data = new List<DataItem>();

        public async Task LoadDataAsync()
        {
            _data.Clear();

            if (!Directory.Exists(_downloadPath))
            {
                Directory.CreateDirectory(_downloadPath);
            }

            foreach (var filename in _fileNames)
            {
                await DownloadFile(filename);
                await ParseCsv(Path.Combine(_downloadPath, filename));
            }
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

                            case EuriborPeriod.ThreeMonths:
                                _data.AddRange(ParseData(dates, set, EuriborPeriod.ThreeMonths));
                                break;

                            case EuriborPeriod.SixMonths:
                                _data.AddRange(ParseData(dates, set, EuriborPeriod.SixMonths));
                                break;

                            case EuriborPeriod.TwelveMonths:
                                _data.AddRange(ParseData(dates, set, EuriborPeriod.TwelveMonths));
                                break;
                        }
                    }
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
            if (str.StartsWith("3m"))
            {
                return EuriborPeriod.ThreeMonths;
            }
            if (str.StartsWith("6m"))
            {
                return EuriborPeriod.SixMonths;
            }
            if (str.StartsWith("12m"))
            {
                return EuriborPeriod.TwelveMonths;
            }

            return EuriborPeriod.NotDefined;
        }

        private IEnumerable<DataItem> ParseData(string[] dates, string[] values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            // 1st item is empty or period name so start from index 1
            for(var i = 1; i < dates.Length; i++)
            {
                var item = new DataItem { Period = period };

                if (DateTime.TryParse(dates[i], out var date))
                {
                    item.Date = date;
                }

                if(double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    if(d == 0) d = double.NaN;
                    item.Value = d;
                }
                results.Add(item);
            }

            return results;
        }

        public double GetMinValue()
        {
            return Math.Round(_data.Where(d => !double.IsNaN(d.Value)).Min(v => v.Value) - 0.1, 1);
        }

        public double GetMaxValue()
        {
            return Math.Round(_data.Where(d => !double.IsNaN(d.Value)).Max(v => v.Value) + 0.1, 1);
        }

        public DateTime GetMinDate()
        {
            return _data.Min(d => d.Date);
        }

        public DateTime GetMaxDate()
        {
            return _data.Max(d => d.Date);
        }
    }
}