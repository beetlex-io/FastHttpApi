using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class CommandLineArgs
    {
        [Option("-host", Required = false)]
        public string Host { get; set; }
        [Option("-port", Required = false)]
        public int Port { get; set; }
        [Option("-sslport", Required = false)]
        public int SSLPort { get; set; }

        [Option("-sslfile", Required = false)]
        public string SSLFile { get; set; }

        [Option("-sslpwd", Required = false)]
        public string SSLPassWord { get; set; }

        [Option("-sock", Required = false)]
        public string Sock { get; set; }
    }
}
