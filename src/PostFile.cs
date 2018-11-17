using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class PostFile
    {
        public string FileName { get; internal set; }

        public string ContentType { get; internal set; }

        public System.IO.Stream Data { get; internal set; }
    }
}
