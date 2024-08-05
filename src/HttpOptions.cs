﻿using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Authentication;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpOptions : ICloneable
    {
        public HttpOptions()
        {
            Port = 9090;
            SSL = false;
            MaxBodyLength = 1024 * 1024 * 4;
            Encoding = Encoding.UTF8;
            OutputStackTrace = false;
            Filters = new List<FilterAttribute>();
            StaticResurceType = @"jpg;jpeg;gif;png;js;html;htm;css;txt;ico;xml";
            StaticResourcePath = System.IO.Directory.GetCurrentDirectory() +
                System.IO.Path.DirectorySeparatorChar + "views";
            DefaultPage = "index.html;index.htm";
            Debug = false;
            WriteLog = false;
            MaxConnections = 2000;
            NoGzipFiles = "jpg;jpeg;png;gif;png;ico;zip;rar";
            CacheFiles = "html;htm;js;css";
            BufferSize = 1024 * 4;
            WebSocketSessionMaxRps = 0;
            LogLevel = EventArgs.LogType.Warring;
            LogToConsole = false;
            NotLoadFolder = @"\Files;\Images";
            FileManager = false;
            CacheFileSize = 500;
            PacketCombined = 0;
            // UrlIgnoreCase = true;
            UseIPv6 = true;
            SessionTimeOut = 60 * 10;
            BufferPoolMaxMemory = 500;
            SSLPort = 443;
            StaticResurceCacheTime = 0;
            Settings = new List<Setting>();
            MaxrpsSettings = new List<ActionMaxrps>();
            CacheLogMaxSize = 0;
            IOQueueEnabled = false;
            Statistical = true;
            int threads = (Environment.ProcessorCount / 2);
            if (threads == 0)
                threads = 1;
            IOQueues = Math.Min(threads, 16);
            BufferPoolGroups = 4;


        }
        [JsonIgnore]
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();

        public string SockFile { get; set; }

        [Conditional("DEBUG")]
        public void SetDebug(string viewpath = null)
        {
            Debug = true;
            if (string.IsNullOrEmpty(viewpath))
            {
                string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
                path += @"views";
                StaticResourcePath = path;
            }
            else
            {
                StaticResourcePath = viewpath;
            }
        }

        //当IP被限制请求后，是否禁止IP连接
        public bool DisableIPAccept { get; set; } = false;

        public string BindDomains { get; set; }

        public string InvalidDomainUrl { get; set; }

        public bool DisableXRealIP { get; set; } = false;

        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls11 | SslProtocols.Tls12;

        public string ActionExt { get; set; }

        public int MaxRps { get; set; } = 0;

        public int SessionMaxRps { get; set; } = 0;

        public SameSiteType? SameSite { get; set; }

        public bool CookieSecure { get; set; }

        public string ServerTag { get; set; } = "beetlex.io";

        public bool OutputServerTag { get; set; } = true;

        public int IPRpsLimit { get; set; } = 0;

        public int IPRpsLimitDisableTime { get; set; } =0;

        public int MaxWaitQueue { get; set; } = 50;

        public int BufferPoolSize { get; set; } = 10;

        public int BufferPoolGroups { get; set; }

        public int IOQueues { get; set; }

        public bool SyncAccept { get; set; } = false;

        public bool ManageApiEnabled { get; set; } = true;

        public bool Statistical { get; set; }

        public bool IOQueueEnabled { get; set; }

        public int CacheLogMaxSize { get; set; }

        public string CacheLogFilter { get; set; }

        public IDataFrameSerializer WebSocketFrameSerializer { get; set; }

        public List<ActionMaxrps> MaxrpsSettings { get; set; }

        public List<Setting> Settings { get; set; }

        public string AccessKey { get; set; }

        public bool AutoGzip { get; set; } = false;

        public int StaticResurceCacheTime { get; set; }

        public int BufferPoolMaxMemory { get; set; }

        public int SessionTimeOut { get; set; }

        public bool UseIPv6 { get; set; }

        public List<VirtualFolder> Virtuals { get; set; } = new List<VirtualFolder>();

        // public bool UrlIgnoreCase { get; set; }

        [JsonIgnore]
        public OptionsAttribute CrossDomain { get; set; }

        //[JsonIgnore]
        //public UrlRoute[] Routes { get; set; }

        public int PacketCombined { get; set; }

        public bool FileManager { get; set; }

        public string FileManagerPath { get; set; }

        public bool LogToConsole { get; set; }



        public string NotLoadFolder { get; set; }

        public string CacheFiles { get; set; }

        public int CacheFileSize { get; set; }

        public LogType LogLevel { get; set; }

        public int WebSocketSessionMaxRps { get; set; }

        public int BufferSize { get; set; }

        public string NoGzipFiles { get; set; }

        public int MaxConnections { get; set; }

        public string Manager { get; set; }

        public string ManagerPWD { get; set; }

        public bool WriteLog { get; set; }

        public string Host { get; set; }

        public bool Debug { get; set; }

        public bool FixedConverter { get; set; } = false;

        public bool AgentRewrite { get; set; } = false;

        public bool RewriteIgnoreCase { get; set; } = true;

        public int RewriteCachedSize { get; set; } = 500000;

        public int Port { get; set; }

        public bool SSLOnly { get; set; } = false;

        public bool SSL { get; set; }

        public int SSLPort { get; set; }

        public string CertificateFile { get; set; }

        public string CertificatePassword { get; set; }

        public int MaxBodyLength { get; set; }

        [JsonIgnore]
        public System.Text.Encoding Encoding { get; set; }

        public bool OutputStackTrace { get; set; }

        public string StaticResurceType { get; set; }

        [JsonIgnore]
        public IList<FilterAttribute> Filters { get; set; }

        public string DefaultPage { get; set; }

        public void AddFilter<T>() where T : FilterAttribute, new()
        {
            Filters.Add(new T());
        }

        public void AddFilter(FilterAttribute filter)
        {
            Filters.Add(filter);
        }

        public object Clone()
        {
            HttpOptions config = new HttpOptions();
            config.CertificateFile = this.CertificateFile;
            config.CertificatePassword = this.CertificatePassword;
            config.Debug = this.Debug;
            config.DefaultPage = this.DefaultPage;
            config.Encoding = this.Encoding;
            config.Filters = this.Filters;
            config.Host = this.Host;
            config.MaxBodyLength = this.MaxBodyLength;
            config.OutputStackTrace = this.OutputStackTrace;
            config.Port = this.Port;
            config.SSL = this.SSL;
            config.SSLPort = this.SSLPort;
            config.StaticResourcePath = this.StaticResourcePath;
            config.StaticResurceType = this.StaticResurceType;
            return config;
        }

        public string StaticResourcePath { get; set; }

        public string GetValue(string name)
        {
            foreach (var item in Settings)
                if (item.Name == name)
                    return item.Value;
            return null;
        }

        public int GetActionMaxrps(string url)
        {
            foreach (var item in MaxrpsSettings)
            {
                if (item.Url == url)
                    return item.MaxRps;
            }
            return 0;
        }
    }

    public class ActionMaxrps
    {
        public string Url { get; set; }

        public int MaxRps { get; set; }
    }

    public class Setting
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
