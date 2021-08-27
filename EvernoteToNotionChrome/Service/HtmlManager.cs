
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (!Directory.Exists(path + @"\Replace\"))
            {
                Directory.CreateDirectory(path + @"\Replace\");
            }

            List<string> imageList = new List<string>();

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

                if (!src.Value.Contains("en_todo"))
                {
                    imageList.Add(src.Value);
                }
                else
                {
                    ConversionTodoList(link, path, src.Value);

                }
            }

            //读取图片，上传
            var docString = doc.DocumentNode.OuterHtml;
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
                    continue;
                }
                result = result.Substring(0, result.IndexOf("?Content-Type="));
                Debug.WriteLine(docString);
                //替换
                if (!string.IsNullOrEmpty(result))
                {
                    docString = docString.Replace(file, result);
                }

            }

            Debug.WriteLine(docString);

            File.WriteAllText(path + @"\Replace\" + Path.GetFileName(filePath), docString);

        }



        static void ConversionTodoList(HtmlNode node, string basePath, string path)
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
