﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using EvernoteToNotionChrome.Utils;

namespace EvernoteToNotionChrome.Service
{
    public class EasyLogManager
    {
        private static string _logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".log";

        public static SimpleLogger Logger { set; get; }

        /// <summary>
        /// 如果日志大于10M则清除
        /// </summary>
        static EasyLogManager ()
        {
            if (File.Exists(_logFilename))
            {
                FileInfo finfo = new FileInfo(_logFilename);
                if (!finfo.Exists)
                {
                    FileStream fs = File.Create(_logFilename);
                    fs.Close();
                }
                else
                {
                    try
                    {
                        if (finfo.Length > 1024 * 1024 * 20)
                        {
                            File.Delete(_logFilename);
                            FileStream fs = File.Create(_logFilename);
                            fs.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            Logger = new SimpleLogger();


        }


    }
}
