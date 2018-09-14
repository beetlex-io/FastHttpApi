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
    }
}
