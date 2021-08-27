using CefSharp;
using CefSharp.Handler;
using EvernoteToNotionChrome.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvernoteToNotionChrome.Utils
{
    public class OxoRequestHandler : RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture,
            bool isRedirect)
        {
            // 先调用基类的实现，断点调试
            return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // 先调用基类的实现，断点调试
            return new OxoResourceRequestHandler();
        }
    }

    class OxoResourceRequestHandler : ResourceRequestHandler
    {
        private Dictionary<UInt64, MemoryStreamResponseFilter> responseDictionary = new Dictionary<UInt64, MemoryStreamResponseFilter>();

        //public  _requestHeandler;
        public OxoResourceRequestHandler() : base()
        {
            
        }


        protected override void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback); /*(browserControl, browser, frame, request, callback);*/
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            try
            {

                //https://www.notion.so/api/v3/getUploadFileUrl
                //if (request.Method == "POST" && request.Url.EndsWith("getUploadFileUrl"))
                //{
                //    var dataFilter = new MemoryStreamResponseFilter(); //新建成数据 处理器
                //    responseDictionary.Add(request.Identifier, dataFilter);
                //    return dataFilter;
                //}

                if (request.Method == "PUT" && request.Url.Contains("X-Amz-Signature"))
                {
                    var dataFilter = new MemoryStreamResponseFilter(); //新建成数据 处理器
                    responseDictionary.Add(request.Identifier, dataFilter);
                    return dataFilter;
                }
            }
            catch (System.Exception ex)
            {

            }
            finally
            {

            }
            return base.GetResourceResponseFilter(browserControl, browser, frame, request, response);
        }


        Random _rand = new Random();

        protected override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            try
            {
                Uri.TryCreate(request.Url, UriKind.Absolute, out var url);
                var extension = url?.ToString()?.ToLower();

                Debug.WriteLine($@"LoadComplete:{request.Url}");

                MemoryStreamResponseFilter filter;
                if (responseDictionary.TryGetValue(request.Identifier, out filter))
                {
                    Debug.WriteLine("------------------------------------------------------------------------");
                    Debug.WriteLine(extension);


                    if (request.Method != "PUT")
                    {
                        string postData = "";
                        if (request.PostData?.Elements?.Count > 0)
                        {
                            foreach (var item in request.PostData.Elements)
                            {
                                Debug.WriteLine(Encoding.UTF8.GetString(item.Bytes));
                                postData += Encoding.UTF8.GetString(item.Bytes);
                            }
                        }

                        Debug.WriteLine("responseDictionary.Count:" + responseDictionary.Count);

                        var data = filter.Data;
                        var dataLength = filter.Data.Length;
                        var dataAsUtf8String = Encoding.UTF8.GetString(data);
                        Debug.WriteLine(dataAsUtf8String);
                    }
                    else
                    {
                        //PUT
                        if (response.StatusCode == 200)
                        {
                            Debug.WriteLine("PUT完成");
                            UploadManager.PutSuccess(request.Url); ;
                        }
                    }


                    //数据到达
                    //HttpPacketHookManager.PacketRoute(url.PathAndQuery.Replace("/", "_"), dataAsUtf8String, postData, "", "");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                if (responseDictionary.ContainsKey(request.Identifier))
                {
                    responseDictionary.Remove(request.Identifier); // 释放
                    Debug.WriteLine($"数据包缓存量：{responseDictionary.Count}");
                }

                base.OnResourceLoadComplete(browserControl, browser, frame, request, response, status, receivedContentLength);
            }
        }
    }
}
