//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Design\DesignDataService.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EuriborHistory.Design
{
    public class DesignDataService : IDataService
    {
        public void GetData(Action<List<DataItem>, Exception> callback)
        {
            // Use this to create design time data

            var item = new List<DataItem>();
            callback(item, null);
        }

        public Task LoadDataAsync() => throw new NotImplementedException();
    }
}