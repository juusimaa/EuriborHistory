//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\XlsData\XlsParserOld.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EuriborHistory.Model.FileParsing.XlsFiles
{
    public class XlsParserOld : FileParser
    {
        private readonly int _year;

        public XlsParserOld(int year)
        {
            _year = year;
        }

        public override IEnumerable<DataItem> ParseFile(string filename)
        {
            var data = new List<DataItem>();

            HSSFWorkbook hssfwb;
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            if (hssfwb.GetSheet($"hist_EURIBOR_{_year}") is ISheet sheet)
            {
                // 1st column is dates
                var dates = new List<DateTime>();
                var enUS = new CultureInfo("en-US");

                for (var row = 2; row <= sheet.LastRowNum; row++)
                {
                    var str = sheet.GetRow(row).Cells[0].ToString();
                    if (DateTime.TryParseExact(str, "MM/dd/yy", enUS, DateTimeStyles.None, out var d))
                    {
                        dates.Add(d);
                    }
                }
                dates = dates.OrderBy(d => d.Date).ToList();

                // 1st row is periods
                var firstRow = sheet.GetRow(0);
                foreach (var cell in firstRow.Cells)
                {
                    var rates = GetRates(sheet, cell);

                    switch (ParsePeriod(cell.StringCellValue))
                    {
                        case EuriborPeriod.OneWeek:
                             data.AddRange(GetDataList(dates, rates, EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }

            return data;
        }

        private List<double> GetRates(ISheet sheet, ICell cell)
        {
            var rates = new List<double>();

            for (var row = 2; row <= sheet.LastRowNum; row++)
            {
                var dataFormatter = new DataFormatter(CultureInfo.InvariantCulture);
                var c = sheet.GetRow(row).Cells[cell.ColumnIndex];

                if (double.TryParse(dataFormatter.FormatCellValue(c), out var d))
                {
                    rates.Add(d);
                }
            }

            return rates;            
        }
    }
}
