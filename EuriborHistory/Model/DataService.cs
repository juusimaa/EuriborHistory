using EuriborHistory.Enums;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace EuriborHistory.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<List<DataItem>, Exception> callback)
        {
            // Use this to connect to the actual data service

            callback(_data, null);
        }

        private List<DataItem> _data = new List<DataItem>();

        public async void LoadDataAsync()
        {
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(@"https://www.emmi-benchmarks.eu/assets/modules/rateisblue/file_processing/publication/processed/hist_EURIBOR_2019.csv",
                "2019.csv");
            ParseCsv();
        }

        private void ParseCsv()
        {
            using (TextFieldParser parser = new TextFieldParser("2019.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // 6 lines of data
                var dates = parser.ReadFields();
                var oneWeekData = parser.ReadFields();
                var oneMonthData = parser.ReadFields();
                var threeMonthData = parser.ReadFields();
                var sixMonthData = parser.ReadFields();
                var twelveMonthData = parser.ReadFields();

                _data.AddRange(ParseData(dates, oneWeekData, EuriborPeriod.OneWeek));
                _data.AddRange(ParseData(dates, oneMonthData, EuriborPeriod.OneWeek));
                _data.AddRange(ParseData(dates, threeMonthData, EuriborPeriod.ThreeMonths));
                _data.AddRange(ParseData(dates, sixMonthData, EuriborPeriod.SixMonths));
                _data.AddRange(ParseData(dates, twelveMonthData, EuriborPeriod.TwelveMonths));
            }
        }

        private IEnumerable<DataItem> ParseData(string[] dates, string[] values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            // 1st item is empty or period name so start from index 1
            for (var i = 1; i < dates.Length; i++)
            {
                var item = new DataItem
                {
                    Date = DateTime.Parse(dates[i]),
                    Period = period
                };
                if (decimal.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    item.Value = d;
                }
                results.Add(item);
            }

            return results;
        }
    }
}