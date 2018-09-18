using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class HttpToken
    {
        public bool KeepAlive { get; set; }

        public StaticResurce.FileBlock File { get; set; }
    }
}
