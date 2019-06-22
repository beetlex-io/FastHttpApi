using BeetleX.Buffers;
using System;
using System.Collections.Generic;

using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HeaderStringHelper
    {
        public const int CACHE_STRING_MAXLENGTH = 512;

        public const int CACHE_MAXSIZE = 1024 * 32;

        public static int Count => mStringCache.Count;

        private static Dictionary<ulong, string> mStringCache = new Dictionary<ulong, string>();

        public unsafe static string GetString(ReadOnlySpan<char> data, bool toLower = false)
        {
            int length = data.Length;
            if (toLower)
            {
                fixed (char* ptr = data)
                {
                    char* each = ptr;
                    for (int i = 0; i < length; i++)
                    {
                        *each = char.ToLower(*each);
                        each++;
                    }
                }
            }
            ulong code = GetHashCode(data);
            if (!mStringCache.TryGetValue(code, out string result))
            {
                result = new string(data);
                lock (mStringCache)
                {
                    if (Count < CACHE_MAXSIZE)
                    {
                        mStringCache[code] = result;
                    }
                }
            }
            return result;

        }

        public static unsafe ulong GetHashCode(ReadOnlySpan<char> data)
        {
            int length = data.Length;
            fixed (char* ptr = data)
            {
                int l = 0;
                int num = 352654597;
                int num2 = num;
                int* ptr2 = (int*)ptr;
                int i = length % 2;
                int count = length / 2;
                for (l = 0; l < count; l++)
                {
                    if (l % 2 == 0)
                    {
                        num = ((num << 5) + num + (num >> 27) ^ *ptr2);
                    }
                    else
                    {
                        num2 = ((num2 << 5) + num2 + (num2 >> 27) ^ ptr2[1]);
                    }
                    ptr2 += 1;
                }
                if (i > 0)
                {
                    num = ((num << 5) + num + (num >> 27) ^ (int)data[length - 1]);
                }
                uint tag = (uint)data[0] << 8 | (uint)data[length - 1];
                var result = num + num2 * 1566083941;
                return (ulong)result << 28 | (ulong)length << 16 | tag;
            }
        }
    }

    class StringBuffer
    {

        public StringBuffer()
        {
            Bytes = new byte[1024 * 4];
            Chars = new Char[1024 * 4];
        }

        public byte[] Bytes;

        public Char[] Chars;
    }

    public static class PipeStreamExtend
    {
        [ThreadStatic]
        private static StringBuffer stringBuffer;

        public static bool ReadLine(this PipeStream stream, out Span<char> line)
        {
            //line = default;
            //if (stringBuffer == null)
            //    stringBuffer = new StringBuffer();
            //int count = stream.IndexOf(HeaderTypeFactory.LINE_BYTES, stringBuffer.Bytes);
            //if (count > 0)
            //{
            //    stream.ReadFree(count);
            //    var len = Encoding.ASCII.GetChars(stringBuffer.Bytes, 0, count - 2, stringBuffer.Chars, 0);
            //    line = new Span<char>(stringBuffer.Chars, 0, len);
            //    return true;
            //}
            //return false;
            line = default;
            if (stringBuffer == null)
                stringBuffer = new StringBuffer();
            var indexof = stream.IndexOf(HeaderTypeFactory.LINE_BYTES);
            if (indexof.End != null)
            {
                stream.Read(stringBuffer.Bytes, 0, indexof.Length);
                var len = Encoding.ASCII.GetChars(stringBuffer.Bytes, 0, indexof.Length - 2, stringBuffer.Chars, 0);
                line = new Span<char>(stringBuffer.Chars, 0, len);
                return true;
            }
            return false;
        }
    }
}
