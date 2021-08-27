using EvernoteToNotion.Service;
using System;
using System.IO;

namespace EvernoteToNotion
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start！");





//#if DEBUG
            //TEST
            //var path = @"C:\Users\aiqin\Desktop\全部导出\开发";
            //foreach (var item in Directory.GetFiles(path, "*.html"))
            //{
            //    HtmlManager.UploadHtmlData(item);
            //}
//#else
            try
            {
	            if (args.Length > 0)
	            {
	                StartWithPath(args[0]);
	            }
	            else
	            {
	                Console.WriteLine(@$"是否使用当前目录{Directory.GetCurrentDirectory()}？（Y or N）");
	                string input = Console.ReadLine();
	                if (input.ToUpper() == "Y")
	                {
	                    StartWithPath(Directory.GetCurrentDirectory());
	                }
	            }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
//#endif


        }


        static void StartWithPath(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetFiles(path))
                {
                    HtmlManager.UploadHtmlData(item);
                }

                Console.WriteLine(@"已经完成，处理完成的HTML存储在\Replace下");
            }
            else
            {
                Console.WriteLine(@"参数错误：需要传递一个目录");
            }
        }



    }
}
