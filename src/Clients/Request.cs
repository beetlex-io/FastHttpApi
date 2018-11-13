using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Clients
{
    public class Request
    {
        public const string POST = "POST";

        public const string GET = "GET";

        public const string DELETE = "DELETE";

        public const string PUT = "PUT";

        public Request()
        {
            Method = GET;
            this.HttpProtocol = "HTTP/1.1";
        }

        public IClientBodyFormater Formater { get; set; }

        public Dictionary<string, string> QuestryString { get; set; }

        public Header Header { get; set; }

        public string Url { get; set; }

        public string Method { get; set; }

        public string HttpProtocol { get; set; }

        public Object Body { get; set; }

        public void Execute(PipeStream stream)
        {
            var buffer = HttpParse.GetByteBuffer();
            int offset = 0;
            offset += Encoding.ASCII.GetBytes(Method, 0, Method.Length, buffer, offset);

            buffer[offset] = HeaderTypeFactory._SPACE_BYTE;
            offset++;

            offset += Encoding.ASCII.GetBytes(Url, 0, Url.Length, buffer, offset);

            //stream.Write(Method + " ");
            //stream.Write(Url);
            if (QuestryString != null && QuestryString.Count > 0)
            {
                int i = 0;
                foreach (var item in this.QuestryString)
                {

                    string key = item.Key;
                    string value = item.Value;
                    if (string.IsNullOrEmpty(value))
                        continue;
                    value = System.Net.WebUtility.UrlEncode(value);
                    if (i == 0)
                    {
                        buffer[offset] = HeaderTypeFactory._QMARK;
                        offset++;
                        //stream.Write("?");
                    }
                    else
                    {
                        buffer[offset] = HeaderTypeFactory._AND;
                        offset++;
                        //stream.Write("&");
                    }
                    offset += Encoding.ASCII.GetBytes(key, 0, key.Length, buffer, offset);
                    buffer[offset] = HeaderTypeFactory._EQ;
                    offset++;
                    offset += Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, offset);
                    //stream.Write(item.Key + "=");
                    //stream.Write(System.Net.WebUtility.UrlEncode(item.Value));
                    i++;
                }
            }
            buffer[offset] = HeaderTypeFactory._SPACE_BYTE;
            offset++;

            // stream.Write(HeaderTypeFactory.SPACE_BYTES, 0, 1);
            //stream.Write(this.HttpProtocol);
            //stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            offset += Encoding.ASCII.GetBytes(HttpProtocol, 0, HttpProtocol.Length, buffer, offset);

            buffer[offset] = HeaderTypeFactory._LINE_R;
            offset++;
            buffer[offset] = HeaderTypeFactory._LINE_N;
            offset++;
            stream.Write(buffer, 0, offset);
            if (Header != null)
                Header.Write(stream);
            if (Method == POST || Method == PUT)
            {
                if (Body != null)
                {
                    stream.Write(HeaderTypeFactory.CONTENT_LENGTH_BYTES, 0, 16);
                    MemoryBlockCollection contentLength = stream.Allocate(10);
                    stream.Write(HeaderTypeFactory.TOW_LINE_BYTES, 0, 4);
                    int len = stream.CacheLength;
                    Formater.Serialization(Body, stream);
                    int count = stream.CacheLength - len;
                    contentLength.Full(count.ToString().PadRight(10), stream.Encoding);
                }
                else
                {
                    stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES.Length);
                    stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
                }
            }
            else
            {
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }
        }
    }
}
