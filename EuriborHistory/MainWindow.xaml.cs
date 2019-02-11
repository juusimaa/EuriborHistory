//-----------------------------------------------------------------------
// <copyright file="C:\Users\jouni\source\EuriborHistory\EuriborHistory\MainWindow.xaml.cs" company="">
//     Author: Jouni Uusimaa
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using EuriborHistory.ViewModel;
using System.Windows;

namespace EuriborHistory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}