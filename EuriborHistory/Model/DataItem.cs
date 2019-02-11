using EuriborHistory.Enums;
using System;

namespace EuriborHistory.Model
{
    public class DataItem
    {
        public DateTime Date { get; set; }

        public EuriborPeriod Period { get; set; }

        public decimal Value { get; set; }
    }
}