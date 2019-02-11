using System;
using System.Collections.Generic;

namespace EuriborHistory.Model
{
    public interface IDataService
    {
        void GetData(Action<List<DataItem>, Exception> callback);

        void LoadDataAsync();
    }
}
