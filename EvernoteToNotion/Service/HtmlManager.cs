
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvernoteToNotion.Service
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
            Console.WriteLine("处理HTML：" + filePath);
            foreach (HtmlNode link in nodes)
            {
                HtmlAttribute src = link.Attributes["src"];
                if (src == null)
                {
                    continue;
                }

                Console.WriteLine("处理IMAGE标签：" + src.Value);

                if (!src.Value.Contains("en_todo"))
                {
                    imageList.Add(src.Value);
                }
                else
                {
                    link.ParentNode.RemoveChild(link, false); //<-- keepGrandChildren
                }
            }

            //读取图片，上传至cos
            var docString = doc.DocumentNode.OuterHtml;
            foreach (string file in imageList)
            {
                if (file.StartsWith("file:") || file.StartsWith("http:") || file.StartsWith("https:"))
                {
                    continue;
                }

                var fullFilePath = path + @"\" + file.Replace(@"/", @"\");

                Debug.WriteLine(fullFilePath);
                var result = COSManager.UploadFile(fullFilePath).Result;

                //替换
                if (!string.IsNullOrEmpty(result))
                {
                    docString = docString.Replace(file, result);
                }

            }

            Debug.WriteLine(docString);

            File.WriteAllText(path + @"\Replace\" + Path.GetFileName(filePath), docString);

        }



        //替换





    }
}
