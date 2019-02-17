//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\FileParsing\CsvFiles\CsvParser.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Enums;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

namespace EuriborHistory.Model.FileParsing.CsvFiles
{
    public class CsvParser : FileParser
    {
        public override IEnumerable<DataItem> ParseFile(string filename)
        {
            var results = new List<DataItem>();

            using(TextFieldParser parser = new TextFieldParser(filename))
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
                            results.AddRange(GetDataList(dates,
                                                         set,
                                                         EuriborPeriod.OneWeek));
                            break;

                        case EuriborPeriod.OneMonth:
                            results.AddRange(GetDataList(dates,
                                                         set,
                                                         EuriborPeriod.OneMonth));
                            break;

                        case EuriborPeriod.ThreeMonths:
                            results.AddRange(GetDataList(dates,
                                                         set,
                                                         EuriborPeriod.ThreeMonths));
                            break;

                        case EuriborPeriod.SixMonths:
                            results.AddRange(GetDataList(dates,
                                                         set,
                                                         EuriborPeriod.SixMonths));
                            break;

                        case EuriborPeriod.TwelveMonths:
                            results.AddRange(GetDataList(dates,
                                                         set,
                                                         EuriborPeriod.TwelveMonths));
                            break;
                    }
                }
            }

            return results;
        }
    }
}
