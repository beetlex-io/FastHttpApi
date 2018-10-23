using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class UrlEncodeDataContext : DataContxt
    {
        public UrlEncodeDataContext(string data)
        {
            HttpParse.AsynczeFromUrlEncoded(data, this);
        }
    }
}
