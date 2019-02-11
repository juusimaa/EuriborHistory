//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\App.xaml.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using GalaSoft.MvvmLight.Threading;
using System.Windows;

namespace EuriborHistory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
=> DispatcherHelper.Initialize();
    }
}
