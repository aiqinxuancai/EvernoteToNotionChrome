using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace HTMLToDocX.Utils
{
    internal class DocXHelper
    {
        public static void ModifyDocument(string filePath, string textToDetect)
        {
            using (DocX document = DocX.Load(filePath))
            {
                // 移除段落
                RemoveParagraphsContainingText(document, textToDetect);

                // 移除所有页脚
                RemoveFooters(document);

                // 保存修改后的文档
                //string outputPath = "output.docx";
                document.SaveAs(filePath);
            }
        }

        static void RemoveParagraphsContainingText(DocX document, string textToDetect)
        {
            for (int i = document.Paragraphs.Count - 1; i >= 0; i--)
            {
                var paragraph = document.Paragraphs[i];

                if (paragraph.Text.Contains(textToDetect))
                {
                    paragraph.Remove(false);
                }
            }
        }

        static void RemoveFooters(DocX document)
        {
            RemoveFooterParagraphs(document.Footers.Odd);
            RemoveFooterParagraphs(document.Footers.First);
            RemoveFooterParagraphs(document.Footers.Even);
        }

        static void RemoveFooterParagraphs(Footer footer)
        {
            if (footer != null)
            {
                for (int i = footer.Paragraphs.Count - 1; i >= 0; i--)
                {
                    footer.Paragraphs[i].Remove(false);
                }
            }
        }
    }
}
