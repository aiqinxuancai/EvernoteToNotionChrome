
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EvernoteToNotionChrome.Service
{
    public class HtmlManager
    {

        public static void UploadHtmlData(string filePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            var path = Path.GetDirectoryName(filePath);
            var savePath = path + @"\Replace\" + Path.GetFileName(filePath);

            if (File.Exists(savePath))
            {
                //已经存在 不继续处理
                return;
            }


            if (!Directory.Exists(path + @"\Replace\"))
            {
                Directory.CreateDirectory(path + @"\Replace\");
            }

            List<string> imageList = new List<string>();

            List<string> fileList = new List<string>();

            //处理img标签
            var nodes = doc.DocumentNode.SelectNodes("//img");

            if (nodes == null)
            {
                File.WriteAllText(path + @"\Replace\" + Path.GetFileName(filePath), File.ReadAllText(filePath));
                return;
            }

            GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"处理HTML：{filePath}");

            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (src == null)
                {
                    continue;
                }

                GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"处理IMAGE标签：{src.Value}" );

                if (link.ParentNode.Name == "a")
                {
                    Debug.WriteLine("上级node是a标签，文件类型，不按照图片处理");
                    HtmlNode parentNode = link.ParentNode;
                    string href = parentNode.Attributes["href"].Value; //文件路径
                    fileList.Add(href);
                }
                else if (!src.Value.Contains("en_todo"))
                {
                    //读取图片尺寸并替换到image标签，让notion识别宽高
                    var fullFilePath = path + @"\" + src.Value.Replace(@"/", @"\");
                    Size imageSize = GetImageSize(fullFilePath);
                    link.SetAttributeValue("style", $"width: {imageSize.Width}px; height: {imageSize.Height}px;");

                    //如果上级是a标签，则不处理，后续按照a标签处理
                    imageList.Add(src.Value);
                }
                else
                {
                    ConversionTodoList(link, path, src.Value);
                }
            }

            //上传所有图片并替换到html文件中
            var docString = UploadAllImage(imageList, path, doc.DocumentNode.OuterHtml);

            docString = UploadAllImage(fileList, path, docString);


            Debug.WriteLine(doc.DocumentNode.OuterHtml);

            File.WriteAllText(savePath, doc.DocumentNode.OuterHtml);

        }

        static Size GetImageSize(string path)
        {
            Image image = Bitmap.FromFile(path);
            return image.Size;
        }


        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="localImagePaths"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static string UploadAllImage(List<string> imageList, string path, string doc)
        {
            var docString = doc;
            foreach (string file in imageList)
            {
                if (file.StartsWith("file:") || file.StartsWith("http:") || file.StartsWith("https:"))
                {
                    continue;
                }

                var fullFilePath = path + @"\" + file.Replace(@"/", @"\");

                Debug.WriteLine(fullFilePath);
                GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"上传中：{fullFilePath}");
                var result = UploadManager.UploadFile(fullFilePath).Result;
                if (string.IsNullOrEmpty(result))
                {
                    continue; //失败
                }
                result = result.Substring(0, result.IndexOf("?Content-Type="));
                Debug.WriteLine(docString);
                //替换
                if (!string.IsNullOrEmpty(result))
                {
                    docString = docString.Replace(file, result);
                }

            }
            return docString;
        }



        private static void ConversionTodoList(HtmlNode node, string basePath, string path)
        {

            var parentNode = node.ParentNode;
            var fullFilePath = basePath + @"\" + path.Replace(@"/", @"\");
            node.ParentNode.RemoveChild(node, false); //<-- keepGrandChildren

            if (string.IsNullOrWhiteSpace(parentNode.InnerText))
            {
                return;
            }

            //处理todolist
            if (File.ReadAllBytes(fullFilePath).Length == 336)
            {
                parentNode.InnerHtml = parentNode.InnerHtml.Replace(parentNode.InnerText, "- [x]  " + parentNode.InnerText);
            }
            else if (File.ReadAllBytes(fullFilePath).Length == 202)
            {
                parentNode.InnerHtml = parentNode.InnerHtml.Replace(parentNode.InnerText, "- [ ]  " + parentNode.InnerText);
            }
        }

        //替换





    }
}
