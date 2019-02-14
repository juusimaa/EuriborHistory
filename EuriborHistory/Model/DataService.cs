﻿//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataService.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using Microsoft.VisualBasic.FileIO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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
            "2004.xls",
            "2005.xls",
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
            HSSFWorkbook hssfwb;
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            if(hssfwb.GetSheet("Euribor") is ISheet sheet)
            {
                // 1st row is dates
                var datesRow = sheet.GetRow(0);
                var dates = new List<DateTime>();
                foreach(var cell in datesRow.Cells)
                {
                    dates.Add(cell.DateCellValue);
                }

                // rest are rates
                for(var row = 1; row <= sheet.LastRowNum; row++)
                {
                    var currentRow = sheet.GetRow(row);
                    var rates = new List<double>();

                    // TODO: dates and rates should have same count

                    if(currentRow.Cells[0].CellType != CellType.String)
                    {
                        break;
                    }

                    switch(ParsePeriod(currentRow.Cells[0].StringCellValue))
                    {
                        case EuriborPeriod.OneWeek:
                            ParseRow(currentRow, rates);
                            _data.AddRange(ParseData(dates, rates, EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            ParseRow(currentRow, rates);
                            _data.AddRange(ParseData(dates, rates, EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            ParseRow(currentRow, rates);
                            _data.AddRange(ParseData(dates, rates, EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            ParseRow(currentRow, rates);
                            _data.AddRange(ParseData(dates, rates, EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            ParseRow(currentRow, rates);
                            _data.AddRange(ParseData(dates, rates, EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }
        }

        private static void ParseRow(IRow r, List<double> rates)
        {
            foreach (var cell in r.Cells)
            {
                if (cell.CellType == CellType.Numeric)
                {
                    rates.Add(cell.NumericCellValue);
                }
                else
                {
                    rates.Add(double.NaN);
                }
            }
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
                            _data.AddRange(ParseData(dates,
                                                     set,
                                                     EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            _data.AddRange(ParseData(dates,
                                                     set,
                                                     EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            _data.AddRange(ParseData(dates,
                                                     set,
                                                     EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            _data.AddRange(ParseData(dates,
                                                     set,
                                                     EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            _data.AddRange(ParseData(dates,
                                                     set,
                                                     EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }
        }

        private IEnumerable<DataItem> ParseData(IEnumerable<DateTime> dates, IEnumerable<double> values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            for (var i = 0; i < dates.Count(); i++)
            {
                var item = new DataItem
                {
                    Period = period,
                    Date = dates.ElementAt(i),
                    Value = values.ElementAt(i)
                };
                results.Add(item);
            }

            return results;
        }

        private IEnumerable<DataItem> ParseData(string[] dates, string[] values, EuriborPeriod period)
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
            if(str.StartsWith("1w"))
            {
                return EuriborPeriod.OneWeek;
            }
            if(str.StartsWith("1m"))
            {
                return EuriborPeriod.OneMonth;
            }
            if(str.StartsWith("3m"))
            {
                return EuriborPeriod.ThreeMonths;
            }
            if(str.StartsWith("6m"))
            {
                return EuriborPeriod.SixMonths;
            }
            if(str.StartsWith("12m"))
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