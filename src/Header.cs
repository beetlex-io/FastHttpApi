using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HeaderType
    {
        static HeaderType()
        {
            SPACE_BYTES = Encoding.UTF8.GetBytes(" ");
            HEADER_SPLIT = Encoding.UTF8.GetBytes(": ");
            LINE_BYTES = Encoding.UTF8.GetBytes("\r\n");
            NULL_CONTENT_LENGTH_BYTES = Encoding.UTF8.GetBytes("Content-Length: 0\r\n");
            CHUNKED_BYTES = Encoding.UTF8.GetBytes("0\r\n\r\n");
            ACCEPT_BYTES = Encoding.UTF8.GetBytes(ACCEPT + ": ");
            ACCEPT_ENCODING_BYTES = Encoding.UTF8.GetBytes(ACCEPT_ENCODING + ": ");
            ACCEPT_LANGUAGE_BYTES = Encoding.UTF8.GetBytes(ACCEPT_LANGUAGE + ": ");
            CACHE_CONTROL_BYTES = Encoding.UTF8.GetBytes(CACHE_CONTROL + ": ");
            CONNECTION_BYTES = Encoding.UTF8.GetBytes(CONNECTION + ": ");
            COOKIE_BYTES = Encoding.UTF8.GetBytes(COOKIE + ": ");
            REFERER_BYTES = Encoding.UTF8.GetBytes(REFERER + ": ");
            USER_AGENT_BYTES = Encoding.UTF8.GetBytes(USER_AGENT + ": ");
            STATUS_BYTES = Encoding.UTF8.GetBytes(STATUS + ": ");
            CONTENT_TYPE_BYTES = Encoding.UTF8.GetBytes(CONTENT_TYPE + ": ");
            ETAG_BYTES = Encoding.UTF8.GetBytes(ETAG + ": ");
            CONTENT_LENGTH_BYTES = Encoding.UTF8.GetBytes(CONTENT_LENGTH + ": ");
            CONTENT_ENCODING_BYTES = Encoding.UTF8.GetBytes(CONTENT_ENCODING + ": ");
            TRANSFER_ENCODING_BYTES = Encoding.UTF8.GetBytes(TRANSFER_ENCODING + ": ");
            IF_NONE_MATCH_BYTES = Encoding.UTF8.GetBytes(IF_NONE_MATCH + ": ");
            SERVER_BYTES = Encoding.UTF8.GetBytes(SERVER + ": ");
            SET_COOKIE_BYTES = Encoding.UTF8.GetBytes(SET_COOKIE + ": ");
            UPGRADE_BYTES = Encoding.UTF8.GetBytes(UPGRADE + ": ");
            ACCESS_CONTROL_ALLOW_CREDENTIALS_BYTES = Encoding.UTF8.GetBytes(ACCESS_CONTROL_ALLOW_CREDENTIALS + ": ");
            ACCESS_CONTROL_ALLOW_HEADERS_BYTES = Encoding.UTF8.GetBytes(ACCESS_CONTROL_ALLOW_HEADERS + ": ");
            ACCESS_CONTROL_ALLOW_ORIGIN_BYTES = Encoding.UTF8.GetBytes(ACCESS_CONTROL_ALLOW_ORIGIN + ": ");
            DATE_BYTES = Encoding.UTF8.GetBytes(DATE + ": ");
            ORIGIN_BYTES = Encoding.UTF8.GetBytes(ORIGIN + ": ");
            SEC_WEBSOCKET_EXTENSIONS_BYTES = Encoding.UTF8.GetBytes(SEC_WEBSOCKET_EXTENSIONS + ": ");
            SEC_WEBSOCKET_KEY_BYTES = Encoding.UTF8.GetBytes(SEC_WEBSOCKET_KEY + ": ");
            SEC_WEBSOCKET_VERSION_BYTES = Encoding.UTF8.GetBytes(SEC_WEBSOCKET_VERSION + ": ");
            SEC_WEBSOCKT_ACCEPT_BYTES = Encoding.UTF8.GetBytes(SEC_WEBSOCKT_ACCEPT + ": ");
        }

        public static void Write(string name, PipeStream stream)
        {
            if (name[0] == 'A')
            {
                switch (name)
                {
                    case ACCEPT:
                        stream.Write(ACCEPT_BYTES, 0, ACCEPT_BYTES.Length);
                        break;
                    case ACCEPT_ENCODING:
                        stream.Write(ACCEPT_ENCODING_BYTES, 0, ACCEPT_ENCODING_BYTES.Length);
                        break;
                    case ACCEPT_LANGUAGE:
                        stream.Write(ACCEPT_LANGUAGE_BYTES, 0, ACCEPT_LANGUAGE_BYTES.Length);
                        break;
                    case ACCESS_CONTROL_ALLOW_CREDENTIALS:
                        stream.Write(ACCESS_CONTROL_ALLOW_CREDENTIALS_BYTES, 0, ACCESS_CONTROL_ALLOW_CREDENTIALS_BYTES.Length);
                        break;
                    case ACCESS_CONTROL_ALLOW_HEADERS:
                        stream.Write(ACCESS_CONTROL_ALLOW_HEADERS_BYTES, 0, ACCESS_CONTROL_ALLOW_HEADERS_BYTES.Length);
                        break;
                    case ACCESS_CONTROL_ALLOW_ORIGIN:
                        stream.Write(ACCESS_CONTROL_ALLOW_ORIGIN_BYTES, 0, ACCESS_CONTROL_ALLOW_ORIGIN_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'C')
            {
                switch (name)
                {
                    case CACHE_CONTROL:
                        stream.Write(CACHE_CONTROL_BYTES, 0, CACHE_CONTROL_BYTES.Length);
                        break;
                    case CONNECTION:
                        stream.Write(CONNECTION_BYTES, 0, CONNECTION_BYTES.Length);
                        break;
                    case COOKIE:
                        stream.Write(COOKIE_BYTES, 0, COOKIE_BYTES.Length);
                        break;
                    case CONTENT_LENGTH:
                        stream.Write(CONTENT_LENGTH_BYTES, 0, CONTENT_LENGTH_BYTES.Length);
                        break;
                    case CONTENT_ENCODING:
                        stream.Write(CONTENT_ENCODING_BYTES, 0, CONTENT_ENCODING_BYTES.Length);
                        break;
                    case CONTENT_TYPE:
                        stream.Write(CONTENT_TYPE_BYTES, 0, CONTENT_TYPE_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'D')
            {
                switch (name)
                {
                    case DATE:
                        stream.Write(DATE_BYTES, 0, DATE_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'E')
            {
                switch (name)
                {
                    case ETAG:
                        stream.Write(ETAG_BYTES, 0, ETAG_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'I')
            {
                switch (name)
                {
                    case IF_NONE_MATCH:
                        stream.Write(IF_NONE_MATCH_BYTES, 0, IF_NONE_MATCH_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'O')
            {
                switch (name)
                {
                    case ORIGIN:
                        stream.Write(ORIGIN_BYTES, 0, ORIGIN_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'R')
            {
                switch (name)
                {
                    case REFERER:
                        stream.Write(REFERER_BYTES, 0, REFERER_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'S')
            {
                switch (name)
                {
                    case STATUS:
                        stream.Write(STATUS_BYTES, 0, STATUS_BYTES.Length);
                        break;
                    case SERVER:
                        stream.Write(SERVER_BYTES, 0, SERVER_BYTES.Length);
                        break;
                    case SET_COOKIE:
                        stream.Write(SET_COOKIE_BYTES, 0, SET_COOKIE_BYTES.Length);
                        break;
                    case SEC_WEBSOCKET_EXTENSIONS:
                        stream.Write(SEC_WEBSOCKET_EXTENSIONS_BYTES, 0, SEC_WEBSOCKET_EXTENSIONS_BYTES.Length);
                        break;
                    case SEC_WEBSOCKET_KEY:
                        stream.Write(SEC_WEBSOCKET_KEY_BYTES, 0, SEC_WEBSOCKET_KEY_BYTES.Length);
                        break;
                    case SEC_WEBSOCKET_VERSION:
                        stream.Write(SEC_WEBSOCKET_VERSION_BYTES, 0, SEC_WEBSOCKET_VERSION_BYTES.Length);
                        break;
                    case SEC_WEBSOCKT_ACCEPT:
                        stream.Write(SEC_WEBSOCKT_ACCEPT_BYTES, 0, SEC_WEBSOCKT_ACCEPT_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'T')
            {
                switch (name)
                {
                    case TRANSFER_ENCODING:
                        stream.Write(TRANSFER_ENCODING_BYTES, 0, TRANSFER_ENCODING_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else if (name[0] == 'U')
            {
                switch (name)
                {
                    case UPGRADE:
                        stream.Write(UPGRADE_BYTES, 0, UPGRADE_BYTES.Length);
                        break;
                    case USER_AGENT:
                        stream.Write(USER_AGENT_BYTES, 0, USER_AGENT_BYTES.Length);
                        break;
                    default:
                        stream.Write(name + ": ");
                        break;
                }
            }
            else
            {
                stream.Write(name + ": ");
            }

        }

        public const string ORIGIN = "Origin";

        public static Byte[] ORIGIN_BYTES;

        public const string DATE = "DATE";

        public static Byte[] DATE_BYTES;

        public const string SEC_WEBSOCKT_ACCEPT = "Sec-WebSocket-Accept";

        public static byte[] SEC_WEBSOCKT_ACCEPT_BYTES;

        public const string SEC_WEBSOCKET_VERSION = "Sec_WebSocket_Version";

        public static byte[] SEC_WEBSOCKET_VERSION_BYTES;

        public const string SEC_WEBSOCKET_EXTENSIONS = "Sec-WebSocket-Extensions";

        public static Byte[] SEC_WEBSOCKET_EXTENSIONS_BYTES;

        public const string SEC_WEBSOCKET_KEY = "Sec-WebSocket-Key";

        public static Byte[] SEC_WEBSOCKET_KEY_BYTES;

        public const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";

        public static Byte[] ACCESS_CONTROL_ALLOW_ORIGIN_BYTES;

        public const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";

        public static byte[] ACCESS_CONTROL_ALLOW_HEADERS_BYTES;

        public const string ACCESS_CONTROL_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";

        public static byte[] ACCESS_CONTROL_ALLOW_CREDENTIALS_BYTES;

        public const string UPGRADE = "Upgrade";

        public static Byte[] UPGRADE_BYTES;

        public static byte[] NULL_CONTENT_LENGTH_BYTES;

        public static byte[] CHUNKED_BYTES;

        public static byte[] LINE_BYTES;

        public static byte[] SPACE_BYTES;

        public static byte[] HEADER_SPLIT;

        public const string ACCEPT = "Accept";

        public static byte[] ACCEPT_BYTES;

        public const string ACCEPT_ENCODING = "Accept-Encoding";

        public static byte[] ACCEPT_ENCODING_BYTES;

        public const string ACCEPT_LANGUAGE = "Accept-Language";

        public static byte[] ACCEPT_LANGUAGE_BYTES;

        public const string CACHE_CONTROL = "Cache-Control";

        public static byte[] CACHE_CONTROL_BYTES;

        public const string CONNECTION = "Connection";

        public static byte[] CONNECTION_BYTES;

        public const string COOKIE = "Cookie";

        public static byte[] COOKIE_BYTES;

        public const string HOST = "Host";

        public static byte[] HOST_BYTE;

        public const string REFERER = "Referer";

        public static byte[] REFERER_BYTES;

        public const string USER_AGENT = "User-Agent";

        public static byte[] USER_AGENT_BYTES;

        public const string STATUS = "Status";

        public static byte[] STATUS_BYTES;

        public const string CONTENT_TYPE = "Content-Type";

        public static Byte[] CONTENT_TYPE_BYTES;

        public const string ETAG = "ETag";

        public static byte[] ETAG_BYTES;

        public const string CONTENT_LENGTH = "Content-Length";

        public static byte[] CONTENT_LENGTH_BYTES;

        public const string CONTENT_ENCODING = "Content-Encoding";

        public static byte[] CONTENT_ENCODING_BYTES;

        public const string TRANSFER_ENCODING = "Transfer-Encoding";

        public static byte[] TRANSFER_ENCODING_BYTES;

        public const string IF_NONE_MATCH = "If-None-Match";

        public static byte[] IF_NONE_MATCH_BYTES;

        public const string SERVER = "Server";

        public static byte[] SERVER_BYTES;

        public const string SET_COOKIE = "Set-Cookie";

        public static byte[] SET_COOKIE_BYTES;
    }

    public class Header
    {

        private Dictionary<string, string> mItems = new Dictionary<string, string>(16);

        public void Add(string name, string value)
        {
            mItems[name] = value;
        }
        public string this[string name]
        {
            get
            {
                string result = null;
                mItems.TryGetValue(name, out result);
                return result;
            }
            set
            {
                mItems[name] = value;
            }
        }

        public bool Read(PipeStream stream, Cookies cookies)
        {
            IndexOfResult index = stream.IndexOf(HeaderType.LINE_BYTES);
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
                        Add(result.Item1, result.Item2);
                    }
                }
                index = stream.IndexOf(HeaderType.LINE_BYTES);
            }
            return false;
        }

        internal void Write(PipeStream stream)
        {
            foreach (var item in mItems)
            {
                HeaderType.Write(item.Key, stream);
                stream.Write(item.Value);
                stream.Write(HeaderType.LINE_BYTES, 0, 2);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mItems)
            {
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            }
            return sb.ToString();
        }
    }


}
