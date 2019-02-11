using System;

namespace EuriborHistory.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem();
            callback(item, null);
        }
    }
}