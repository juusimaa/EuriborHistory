//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\XlsData\XlsParser.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace EuriborHistory.Model.XlsData
{
    public class XlsParser : DataParser
    {
        public override IEnumerable<DataItem> ParseData(string filename)
        {
            var data = new List<DataItem>();

            HSSFWorkbook hssfwb;
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }

            if (hssfwb.GetSheet("Euribor") is ISheet sheet)
            {
                // 1st row is dates
                var datesRow = sheet.GetRow(0);
                var dates = new List<DateTime>();
                foreach (var cell in datesRow.Cells)
                {
                    dates.Add(cell.DateCellValue);
                }

                // rest are rates
                for (var row = 1; row <= sheet.LastRowNum; row++)
                {
                    var currentRow = sheet.GetRow(row);
                    var rates = new List<double>();

                    // TODO: dates and rates should have same count

                    if (currentRow.Cells[0].CellType != CellType.String)
                    {
                        break;
                    }

                    switch (ParsePeriod(currentRow.Cells[0].StringCellValue))
                    {
                        case EuriborPeriod.OneWeek:
                            ParseRow(currentRow, rates);
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            ParseRow(currentRow, rates);
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            ParseRow(currentRow, rates);
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            ParseRow(currentRow, rates);
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            ParseRow(currentRow, rates);
                            data.AddRange(GetDataList(dates, rates, EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }

            return data;
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
    }
}
