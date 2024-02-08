using Flurl.Http;
using HtmlAgilityPack;
using HTMLToNotion.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace EvernoteToNotion.Services
{

    internal class HtmlImageModel
    {
        public HtmlNode node;
        public string src;
    }



    internal class HtmlManager
    {
        /// <summary>
        /// 返回图片总数
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int UploadHtmlData(string filePath, string newFilePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            //处理img标签
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes == null)
            {
                File.WriteAllText(newFilePath, File.ReadAllText(filePath));
                return 0;
            }

            Debug.WriteLine($"开始路径{filePath}");
            Debug.WriteLine($"找到图片{nodes.Count}个");

            List<HtmlImageModel> nodeList = GetDocAllImageLabel(nodes);
            List<string> fileList = GetDocAllFileLabel(nodes);

            //FixTodoListNode(path, nodes);

            var docString = doc.DocumentNode.OuterHtml;

            Debug.WriteLine($"处理图片");
            foreach (var item in nodeList)
            {
                byte[] imageBytes = new byte[0];

                if (item.src.StartsWith("http"))
                {
                    //网络图片
                    imageBytes = item.src.GetBytesAsync().Result;

                }
                else
                {
                    //本地
                    if (System.IO.Path.IsPathRooted(item.src))
                    {
                        imageBytes = File.ReadAllBytes(item.src);
                    }
                    else
                    {
                        imageBytes = File.ReadAllBytes(System.IO.Path.GetDirectoryName(filePath) + "/" + item.src);
                    }
                }

                //TODO

                //Debug.WriteLine($"处理图片{Path.GetDirectoryName(filePath) + "/" + item}");
                //var imageBytes = File.ReadAllBytes(Path.GetDirectoryName(filePath) + "/" + item);

                //string base64ImageRepresentation = Convert.ToBase64String(imageBytes);
                //var image = $"data:image/jpeg;base64,{base64ImageRepresentation}";

                //docString = docString.Replace(item, image);

            }

            //上传所有图片并替换到html文件中
            // var docString = UploadAllImage(imageList, path, doc.DocumentNode.OuterHtml);

            //a标签判定为文件类型，不上传
            //docString = UploadAllImage(fileList, path, docString);

            File.WriteAllText(newFilePath, docString);
            return nodeList.Count;

        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<HtmlImageModel> GetDocAllImageLabel(HtmlNodeCollection nodes)
        {
            List<HtmlImageModel> nodeList = new List<HtmlImageModel>();
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
                    HtmlImageModel model = new HtmlImageModel();
                    model.src = src.Value;
                    model.node = link;
                    nodeList.Add(model);
                }
            }
            return nodeList;
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
    }
}
