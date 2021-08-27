using CefSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvernoteToNotionChrome.Service
{
    public class UploadManager
    {
        public static bool IsUploading { set; get; }
        public static string LastUploadFileUrl { set; get; }


        public static async Task<string> UploadFile(string filePath)
        {
            IsUploading = true;
            LastUploadFileUrl = "";

            MainWindow.SingleInstance.Dispatcher.Invoke(() => {
                BitmapSource bSource = new BitmapImage(new Uri(filePath));
                Clipboard.SetImage(bSource);
                MainWindow.SingleInstance.Browser.Paste();
            });

            //等待上传完毕
            await Task.Run(() =>
            {
                while (IsUploading)
                {
                    Thread.Sleep(1000);
                }
            });

            //上传完毕
            return LastUploadFileUrl;
        }


        public static void PutSuccess(string url)
        {
            LastUploadFileUrl = url;
            IsUploading = false;
        }
    }
}
