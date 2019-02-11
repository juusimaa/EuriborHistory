using EuriborHistory.Model;
using System;
using System.Collections.Generic;

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

        public void LoadDataAsync()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}