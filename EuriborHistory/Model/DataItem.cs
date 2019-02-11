//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\DataItem.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
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