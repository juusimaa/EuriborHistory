﻿//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\IDataService.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace EuriborHistory.Model
{
    public interface IDataService
    {
        void GetData(Action<List<DataItem>, Exception> callback);

        void LoadData();

        double GetMinValue();

        double GetMaxValue();

        DateTime GetMinDate();

        DateTime GetMaxDate();

        double GetMinValue(DateTime startDate, DateTime endDate);

        double GetMaxValue(DateTime startDate, DateTime endDate);
    }
}
