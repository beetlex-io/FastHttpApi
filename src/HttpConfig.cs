using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpConfig
    {
        public HttpConfig()
        {
            Port = 9090;
            SSL = false;
            MaxBodyLength = 1024 * 1024 * 2;
            BodySerializer = new StringSerializer();
            Encoding = Encoding.UTF8;
            OutputStackTrace = false;
            Filters = new List<FilterAttribute>();
            StaticResurceType = @"jpg=image/jpeg;jpeg=image/jpeg;gif=image/gif;png=image/png;js=application/x-javascript;html=text/html;htm=text/html;css=text/css;txt=text/plain";
            StaticResourcePath = System.IO.Directory.GetCurrentDirectory() +
                System.IO.Path.DirectorySeparatorChar + "views";
            DefaultPage = "index.html;index.htm";
        }

        public string Host { get; set; }

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

        public string StaticResourcePath { get; set; }
    }
}
