using Aspose.Words.Drawing;
using Flurl.Http;
using HtmlAgilityPack;
using HTMLToNotion.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EvernoteToNotion.Services
{

    internal class HtmlImageModel
    {
        public HtmlNode node;
        public string src;
    }

    internal class HtmlFileModel
    {
        public HtmlNode node;
        public string src;
    }

    internal class HtmlManager
    {
        /// <summary>
        /// 处理HTML文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int ModifyHtml(string filePath, string newFilePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);


            if (!Directory.Exists(Path.GetDirectoryName(newFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            }

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
           
            var docString = doc.DocumentNode.OuterHtml;

            foreach (var item in nodeList)
            {
                Debug.WriteLine($"处理图片 {item.src}");

                byte[] imageBytes = new byte[0];

                try
                {
                    if (item.src.StartsWith("http")) //网络图片则下载
                    {
                        imageBytes = item.src.GetBytesAsync().Result;
                    }
                    else
                    {
                        if (System.IO.Path.IsPathRooted(item.src))
                        {
                            imageBytes = File.ReadAllBytes(item.src);
                        }
                        else
                        {

                            var path = Path.GetFullPath(Path.GetDirectoryName(filePath) + "/" + item.src);

                            imageBytes = File.ReadAllBytes(path);
                        }
                    }


                    if (imageBytes.Length > 0)
                    {
                        string base64ImageRepresentation = Convert.ToBase64String(imageBytes);
                        var image = $"data:image/jpeg;base64,{base64ImageRepresentation}";

                        Bitmap bitmap = new Bitmap(new MemoryStream(imageBytes));

                        item.node.SetAttributeValue("src", image);
                        item.node.SetAttributeValue("style", $"width: {bitmap.Width}px; height: {bitmap.Height}px;");
                    }
                } 
                catch (Exception ex)
                {
                
                }

                
            }

            List<HtmlFileModel> fileList = GetDocAllFileLabel(nodes);
   
            foreach (var item in fileList)
            {
                Debug.WriteLine($"处理文件 {item.src}");

                byte[] imageBytes = new byte[0];

                try
                {
                    if (item.src.StartsWith("http")) //网络图片则下载
                    {
                        imageBytes = item.src.GetBytesAsync().Result;
                    }
                    else
                    {
                        if (System.IO.Path.IsPathRooted(item.src))
                        {
                            imageBytes = File.ReadAllBytes(item.src);
                        }
                        else
                        {

                            var path = Path.GetFullPath(Path.GetDirectoryName(filePath) + "/" + item.src);

                            imageBytes = File.ReadAllBytes(path);
                        }
                    }


                    if (imageBytes.Length > 0)
                    {
                        string base64ImageRepresentation = Convert.ToBase64String(imageBytes);
                        var file = $"data:application/octet-stream;base64,{base64ImageRepresentation}";

                      
                        item.node.SetAttributeValue("href", file);
                        item.node.SetAttributeValue("download", Path.GetFileName(item.src));
                    }
                }
                catch (Exception ex)
                {

                }


            }

            File.WriteAllText(newFilePath, doc.DocumentNode.OuterHtml);
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

                HtmlImageModel model = new HtmlImageModel();
                model.src = src.Value;
                model.node = link;
                nodeList.Add(model);
            }
            return nodeList;
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<HtmlFileModel> GetDocAllFileLabel(HtmlNodeCollection nodes)
        {
            List<HtmlFileModel> nodeList = new List<HtmlFileModel>();

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


                    HtmlFileModel model = new HtmlFileModel();
                    model.src = href;
                    model.node = parentNode;
                    nodeList.Add(model);
                }
            }
            return nodeList;
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
