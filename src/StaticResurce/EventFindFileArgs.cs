using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{

    public class EventFindFileArgs : System.EventArgs
    {
        public string File { get; set; }

        public HttpRequest Request { get; internal set; }

        public string Url { get; internal set; }
    }
}
