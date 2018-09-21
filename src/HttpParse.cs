using BeetleX.Buffers;
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
                mCharCacheBuffer = new char[1024 * 4];
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

        public static ReadOnlySpan<char> ReadCharLine(IndexOfResult result)
        {
            int offset = 0;
            char[] data = HttpParse.GetCharBuffer();
            IMemoryBlock memory = result.Start;
            for (int i = result.StartPostion; i < memory.Bytes.Length; i++)
            {
                data[offset] = (char)result.Start.Bytes[i];
                offset++;
                if (offset == result.Length)
                    break;
            }
            if (offset < result.Length)
            {
                Next:
                memory = result.Start.NextMemory;
                int count;
                if (memory.ID == result.End.ID)
                {
                    count = result.EndPostion + 1;
                }
                else
                {
                    count = memory.Bytes.Length;
                }
                for (int i = 0; i < count; i++)
                {
                    data[offset] = (char)memory.Bytes[i];
                    offset++;
                    if (offset == result.Length)
                        break;
                }
                if (offset < result.Length)
                    goto Next;
            }
            return new ReadOnlySpan<char>(data, 0, result.Length - 2);

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
                    return CharToLower(url.Slice(0, i));
                }
            }
            return CharToLower(url);
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

        public static void AnalyzeCookie(ReadOnlySpan<char> cookieData, Cookies cookies)
        {
            int offset = 0;
            string name = null, value = null;
            for (int i = 0; i < cookieData.Length; i++)
            {
                if (cookieData[i] == '=')
                {
                    if (cookieData[offset] == ' ')
                        offset++;
                    name = new string(cookieData.Slice(offset, i - offset));
                    offset = i + 1;
                }
                if (name != null && cookieData[i] == ';')
                {
                    value = new string(cookieData.Slice(offset, i - offset));
                    offset = i + 1;
                    cookies.Add(name, value);
                    name = null;

                }

            }
            if (name != null)
            {
                value = new string(cookieData.Slice(offset, cookieData.Length - offset));
                cookies.Add(name, value);
            }
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
                    if (i > offset)
                    {
                        value = new string(url.Slice(offset, i - offset));
                        qs.Add(name, value);
                    }
                    name = null;
                    offset = i + 1;
                }
            }
            if (name != null)
            {
                if (url.Length > offset)
                {
                    value = new string(url.Slice(offset, url.Length - offset));
                    qs.Add(name, value);
                }
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
