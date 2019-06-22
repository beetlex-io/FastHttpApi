using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            BufferSize = 1024 * 8;
            WebSocketMaxRPS = 30;
            LogLevel = EventArgs.LogType.Warring;
            LogToConsole = false;
            NotLoadFolder = @"\Files;\Images";
            FileManager = false;
            CacheFileSize = 500;
            PacketCombined = 0;
            UrlIgnoreCase = true;
            UseIPv6 = true;
            SessionTimeOut = 60 * 60;
            BufferPoolMaxMemory = 500;
            SSLPort = 443;
            StaticResurceCacheTime = 0;
            Settings = new List<Setting>();
            MaxrpsSettings = new List<ActionMaxrps>();
            CacheLogLength = 0;
            IOQueueEnabled = false;
            Statistical = true;
            int threads = (Environment.ProcessorCount / 2);
            if (threads == 0)
                threads = 1;
            IOQueues = Math.Min(threads, 16);
            BufferPoolGroups = Environment.ProcessorCount;
        }


        public bool PrivateBufferPool { get; set; } = false;

        public int BufferPoolSize { get; set; } = 10;

        public int BufferPoolGroups { get; set; }

        public int IOQueues { get; set; }

        public bool SyncAccept { get; set; } = true;

        public bool ManageApiEnabled { get; set; } = true;

        public bool Statistical { get; set; }

        public bool IOQueueEnabled { get; set; }

        public int CacheLogLength { get; set; }

        public List<ActionMaxrps> MaxrpsSettings { get; set; }

        public List<Setting> Settings { get; set; }

        public string AccessKey { get; set; }

        public int StaticResurceCacheTime { get; set; }

        public int BufferPoolMaxMemory { get; set; }

        public int SessionTimeOut { get; set; }

        public bool UseIPv6 { get; set; }

        public bool UrlIgnoreCase { get; set; }

        [JsonIgnore]
        public UrlRoute[] Routes { get; set; }

        public int PacketCombined { get; set; }

        public bool FileManager { get; set; }

        public string FileManagerPath { get; set; }

        public bool LogToConsole { get; set; }

        public string NotLoadFolder { get; set; }

        public string CacheFiles { get; set; }

        public int CacheFileSize { get; set; }

        public LogType LogLevel { get; set; }

        public int WebSocketMaxRPS { get; set; }

        public int BufferSize { get; set; }

        public string NoGzipFiles { get; set; }

        public int MaxConnections { get; set; }

        public string Manager { get; set; }

        public string ManagerPWD { get; set; }

        public bool WriteLog { get; set; }

        public string Host { get; set; }

        public bool Debug { get; set; }

        public int Port { get; set; }

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
