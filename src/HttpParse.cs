using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpParse
    {
        [ThreadStatic]
        private static char[] mCharCacheBuffer;

        public static char[] GetCharBuffer()
        {
            if (mCharCacheBuffer == null)
                mCharCacheBuffer = new char[1024 * 2];
            return mCharCacheBuffer;
        }

        [ThreadStatic]
        private static byte[] mByteBuffer;
        public static byte[] GetByteBuffer()
        {
            if (mByteBuffer == null)
                mByteBuffer = new byte[1024 * 4];
            return mByteBuffer;
        }

        public static string CharToLower(ReadOnlySpan<char> url)
        {
            char[] buffer = GetCharBuffer();
            for (int i = 0; i < url.Length; i++)
                buffer[i] = Char.ToLower(url[i]);
            return new string(buffer, 0, url.Length);
        }

        public static string GetBaseUrl(ReadOnlySpan<char> url)
        {
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '?')
                {
                    return new string(url.Slice(0, i));
                }
            }
            return new string(url);
        }

        public static string GetBaseUrlExt(ReadOnlySpan<char> url)
        {
            int offset = 0;
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '.')
                {
                    offset = i + 1;
                }
            }
            if (offset > 0)
                return CharToLower(url.Slice(offset, url.Length - offset));
            return null;
        }

        public static void AnalyzeQueryString(ReadOnlySpan<char> url, QueryString qs)
        {
            int offset = 0;
            string name = null, value = null;
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '?')
                {
                    offset = i + 1;
                }
                if (url[i] == '=' && offset > 0)
                {
                    name = new string(url.Slice(offset, i - offset));
                    offset = i + 1;
                }
                if (name != null && url[i] == '&')
                {
                    value = new string(url.Slice(offset, i - offset));
                    offset = i + 1;
                    qs.Add(name, value);
                    name = null;
                }
            }
            if (name != null)
            {
                value = new string(url.Slice(offset, url.Length - offset));
                qs.Add(name, value);
            }

        }

        public static Tuple<string, string> AnalyzeHeader(ReadOnlySpan<char> line)
        {
            string name = null, value = null;
            int offset = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ':')
                {
                    name = new string(line.Slice(offset, i - offset));
                    offset = i + 1;
                }
                else
                {
                    if (name != null)
                    {
                        if (line[i] == ' ')
                            offset++;
                        else
                            break;
                    }
                }
            }
            value = new string(line.Slice(offset, line.Length - offset));
            return new Tuple<string, string>(name, value);
        }

        public static Tuple<string, string, string> AnalyzeRequestLine(ReadOnlySpan<char> line)
        {
            string[] value = new string[3];
            int offset = 0;
            int items = 0;
            int length = line.Length - 1;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    value[items] = new string(line.Slice(offset, i - offset));
                    offset = i + 1;
                    items++;

                }
                if (items == 2)
                    break;
            }
            value[2] = new string(line.Slice(offset, line.Length - offset));
            return new Tuple<string, string, string>(value[0], value[1], value[2]);

        }


    }
}
