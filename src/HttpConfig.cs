using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpConfig : ICloneable
    {
        public HttpConfig()
        {
            Port = 9090;
            SSL = false;
            MaxBodyLength = 1024 * 1024;
            BodySerializer = new JsonBodySerializer();
            Encoding = Encoding.UTF8;
            OutputStackTrace = false;
            Filters = new List<FilterAttribute>();
            StaticResurceType = @"jpg=image/jpeg;jpeg=image/jpeg;gif=image/gif;png=image/png;js=application/x-javascript;html=text/html;htm=text/html;css=text/css;txt=text/plain;ico=application/x-ico";
            StaticResourcePath = System.IO.Directory.GetCurrentDirectory() +
                System.IO.Path.DirectorySeparatorChar + "views";
            DefaultPage = "index.html;index.htm";
            Debug = false;
            Manager = "admin";
            ManagerPWD = "123456";
            WriteLog = false;
            MaxConnections = 2000;
            NoGzipFiles = "jpg;jpeg;png;gif;png;ico;zip;rar";
            CacheFiles = "html;htm;js;css";
            BufferSize = 1024 * 8;
            WebSocketMaxRPS = 30;

            LogLevel = EventArgs.LogType.Warring;
            this.LogToConsole = false;
        }

        public bool LogToConsole { get; set; }

        public string CacheFiles { get; set; }

        public BeetleX.EventArgs.LogType LogLevel { get; set; }

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

        public string CertificateFile { get; set; }

        public string CertificatePassword { get; set; }

        public int MaxBodyLength { get; set; }

        public IBodySerializer BodySerializer { get; set; }

        public System.Text.Encoding Encoding { get; set; }

        public bool OutputStackTrace { get; set; }

        public string StaticResurceType { get; set; }

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
            HttpConfig config = new HttpConfig();
            config.BodySerializer = this.BodySerializer;
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
            config.StaticResourcePath = this.StaticResourcePath;
            config.StaticResurceType = this.StaticResurceType;

            return config;
        }

        public string StaticResourcePath { get; set; }
    }
}
