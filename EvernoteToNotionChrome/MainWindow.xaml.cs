﻿using CefSharp;
using EvernoteToNotionChrome.Service;
using EvernoteToNotionChrome.Utils;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EvernoteToNotionChrome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow SingleInstance { set; get; }

        public MainWindow()
        {
            SingleInstance = this;

            InitializeComponent();

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.Javascript = CefSharp.CefState.Enabled;
            browserSettings.JavascriptAccessClipboard = CefSharp.CefState.Enabled;
            browserSettings.JavascriptDomPaste = CefSharp.CefState.Enabled;
            Browser.BrowserSettings = browserSettings;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Browser.MouseUp += ChromeMain_MouseUp;
            //Browser.FrameLoadEnd += ChromeMain_FrameLoadEnd;
            //Browser.LoadingStateChanged += ChromeMain_LoadingStateChanged;

            Browser.RequestHandler = new OxoRequestHandler();
            Browser.Address = "https://notion.so/";

            GlobalNotification.Default.Register(GlobalNotification.NotificationOutputLogInfo, this, (msg) => {
                Debug.WriteLine(msg.Source);
                this.Dispatcher.Invoke(() => {
                    TextBlockStatus.Text = (string)msg.Source;
                });

            });

        }


        private async void TestDrag_Click(object sender, RoutedEventArgs e)
        {
            var path = TextBoxPath.Text;
            await Task.Run(() => {
                StartWithPath(path);
            }); 
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void StartWithPath(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetFiles(path, "*.html"))
                {
                    HtmlManager.UploadHtmlData(item);
                }
                this.Dispatcher.Invoke(() =>
                {
                    TextBlockStatus.Text = @"完成，处理完成的HTML存储在\Replace下";
                });
                
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    TextBlockStatus.Text = "目录错误";
                });
                
            }
        }
    }
}