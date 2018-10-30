using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;
using static BeetleX.FastHttpApi.HttpParse;

namespace BeetleX.FastHttpApi
{
    public class HeaderTypeFactory
    {

        public static byte[] NULL_CONTENT_LENGTH_BYTES;

        public static byte[] CONTENT_LENGTH_BYTES;

        public static byte[] TOW_LINE_BYTES;

        public static byte[] CHUNKED_BYTES;

        public static byte[] LINE_BYTES;

        public static byte[] SPACE_BYTES;

        public static byte[] HEADER_SPLIT;

        public const byte _LINE_R = 13;

        public const byte _LINE_N = 10;

        public const byte _SPACE_BYTE = 32;

        public const int HEADERNAME_MAXLENGTH = 32;

        public static byte[] SERVAR_HEADER_BYTES;


        #region header
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
        #endregion

        static HeaderTypeFactory()
        {
            SPACE_BYTES = Encoding.UTF8.GetBytes(" ");
            HEADER_SPLIT = Encoding.UTF8.GetBytes(": ");
            LINE_BYTES = Encoding.UTF8.GetBytes("\r\n");
            NULL_CONTENT_LENGTH_BYTES = Encoding.UTF8.GetBytes("Content-Length: 0\r\n");
            CHUNKED_BYTES = Encoding.UTF8.GetBytes("0\r\n\r\n");
            CONTENT_LENGTH_BYTES = Encoding.UTF8.GetBytes("Content-Length: ");
            TOW_LINE_BYTES = Encoding.UTF8.GetBytes("\r\n\r\n");
            SERVAR_HEADER_BYTES = Encoding.UTF8.GetBytes("Server: BeetleX\r\n");
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

        private static System.Collections.Generic.Dictionary<int, HeaderType> mHeaderTypes = new Dictionary<int, HeaderType>();

        private static int mCount;

        private static void Add(String name)
        {
            HeaderType type = new HeaderType(name);
            mHeaderTypes[type.ID] = type;
        }

        private static void Add(string name, HeaderType type)
        {
            if (mCount < 5000)
            {
                lock (mHeaderTypes)
                {
                    int id = name.GetHashCode();
                    mHeaderTypes[id] = type;
                }
                System.Threading.Interlocked.Increment(ref mCount);
            }
        }

        public static HeaderType Find(string name)
        {
            HeaderType type;
            int id = name.GetHashCode();
            if (mHeaderTypes.TryGetValue(id, out type))
                return type;
            foreach (var item in mHeaderTypes.Values)
            {
                if (item.Compare(name))
                {
                    Add(name, item);
                    return item;
                }
            }
            type = new HeaderType(name);
            Add(name, type);
            return type;
        }

        public static void Write(string name, PipeStream stream)
        {
            HeaderType type = Find(name);
            stream.Write(type.Bytes);
        }
    }

    public class Header
    {

        private Dictionary<int, HeaderValue> mValues = new Dictionary<int, HeaderValue>(8);

        public void Add(string name, string value)
        {
            if (value == null)
                value = string.Empty;
            Find(name).Value = value;
        }

        public void Clear()
        {
            mValues.Clear();
        }

        private HeaderValue Find(string name)
        {
            HeaderType type = HeaderTypeFactory.Find(name);
            HeaderValue value;
            if (mValues.TryGetValue(type.ID, out value))
                return value;
            value = new HeaderValue(type, null);
            mValues[type.ID] = value;
            return value;
        }

        private HeaderValue FindOnly(string name)
        {
            HeaderValue result;
            int id = name.GetHashCode();
            mValues.TryGetValue(id, out result);
            return result;
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
            string lineData;
            while (stream.TryReadWith(HeaderTypeFactory.LINE_BYTES, out lineData))
            {
                if (string.IsNullOrEmpty(lineData))
                {
                    return true;
                }
                else
                {
                    ReadOnlySpan<Char> line = lineData;
                    if (line[0] == 'C' && line[5] == 'e' && line[1] == 'o' && line[2] == 'o' && line[3] == 'k' && line[4] == 'i')
                    {
                        HttpParse.AnalyzeCookie(line.Slice(8, line.Length - 8), cookies);
                    }
                    else
                    {
                        Tuple<string, string> result = HttpParse.AnalyzeHeader(line);
                        this[result.Item1] = result.Item2;
                    }
                }
            }
            return false;
        }

        internal void Write(PipeStream stream)
        {
            foreach (var item in mValues.Values)
            {
                item.Write(stream);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mValues.Values)
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

        public void Write(PipeStream stream)
        {
            byte[] buffer = HttpParse.GetByteBuffer();
            int count = Type.Bytes.Length;
            System.Buffer.BlockCopy(Type.Bytes, 0, buffer, 0, count);
            count = count + Encoding.UTF8.GetBytes(Value, 0, Value.Length, buffer, count);
            buffer[count] = HeaderTypeFactory._LINE_R;
            buffer[count + 1] = HeaderTypeFactory._LINE_N;
            stream.Write(buffer, 0, count + 2);
        }
    }


    public class HeaderType
    {
        public HeaderType(string name)
        {
            Name = name;
            Bytes = Encoding.UTF8.GetBytes(name + ": ");
            ID = name.GetHashCode();
        }

        public string Name { get; set; }

        public byte[] Bytes { get; set; }

        public int ID { get; set; }

        public override int GetHashCode()
        {
            return this.ID;
        }

        public bool Compare(string value)
        {
            return string.Compare(Name, value, true) == 0;
        }
    }
}



