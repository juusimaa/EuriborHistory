//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataService.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

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
            IDataReader parser = new DataReader { UrlBase = _urlBase, DownloadPath = _downloadPath };
            _data = parser.ReadFile(_fileNames).ToList();
        }
    }
}