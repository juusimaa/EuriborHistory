//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataParserBase.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Model.FileParsing;
using EuriborHistory.Model.FileParsing.CsvFiles;
using EuriborHistory.Model.FileParsing.XlsFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace EuriborHistory.Model
{
    public class DataReader : IDataReader
    {
        public string UrlBase { get; set; }

        public string DownloadPath { get; set; }

        public IEnumerable<DataItem> ReadFile(string[] fileNames)
        {
            if (!Directory.Exists(DownloadPath))
            {
                Directory.CreateDirectory(DownloadPath);
            }

            var data = new List<DataItem>();

            foreach (var f in fileNames)
            {
                DownloadFile(f);

                switch (Path.GetExtension(f))
                {
                    case ".xls":
                        data.AddRange(ReadXlsFile(Path.Combine(DownloadPath, f)));
                        break;

                    case ".csv":
                        data.AddRange(ReadCsvFile(Path.Combine(DownloadPath, f)));
                        break;
                }
            }

            return data;
        }

        private void DownloadFile(string filename)
        {
            if(DateTime.Now - File.GetCreationTime(Path.Combine(DownloadPath, filename)) > TimeSpan.FromDays(1))
            {
                var webClient = new WebClient();
                webClient.DownloadFile(UrlBase + filename, Path.Combine(DownloadPath, filename));
                webClient.Dispose();
            }
        }

        private IEnumerable<DataItem> ReadCsvFile(string filename)
        {
            var parser = new CsvParser();
            return parser.ParseFile(filename);
        }

        private IEnumerable<DataItem> ReadXlsFile(string filename)
        {
            var results = new List<DataItem>();
            var year = ParseYearFromFilename(filename);
            FileParser parser;

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

            return parser.ParseFile(filename);
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
    }
}
