using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HeaderTypeFactory
    {
        static HeaderTypeFactory()
        {
            SPACE_BYTES = Encoding.UTF8.GetBytes(" ");
            HEADER_SPLIT = Encoding.UTF8.GetBytes(": ");
            LINE_BYTES = Encoding.UTF8.GetBytes("\r\n");
            NULL_CONTENT_LENGTH_BYTES = Encoding.UTF8.GetBytes("Content-Length: 0\r\n");
            CHUNKED_BYTES = Encoding.UTF8.GetBytes("0\r\n\r\n");
            for (int i = 0; i < HEADERNAME_MAXLENGTH; i++)
            {
                mTypes.Add(new List<HeaderType>());
            }
            Add(HeaderTypeFactory.AGE);
            Add(HeaderTypeFactory.AUTHORIZATION);
            Add(HeaderTypeFactory.WWW_AUTHENTICATE);
            Add(HeaderTypeFactory.ACCEPT);
            Add(HeaderTypeFactory.ACCEPT_ENCODING);
            Add(HeaderTypeFactory.ACCEPT_LANGUAGE);
            Add(HeaderTypeFactory.ACCEPT_CHARSET);
            Add(HeaderTypeFactory.ACCESS_CONTROL_ALLOW_CREDENTIALS);
            Add(HeaderTypeFactory.ACCESS_CONTROL_ALLOW_HEADERS);
            Add(HeaderTypeFactory.ACCESS_CONTROL_ALLOW_ORIGIN);
            Add(HeaderTypeFactory.CACHE_CONTROL);
            Add(HeaderTypeFactory.CLIENT_IPADDRESS);
            Add(HeaderTypeFactory.CONNECTION);
            Add(HeaderTypeFactory.CONTENT_ENCODING);
            Add(HeaderTypeFactory.CONTENT_LENGTH);
            Add(HeaderTypeFactory.CONTENT_TYPE);
            Add(HeaderTypeFactory.COOKIE);
            Add(HeaderTypeFactory.DATE);
            Add(HeaderTypeFactory.HOST);
            Add(HeaderTypeFactory.ETAG);
            Add(HeaderTypeFactory.IF_NONE_MATCH);
            Add(HeaderTypeFactory.LOCATION);
            Add(HeaderTypeFactory.ORIGIN);
            Add(HeaderTypeFactory.REFERER);
            Add(HeaderTypeFactory.SEC_WEBSOCKET_EXTENSIONS);
            Add(HeaderTypeFactory.SEC_WEBSOCKET_KEY);
            Add(HeaderTypeFactory.SEC_WEBSOCKET_VERSION);
            Add(HeaderTypeFactory.SEC_WEBSOCKT_ACCEPT);
            Add(HeaderTypeFactory.SERVER);
            Add(HeaderTypeFactory.SET_COOKIE);
            Add(HeaderTypeFactory.STATUS);
            Add(HeaderTypeFactory.TRANSFER_ENCODING);
            Add(HeaderTypeFactory.UPGRADE);
            Add(HeaderTypeFactory.USER_AGENT);

        }

        public const int HEADERNAME_MAXLENGTH = 32;

        private static List<List<HeaderType>> mTypes = new List<List<HeaderType>>();

        private static void Add(String name)
        {
            if (Find(name) == null)
            {
                HeaderType type = new HeaderType(name);
                mTypes[type.Index].Add(type);
            }
        }

        public static HeaderType Find(string name)
        {
            HeaderType type;
            List<HeaderType> headers = mTypes[name.Length % HEADERNAME_MAXLENGTH];
            for (int i = 0; i < headers.Count; i++)
            {
                type = headers[i];
                if (type.Compare(name))
                    return type;

            }
            if (headers.Count < 20)
            {
                type = new HeaderType(name);
                headers.Add(type);
                return type;
            }
            return null;

        }

        public static void Write(string name, PipeStream stream)
        {
            HeaderType type = Find(name);
            if (type != null)
            {
                stream.Write(type.Bytes);
            }
            else
            {
                stream.Write(name + ": ");
            }
        }

        public static byte[] NULL_CONTENT_LENGTH_BYTES;

        public static byte[] CHUNKED_BYTES;

        public static byte[] LINE_BYTES;

        public static byte[] SPACE_BYTES;

        public static byte[] HEADER_SPLIT;


        public const string AUTHORIZATION = "Authorization";

        public const string WWW_AUTHENTICATE = "WWW-Authenticate";

        public const string ORIGIN = "Origin";

        public const string DATE = "Date";

        public const string AGE = "Age";

        public const string LOCATION = "Location";

        public const string SEC_WEBSOCKT_ACCEPT = "Sec-WebSocket-Accept";

        public const string CLIENT_IPADDRESS = "X-Real-IP";

        public const string SEC_WEBSOCKET_VERSION = "Sec_WebSocket_Version";

        public const string SEC_WEBSOCKET_EXTENSIONS = "Sec-WebSocket-Extensions";

        public const string SEC_WEBSOCKET_KEY = "Sec-WebSocket-Key";

        public const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";

        public const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";

        public const string ACCESS_CONTROL_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";

        public const string UPGRADE = "Upgrade";

        public const string ACCEPT = "Accept";

        public const string ACCEPT_ENCODING = "Accept-Encoding";

        public const string ACCEPT_LANGUAGE = "Accept-Language";

        public const string ACCEPT_CHARSET = "Accept-Charset";

        public const string CACHE_CONTROL = "Cache-Control";

        public const string CONNECTION = "Connection";

        public const string COOKIE = "Cookie";

        public const string HOST = "Host";

        public const string REFERER = "Referer";

        public const string USER_AGENT = "User-Agent";

        public const string STATUS = "Status";

        public const string CONTENT_TYPE = "Content-Type";

        public const string ETAG = "ETag";

        public const string CONTENT_LENGTH = "Content-Length";

        public const string CONTENT_ENCODING = "Content-Encoding";

        public const string TRANSFER_ENCODING = "Transfer-Encoding";

        public const string IF_NONE_MATCH = "If-None-Match";

        public const string SERVER = "Server";

        public const string SET_COOKIE = "Set-Cookie";

    }

    public class Header
    {

        private List<HeaderValue> mValues = new List<HeaderValue>(8);

        public void Add(string name, string value)
        {
            if (value == null)
                value = string.Empty;
            Find(name).Value = value;
        }

        private HeaderValue Find(string name)
        {
            HeaderValue result;
            for (int i = 0; i < mValues.Count; i++)
            {
                result = mValues[i];
                if (result.Type.Compare(name))
                    return result;
            }
            HeaderType type = HeaderTypeFactory.Find(name);
            if (type == null)
                type = new HeaderType(name);
            result = new HeaderValue(type, null);
            mValues.Add(result);
            return result;
        }

        private HeaderValue FindOnly(string name)
        {
            HeaderValue result;
            for (int i = 0; i < mValues.Count; i++)
            {
                result = mValues[i];
                if (result.Type.Compare(name))
                    return result;
            }
            return null;
        }

        public string this[string name]
        {
            get
            {
                HeaderValue headerValue = FindOnly(name);
                if (headerValue != null)
                    return headerValue.Value;
                else
                    return null;
            }
            set
            {
                Find(name).Value = value;
            }
        }

        public bool Read(PipeStream stream, Cookies cookies)
        {
            IndexOfResult index = stream.IndexOf(HeaderTypeFactory.LINE_BYTES);
            while (index.End != null)
            {
                if (index.Length == 2)
                {
                    stream.ReadFree(2);
                    return true;
                }
                else
                {
                    ReadOnlySpan<Char> line = HttpParse.ReadCharLine(index);
                    stream.ReadFree(index.Length);
                    if (line[0] == 'C' && line[5] == 'e' && line[1] == 'o' && line[2] == 'o' && line[3] == 'k' && line[4] == 'i')
                    {
                        HttpParse.AnalyzeCookie(line.Slice(8, line.Length - 8), cookies);
                    }
                    else
                    {

                        Tuple<string, string> result = HttpParse.AnalyzeHeader(line);
                        HeaderType type = HeaderTypeFactory.Find(result.Item1);
                        if (type == null)
                            Add(result.Item1, result.Item2);
                        else
                            Add(type.Name, result.Item2);
                    }
                }
                index = stream.IndexOf(HeaderTypeFactory.LINE_BYTES);
            }
            return false;
        }

        internal void Write(PipeStream stream)
        {
            foreach (var item in mValues)
            {
                stream.Write(item.Type.Bytes);
                stream.Write(item.Value);
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mValues)
            {
                sb.AppendFormat("{0}={1}\r\n", item.Type.Name, item.Value);
            }
            return sb.ToString();
        }
    }

    public class HeaderValue
    {
        public HeaderValue(HeaderType type, string value)
        {
            Type = type;
            Value = value;
        }

        public HeaderType Type { get; set; }

        public string Value { get; set; }
    }


    public class HeaderType
    {
        public HeaderType(string name)
        {
            Name = name;
            UpperData = new char[name.Length];
            LowerData = new char[name.Length];
            for (int i = 0; i < Name.Length; i++)
            {
                UpperData[i] = char.ToUpper(name[i]);
                LowerData[i] = char.ToLower(name[i]);
            }
            Bytes = Encoding.UTF8.GetBytes(name + ": ");
            Index = name.Length % HeaderTypeFactory.HEADERNAME_MAXLENGTH;
        }

        public char[] UpperData { get; set; }

        private char[] LowerData;

        public string Name { get; set; }

        public byte[] Bytes { get; set; }

        public int Index { get; set; }

        public bool Compare(string value)
        {
            if (value.Length != Name.Length)
                return false;
            int length = value.Length;
            int end = length - 1;
            if ((value[0] == UpperData[0] || value[0] == LowerData[0]) &&
               (value[end] == UpperData[end] || value[end] == LowerData[end]))
            {
                for (int i = 1; i < end; i++)
                {
                    if (value[i] == UpperData[i] || value[i] == LowerData[i])
                        continue;
                    else
                        return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}



