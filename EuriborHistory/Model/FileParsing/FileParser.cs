//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\FileParsing\FileParser.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EuriborHistory.Model.FileParsing
{
    public abstract class FileParser
    {
        public abstract IEnumerable<DataItem> ParseFile(string filename);

        protected IEnumerable<DataItem> GetDataList(IEnumerable<DateTime> dates,
                                            IEnumerable<double> values,
                                            EuriborPeriod period)
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

        protected IEnumerable<DataItem> GetDataList(string[] dates, string[] values, EuriborPeriod period)
        {
            var results = new List<DataItem>();

            // 1st item is empty or period name so start from index 1
            for (var i = 1; i < dates.Length; i++)
            {
                if (string.IsNullOrEmpty(dates[i]))
                {
                    continue;
                }

                var item = new DataItem { Period = period };

                if (DateTime.TryParse(dates[i], out var date))
                {
                    item.Date = date;
                }

                if (double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    if (d == 0)
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

        protected EuriborPeriod ParsePeriod(string str)
        {
            if (str.StartsWith("1w", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.OneWeek;
            }
            if (str.StartsWith("1m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.OneMonth;
            }
            if (str.StartsWith("3m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.ThreeMonths;
            }
            if (str.StartsWith("6m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.SixMonths;
            }
            if (str.StartsWith("12m", StringComparison.InvariantCultureIgnoreCase))
            {
                return EuriborPeriod.TwelveMonths;
            }

            return EuriborPeriod.NotDefined;
        }
    }
}
