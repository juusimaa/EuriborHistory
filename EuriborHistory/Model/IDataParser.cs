//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\Model\IDataParser.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;

namespace EuriborHistory.Model
{
    public interface IDataParser
    {
        IEnumerable<DataItem> ParseData(string filename);
    }
}
