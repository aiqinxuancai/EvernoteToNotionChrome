using CefSharp;
using CefSharp.Wpf;
using EvernoteToNotionChrome.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace EvernoteToNotionChrome
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        App() 
        {

            //Add Custom assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += Current_DispatcherUnhandledException;

            //Any CefSharp references have to be in another method with NonInlining
            // attribute so the assembly rolver has time to do it's thing.
            InitializeCefSharp();

        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            //var settings = new CefSettings();
            var setting = new CefSettings()
            {
                CachePath = Directory.GetCurrentDirectory() + @"\Cache",
            };

            //setting.RemoteDebuggingPort = 8088;
            setting.Locale = "zh-CN";
            //setting.UserAgent = "Mozilla/6.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.241.0 Safari/537.36";

            //代理设置
            setting.CefCommandLineArgs.Add("enable-npapi", "1");

            //硬件加速设置
            setting.CefCommandLineArgs.Add("--enable-media-stream", "1");
            //setting.CefCommandLineArgs.Add("disable-gpu", "0");
            //setting.SetOffScreenRenderingBestPerformanceArgs();
            //setting.CefCommandLineArgs.Add("disable-gpu", "1");
            //setting.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            //setting.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            setting.CefCommandLineArgs.Add("enable-media-stream", "1");

            //Flash设置
            setting.CefCommandLineArgs["enable-system-flash"] = "0";
            //setting.CefCommandLineArgs.Add("enable-system-flash", "0"); //Automatically discovered and load a system-wide installation of Pepper Flash.
            //setting.CefCommandLineArgs.Add("ppapi-flash-path", @".\plugins\pepflashplayer64_23_0_0_162.dll"); //Load a specific pepper flash version (Step 1 of 2)
            //setting.CefCommandLineArgs.Add("ppapi-flash-version", "23.0.0.162"); //Load a specific pepper flash version (Step 2 of 2)



            // Set BrowserSubProcessPath based on app bitness at runtime
            //setting.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
            //                                       Environment.Is64BitProcess ? "x64" : "x86",
            //                                       "CefSharp.BrowserSubprocess.exe");

            if (!Cef.Initialize(setting, performDependencyCheck: false, browserProcessHandler: null))
            {
                throw new Exception("Unable to Initialize Cef");
            }

            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        // Required by CefSharp to load the unmanaged dependencies when running using AnyCPU
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }



        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            EasyLogManager.Logger.Error("Error#1 " + ex?.Message + Environment.NewLine + ex?.InnerException?.ToString());
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            EasyLogManager.Logger.Error("Error#2 " + e?.Exception?.Message + Environment.NewLine + e?.Exception?.InnerException?.ToString());
            e.Handled = true;
        }
    }
}
