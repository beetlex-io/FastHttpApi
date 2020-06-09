using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class EventFileResponseArgs : System.EventArgs
    {
        public HttpRequest Request { get; internal set; }

        public HttpResponse Response { get; internal set; }

        public FileResource Resource { get; set; }

        public FileContentType ContentType { get; set; }

        public bool Cancel { get; set; } = false;

    }
}
