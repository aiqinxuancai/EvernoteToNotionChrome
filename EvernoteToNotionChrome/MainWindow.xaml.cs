using Aspose.Words;
using CefSharp;
using CefSharp.Internals;
using EvernoteToNotionChrome.Models;
using EvernoteToNotionChrome.Service;
using EvernoteToNotionChrome.Utils;
using EvernoteToNotionChrome.Views;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
using Wpf.Ui;
using Wpf.Ui.Controls;
using Xceed.Document.NET;
using Xceed.Words.NET;




namespace EvernoteToNotionChrome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow, IContentDialogService, INotifyPropertyChanged
    {
        public static MainWindow Instance { set; get; }


        public MainWindowViewModel ViewModel { get; }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public INavigationView GetNavigation()
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type pageType)
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public void SetPageService(IPageService pageService)
        {
            throw new NotImplementedException();
        }

        public void ShowWindow()
        {
            throw new NotImplementedException();
        }

        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        public void SetContentPresenter(ContentPresenter contentPresenter)
        {
            throw new NotImplementedException();
        }

        public ContentPresenter GetContentPresenter()
        {
            throw new NotImplementedException();
        }

        public Task<ContentDialogResult> ShowAlertAsync(string title, string message, string closeButtonText, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ContentDialogResult> ShowSimpleDialogAsync(SimpleContentDialogCreateOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        public MainWindow()
        {
            Instance = this;

           

            InitializeComponent();
            DataContext = this;

            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.Javascript = CefSharp.CefState.Enabled;
            browserSettings.JavascriptAccessClipboard = CefSharp.CefState.Enabled;
            browserSettings.JavascriptDomPaste = CefSharp.CefState.Enabled;
            Browser.BrowserSettings = browserSettings;
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.RequestHandler = new OxoRequestHandler();
            Browser.Address = "https://notion.so/";
            
            var screenInfo = ((IRenderWebBrowser)Browser).GetScreenInfo();

            GlobalNotification.Default.Register(GlobalNotification.NotificationOutputLogInfo, this, (msg) => {
                Debug.WriteLine(msg.Source);
                this.Dispatcher.Invoke(() => {
                    //TextBlockStatus.Text = (string)msg.Source;
                    Status = (string)msg.Source;
                });

            });

            _titleBar.Title = @$"EvernoteToNotionChrome {ActionVersion.Version}";

            GAHelper.Instance.RequestPageView($"启动到主界面{ActionVersion.Version}");
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {

            ButtonStart.IsEnabled = false;
            var path = TextBoxPath.Text;


            UploadProgressView.Visibility = Visibility.Visible;


            await Task.Run(() => {
                StartWithPath(path);
            });


            UploadProgressView.Visibility = Visibility.Collapsed;
            ButtonStart.IsEnabled = true;

        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            //await UploadManager.UploadFile(@"C:\Users\aiqin\Documents\Tencent Files\76835052\FileRecv\tinified (5).zip");
            //DialogResultText = result switch
            //{
            //    ContentDialogResult.Primary => "User saved their work",
            //    ContentDialogResult.Secondary => "User did not save their work",
            //    _ => "User cancelled the dialog"
            //};

        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private async void StartWithPath(string path)
        {
            if (Directory.Exists(path))
            {

                var files = Directory.GetFiles(path, "*.html");

                var allImages = 0;
                foreach (var item in files)
                {
                    allImages += HtmlManager.GetHTMLImageCount(item);
                }
                //总图片

                var successCount = 0;
                foreach (var item in files)
                {
                    try
                    {
                        GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"正在处理：{Path.GetFileName(item)}");

                        var document = new Aspose.Words.Document(item);

                        var newPath = Path.GetDirectoryName(item) + "/DOCX/" + Path.GetFileName(item) + ".docx";

                        // 将 HTML 文件转换为 Word DOCX 格式
                        document.Save(newPath, SaveFormat.Docx);

                        // 移除部分信息
                        DocXHelper.ModifyDocument(newPath, "Aspose.Words");

                        // 旧的上传模式
                        //HtmlManager.UploadHtmlData(item);

                        successCount++;
                    }
                    catch (Exception ex) { 
                    
                    
                    }

                }

                await this.Dispatcher.Invoke(async () => 
                {
                    Status = @"完成，处理完成的HTML存储在\DOCX";
                    //TODO 弹出完成提示，并前往Replace

                    var service = App.GetService<IContentDialogService>();
                    service.SetContentPresenter(DialogPresenter);
                    var result = await service.ShowSimpleDialogAsync(
                        new SimpleContentDialogCreateOptions()
                        {
                            Title = "成功",
                            Content = Status,
                            PrimaryButtonText = "打开目录",
                            CloseButtonText = "关闭",
                        }
                    );

                    if (result == ContentDialogResult.Primary)
                    {
                        //打开目录
                    }

                });
                
            }
            else
            {
                await this.Dispatcher.Invoke(async () =>
                {
                    Status = @"目录错误";
                    //TODO 弹出错误提示

                    var service = App.GetService<IContentDialogService>();
                    service.SetContentPresenter(DialogPresenter);
                    var result = await service.ShowSimpleDialogAsync(
                        new SimpleContentDialogCreateOptions()
                        {
                            Title = "失败",
                            Content = "输入的目录不存在",
                            CloseButtonText = "取消",
                        }
                    );
                });
                
            }
        }
        

    }
}
