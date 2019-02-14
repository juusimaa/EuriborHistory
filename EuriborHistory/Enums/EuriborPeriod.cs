//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Enums\EuriborPeriod.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel;

namespace EuriborHistory.Enums
{
    public enum EuriborPeriod
    {
        [Description("Not defined")]
        NotDefined,

        [Description("1 week")]
        OneWeek,

        [Description("1 month")]
        OneMonth,

        [Description("3 months")]
        ThreeMonths,

        [Description("6 months")]
        SixMonths,

        [Description("12 months")]
        TwelveMonths
    }
}
