﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Diagnostics;
using PropertyChanged;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HTMLToDocX.Services
{
    //[AddINotifyPropertyChangedInterface]
    public partial class AppConfigData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //当前客户端ID
        public string ClientId { get; set; } = string.Empty;

        public string LastPath { get; set; } = string.Empty;

        public bool Overwrite { get; set; }

    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        private static readonly Lazy<AppConfig> lazy = new Lazy<AppConfig>(() => new AppConfig());
        
        public static AppConfig Instance => lazy.Value;

        public AppConfigData ConfigData { set; get; } = new AppConfigData();

        private string _configPath = AppContext.BaseDirectory + @"Config.json";

        private object _lock = new object();

        private AppConfig()
        {
            //Init();
            Debug.WriteLine(_configPath);
        }

        public void InitDefault() //载入默认配置
        {
            ConfigData.ClientId = Guid.NewGuid().ToString();
        }


        public bool Init()
        {
            try
            {
                Debug.WriteLine($"初始化配置" + Thread.CurrentThread.ManagedThreadId);
                if (File.Exists(_configPath) == false)
                {
                    Debug.WriteLine($"默认初始化");
                    InitDefault();
                    Save();
                }

                lock (_lock)
                {
                    var fileContent = File.ReadAllText(_configPath);
                    var appData = JsonConvert.DeserializeObject<AppConfigData>(fileContent);
                    ConfigData = appData;
                    ConfigData.PropertyChanged += AppConfigData_PropertyChanged;
                }

                if (string.IsNullOrWhiteSpace(ConfigData.ClientId))
                {
                    ConfigData.ClientId = Guid.NewGuid().ToString();
                    Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                InitDefault();
                Save();
                Debug.WriteLine(ex);
                return false;
            }
        }
        private void AppConfigData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Save()
        {
            try
            {
                lock (_lock)
                {
                    var data = JsonConvert.SerializeObject(ConfigData);
                    Debug.WriteLine($"存储配置{Thread.CurrentThread.ManagedThreadId} {data}");
                    File.WriteAllText(_configPath, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


    }
}
