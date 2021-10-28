using CefSharp;
using CefSharp.Internals;
using EvernoteToNotionChrome.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
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

            bool hasError = false;
            MainWindow.SingleInstance.Dispatcher.Invoke(() => {
                try
                {
                	BitmapSource bSource = new BitmapImage(new Uri(filePath));
                    Clipboard.SetImage(bSource);
                    MainWindow.SingleInstance.Browser.Paste();
                }
                catch (NotSupportedException ex)
                {
                    //filePath不是图片
                    Debug.WriteLine(ex);
                    StringCollection list = new ();
                    list.Add(filePath);
                    Debug.WriteLine("设置文件到剪贴板");
                    //Clipboard.SetFileDropList(list);
                    //MainWindow.SingleInstance.Browser.Paste();
                    //Core.DragData.Create()
                    //IDragData dragData = DragData.Create();
                    //dragData.FileName = filePath;
                    //dragData.IsFile = true;

                    //MainWindow.SingleInstance.Browser.GetBrowserHost().GetWindowHandle()
                    Win32Drap.SendFileDrop(new WindowInteropHelper(MainWindow.SingleInstance).Handle, filePath, 550, 400);

                    //((IRenderWebBrowser)MainWindow.SingleInstance.Browser).StartDragging(dragData, CefSharp.Enums.DragOperationsMask.Move, 550, 400);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine(ex);
                    hasError = true;
                }
            });


            if (hasError)
            {
                return "";
            }

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
