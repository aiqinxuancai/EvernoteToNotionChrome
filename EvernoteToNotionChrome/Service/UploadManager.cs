using CefSharp;
using CefSharp.Internals;
using EvernoteToNotionChrome.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Windows.Media.Imaging;
using System.IO;

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

            Debug.WriteLine($"开始上传{filePath}");

            MainWindow.Instance.Dispatcher.Invoke(() => {
                try
                {
                    using (Image image = Image.Load(filePath)) //使用imagesharp减少错误
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        image.SaveAsPng(memoryStream);
                        BitmapSource bSource = BitmapToBitmapImage(new System.Drawing.Bitmap(memoryStream));
                        Clipboard.SetImage(bSource);

                        MainWindow.Instance.Browser.Paste();
                    }
                }
                catch (NotSupportedException ex)
                {
                    Debug.WriteLine("此文件不是图片");

                    StringCollection filePaths = new StringCollection
                        {
                            filePath
                        };

                    Clipboard.SetFileDropList(filePaths);
                    MainWindow.Instance.Browser.Paste();

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

            int timeSecond = 0;
            //等待上传完毕
            await Task.Run(() =>
            {
                while (IsUploading)
                {
                    Thread.Sleep(1000);
                    timeSecond++;
                    if (timeSecond == 40)
                    {
                        Debug.WriteLine("超时");
                        break;
                    }
                }
            });

            //上传完毕
            return LastUploadFileUrl;
        }

        // Bitmap --> BitmapImage
        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        public static void PutSuccess(string url)
        {
            LastUploadFileUrl = url;
            IsUploading = false;
        }
    }
}
