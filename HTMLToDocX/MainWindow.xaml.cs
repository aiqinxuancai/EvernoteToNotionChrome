using Aspose.Words;
using HTMLToDocX.Services;
using HTMLToDocX.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Xceed.Words.NET;

namespace HTMLToDocX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

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

        private async void StartWithPath(string path)
        {
            var docxPath = Path.Combine(path, "DOCX");

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.html");


                var successCount = 0;
                foreach (var item in files)
                {
                    try
                    {
                        //GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"正在处理：{Path.GetFileName(item)}");
                        this.Dispatcher.Invoke(() =>
                        {
                            UploadProgressView.Status = $"正在处理：{Path.GetFileName(item)}";
                        });
                        

                        var document = new Aspose.Words.Document(item);

                        var newPath = Path.Combine(docxPath, Path.GetFileName(item) + ".docx");

                        // 将 HTML 文件转换为 Word DOCX 格式
                        document.Save(newPath, SaveFormat.Docx);

                        // 移除部分信息
                        DocXHelper.ModifyDocument(newPath, "Aspose.Words");

                        successCount++;
                    }
                    catch (Exception ex)
                    {


                    }

                }

                await this.Dispatcher.Invoke(async () =>
                {
                    var service = App.GetService<IContentDialogService>();
                    service.SetContentPresenter(DialogPresenter);
                    var result = await service.ShowSimpleDialogAsync(
                        new SimpleContentDialogCreateOptions()
                        {
                            Title = "成功",
                            Content = "完成，处理完成的HTML存储在\\DOCX\r\n只需将.docx文件导入Notion既可",
                            PrimaryButtonText = "打开目录",
                            CloseButtonText = "关闭",
                        }
                    );

                    if (result == ContentDialogResult.Primary)
                    {
                        //打开目录
                        Process.Start("explorer.exe", docxPath);
            
                    }

                });

            }
            else
            {
                await this.Dispatcher.Invoke(async () =>
                {
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