using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    class FileContentType
    {
        public FileContentType(string filetype)
        {
            string[] value = filetype.Split('=');
            Ext = value[0].ToLower();
            ContentType = value[1];
        }

        public string Ext { get; set; }

        public string ContentType { get; set; }
    }
}
