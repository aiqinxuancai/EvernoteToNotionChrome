global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
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

using Wpf.Ui;

namespace EvernoteToNotionChrome
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(AppContext.BaseDirectory);
        })
        .ConfigureServices(
            (context, services) =>
            {
                // App Host
                //services.AddHostedService<ApplicationHostService>();

                // Main window container with navigation
                // services.AddSingleton<IWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                //services.AddSingleton<WindowsProviderService>();
            }
        )
        .Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T? GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T ?? null;
        }


        App() 
        {
            AppConfig.Instance.Init();
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
            setting.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0";

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
