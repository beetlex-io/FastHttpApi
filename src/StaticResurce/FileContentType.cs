using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class FileContentType
    {
        public FileContentType(string filetype)
        {
            Ext = filetype;
            ContentType = ContentTypes.GetContentType(Ext);
        }

        public string Ext { get; set; }

        public string ContentType { get; set; }

        public override string ToString()
        {
            return string.Format(".{0},{1}", Ext, this.ContentType);
        }
    }
}
