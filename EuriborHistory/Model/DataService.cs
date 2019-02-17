﻿//-----------------------------------------------------------------------
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

namespace EuriborHistory.Model
{
    /// <summary>
    /// TODO: Use System.IO.Abstractions
    /// </summary>
    public class DataService : IDataService
    {
        private const string _urlBase = @"https://www.emmi-benchmarks.eu/assets/modules/rateisblue/file_processing/publication/processed/hist_EURIBOR_";

        private List<DataItem> _data = new List<DataItem>();
        private string _downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() +
            "\\Euribor History";
        private readonly string[] _fileNames =
        {
            //"2004.xls",
            //"2005.xls",
            "2006.xls",
            "2007.xls",
            "2008.xls",
            "2009.xls",
            "2010.csv",
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

        private void DownloadFile(string filename)
        {
            if(DateTime.Now - File.GetCreationTime(Path.Combine(_downloadPath, filename)) > TimeSpan.FromDays(1))
            {
                var webClient = new WebClient();
                webClient.DownloadFile(_urlBase + filename, Path.Combine(_downloadPath, filename));
            }
        }

        private void ParseXls(string filename)
        {
            var parser = new DataParser();
            _data = parser.ParseData(filename).ToList();
        }

        private void ParseCsv(string file)
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
                            _data.AddRange(GetDataList(dates,
                                                     set,
                                                     EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            _data.AddRange(GetDataList(dates,
                                                     set,
                                                     EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            _data.AddRange(GetDataList(dates,
                                                     set,
                                                     EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            _data.AddRange(GetDataList(dates,
                                                     set,
                                                     EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            _data.AddRange(GetDataList(dates,
                                                     set,
                                                     EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }
        }

        private IEnumerable<DataItem> GetDataList(string[] dates, string[] values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            // 1st item is empty or period name so start from index 1
            for(var i = 1; i < dates.Length; i++)
            {
                if (string.IsNullOrEmpty(dates[i]))
                {
                    continue;
                }

                var item = new DataItem { Period = period };

                if(DateTime.TryParse(dates[i], out var date))
                {
                    item.Date = date;
                }

                if(double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    if(d == 0)
                    {
                        d = double.NaN;
                    }
                    item.Value = d;
                }
                else
                {
                    item.Value = double.NaN;
                }
                results.Add(item);
            }

            return results;
        }

        private EuriborPeriod ParsePeriod(string str)
        {
            if(str.StartsWith("1w", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.OneWeek;
            }
            if(str.StartsWith("1m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.OneMonth;
            }
            if(str.StartsWith("3m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.ThreeMonths;
            }
            if(str.StartsWith("6m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.SixMonths;
            }
            if(str.StartsWith("12m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.TwelveMonths;
            }

            return EuriborPeriod.NotDefined;
        }

        public void GetData(Action<List<DataItem>, Exception> callback) => callback(_data, null);

        public DateTime GetMaxDate() => _data.Max(d => d.Date);

        public double GetMaxValue() => Math.Round(_data.Where(d => !double.IsNaN(d.Value)).Max(v => v.Value) + 0.1, 1);

        public double GetMaxValue(DateTime startDate, DateTime endDate) => _data
                .Where(d => d.Date > startDate && d.Date < endDate)
            .Max(d => d.Value);

        public DateTime GetMinDate() => _data.Min(d => d.Date);

        public double GetMinValue() => Math.Round(_data.Where(d => !double.IsNaN(d.Value)).Min(v => v.Value) - 0.1, 1);

        public double GetMinValue(DateTime startDate, DateTime endDate) => _data
                .Where(d => d.Date > startDate && d.Date < endDate)
            .Min(d => d.Value);

        public void LoadData()
        {
            _data.Clear();

            if(!Directory.Exists(_downloadPath))
            {
                Directory.CreateDirectory(_downloadPath);
            }

            foreach(var filename in _fileNames)
            {
                DownloadFile(filename);

                switch(Path.GetExtension(filename))
                {
                    case ".xls":
                        ParseXls(Path.Combine(_downloadPath, filename));
                        break;

                    case ".csv":
                        ParseCsv(Path.Combine(_downloadPath, filename));
                        break;
                }
            }
        }
    }
}