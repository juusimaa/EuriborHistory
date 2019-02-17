//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataParserBase.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using EuriborHistory.Model.XlsData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EuriborHistory.Model
{
    public class DataParser : IDataParser
    {
        public virtual IEnumerable<DataItem> ParseData(string filename)
        {
            var results = new List<DataItem>();
            var year = ParseYearFromFilename(filename);
            IDataParser parser;

            if (!year.HasValue)
            {
                return results;
            }

            if (year < 2007)
            {
                parser = new XlsParserOld(year.Value);
            }
            else
            {
                parser = new XlsParser();
            }

            return parser.ParseData(filename);
        }

        private int? ParseYearFromFilename(string filename)
        {
            var result = new string(filename.Where(char.IsDigit).ToArray());

            if (int.TryParse(result, out var i))
            {
                return i;
            }

            return null;
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

        protected IEnumerable<DataItem> GetDataList(IEnumerable<DateTime> dates,
                                                    IEnumerable<double> values,
                                                    EuriborPeriod period)
        {
            var results = new List<DataItem>();

            for(var i = 0; i < dates.Count(); i++)
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
    }
}
