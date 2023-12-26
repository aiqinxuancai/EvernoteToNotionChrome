
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace EvernoteToNotionChrome.Service
{
    public class HtmlManager
    {

        /// <summary>
        /// 返回图片总数
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int UploadHtmlData(string filePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            var path = Path.GetDirectoryName(filePath);
            var savePath = path + @"\Replace\" + Path.GetFileName(filePath);

            if (!AppConfig.Instance.ConfigData.Overwrite && File.Exists(savePath))
            {
                //已经存在 不继续处理
                return 0;
            }

            if (!Directory.Exists(path + @"\Replace\"))
            {
                Directory.CreateDirectory(path + @"\Replace\");
            }

            //处理img标签
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes == null)
            {
                File.WriteAllText(path + @"\Replace\" + System.IO.Path.GetFileName(filePath), File.ReadAllText(filePath));
                return 0;
            }
           
           

            Debug.WriteLine($"开始路径{filePath}");
            Debug.WriteLine($"找到图片{nodes.Count}个");

            List<string> imageList = GetDocAllImageLabel(path, nodes);
            List<string> fileList = GetDocAllFileLabel(nodes);
            FixTodoListNode(path, nodes);

            var docString = doc.DocumentNode.OuterHtml;

            Debug.WriteLine($"处理图片");
            foreach (var item in imageList)
            {
         
                Debug.WriteLine($"处理图片{Path.GetDirectoryName( filePath) + "/" + item}");
                var imageBytes = File.ReadAllBytes(Path.GetDirectoryName(filePath) + "/" + item);

                string base64ImageRepresentation = Convert.ToBase64String(imageBytes);
                var image = $"data:image/jpeg;base64,{base64ImageRepresentation}";

                docString = docString.Replace(item, image);

            }

            //上传所有图片并替换到html文件中
            // var docString = UploadAllImage(imageList, path, doc.DocumentNode.OuterHtml);

            //a标签判定为文件类型，不上传
            //docString = UploadAllImage(fileList, path, docString);

            File.WriteAllText(savePath, docString);
            return imageList.Count;

        }

        public static int GetHTMLImageCount(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);
            HtmlDocument doc = new HtmlDocument();

            doc.Load(filePath);
            //处理img标签
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes == null)
            {
                return 0;
            }

            List<string> imageList = GetDocAllImageLabel(path, nodes);
            return imageList.Count;
        }



        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<string> GetDocAllImageLabel(string path, HtmlNodeCollection nodes)
        {
            List<string> imageList = new List<string>();
            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (src == null)
                {
                    continue;
                }

                if (link.ParentNode.Name == "a") //上级node是a标签，文件类型，不按照图片处理;
                {
                    continue;
                }
                else if (src.Value.Contains("en_todo"))
                {
                    continue;
                }
                else
                {
                    //读取图片尺寸并替换到image标签，让notion识别宽高
                    var fullFilePath = path + @"\" + src.Value.Replace(@"/", @"\");
                    Size imageSize = GetImageSize(fullFilePath);
                    link.SetAttributeValue("style", $"width: {imageSize.Width}px; height: {imageSize.Height}px;");

                    //如果上级是a标签，则不处理，后续按照a标签处理
                    imageList.Add(src.Value);
                }
            }
            return imageList;
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<string> GetDocAllFileLabel(HtmlNodeCollection nodes)
        {
            List<string> fileList = new List<string>();
            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (src == null)
                {
                    continue;
                }

                if (link.ParentNode.Name == "a") //上级node是a标签，文件类型，不按照图片处理;
                {
                    HtmlNode parentNode = link.ParentNode;
                    string href = parentNode.Attributes["href"].Value; //文件路径
                    fileList.Add(href);
                }
            }
            return fileList;
        }

        /// <summary>
        /// 修复todolist
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static void FixTodoListNode(string path, HtmlNodeCollection nodes)
        {
            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (src == null)
                {
                    continue;
                }

                if (link.ParentNode.Name == "a")
                {
                    continue;
                }
                else if (src.Value.Contains("en_todo"))
                {
                    ConversionTodoList(link, path, src.Value);
                }
            }
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
                //GlobalNotification.Default.Post(GlobalNotification.NotificationOutputLogInfo, $"上传中：{fullFilePath}");
                var result = UploadManager.UploadFile(fullFilePath).Result;
                if (string.IsNullOrEmpty(result))
                {
                    continue; //失败
                }
                //result = result.Substring(0, result.IndexOf("?Content-Type="));

                Debug.WriteLine("最后上传地址处理前：" + result);

                var index = result.IndexOf("?Content-Type=");
                if (index == -1)
                {
                    index = result.IndexOf("?X-Amz");
                }

                //result = result.Substring(0, index);
                Debug.WriteLine("最后上传地址：" + result);
                //替换
                if (!string.IsNullOrEmpty(result))
                {
                    //
                    //result = result.Replace("?", "%3F").Replace("&", "%26");

                    docString = docString.Replace(file, result);
                    Debug.WriteLine("替换地址：" + file + " -> " + result);
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
