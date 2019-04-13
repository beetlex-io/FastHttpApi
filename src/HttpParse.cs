using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpParse
    {
        public const string GET_TAG = "GET";

        public const string POST_TAG = "POST";

        public const string DELETE_TAG = "DELETE";

        public const string PUT_TAG = "PUT";

        public const string OPTIONS_TAG = "OPTIONS";

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
        [ThreadStatic]
        private static char[] mToLowerBuffer;
        public static char[] GetToLowerBuffer()
        {
            if (mToLowerBuffer == null)
            {
                mToLowerBuffer = new char[1024];
            }
            return mToLowerBuffer;
        }

        public static ReadOnlySpan<char> ReadCharLine(IndexOfResult result)
        {
            int offset = 0;
            char[] data = HttpParse.GetCharBuffer();
            IMemoryBlock memory = result.Start;
            for (int i = result.StartPostion; i < memory.Length; i++)
            {
                data[offset] = (char)result.Start.Data[i];
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
                    count = memory.Length;
                }
                for (int i = 0; i < count; i++)
                {
                    data[offset] = (char)memory.Data[i];
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
            char[] buffer = GetToLowerBuffer();
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

        public static string GetBaseUrlToLower(ReadOnlySpan<char> url)
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

        public static string MD5Encrypt(string filename)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(filename));

                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
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

        public static void AsynczeFromUrlEncoded(ReadOnlySpan<char> url, Data.IDataContext context)
        {
            int offset = 0;
            string name = null, value = null;
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '=')
                {
                    name = new string(url.Slice(offset, i - offset));
                    offset = i + 1;
                }
                if (name != null && url[i] == '&')
                {
                    if (i > offset)
                    {
                        value = new string(url.Slice(offset, i - offset));
                        context.SetValue(name, System.Net.WebUtility.UrlDecode(value));
                        offset = i + 1;
                    }
                    name = null;

                }
            }
            if (name != null)
            {
                if (url.Length > offset)
                {
                    value = new string(url.Slice(offset, url.Length - offset));
                    context.SetValue(name, System.Net.WebUtility.UrlDecode(value));
                }
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

        private static ContentHeaderProperty[] GetProperties(ReadOnlySpan<char> line)
        {
            List<ContentHeaderProperty> proerties = new List<ContentHeaderProperty>();
            int offset = 0;
            string name = null;
            string value;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    offset++;
                    continue;
                }
                if (line[i] == '=')
                {
                    name = new string(line.Slice(offset, i - offset));
                    offset = i + 1;
                }
                else if (line[i] == ';')
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        value = new string(line.Slice(offset + 1, i - offset - 2));
                        proerties.Add(new ContentHeaderProperty() { Name = name, Value = value });
                        offset = i + 1;
                        name = null;
                    }
                }
            }
            if (name != null)
            {
                value = new string(line.Slice(offset + 1, line.Length - offset - 2));
                proerties.Add(new ContentHeaderProperty() { Name = name, Value = value });
            }
            return proerties.ToArray();
        }

        public static ContentHeader AnalyzeContentHeader(ReadOnlySpan<char> line)
        {
            ContentHeader result = new ContentHeader();
            ReadOnlySpan<char> property = line;
            int offset = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ':')
                {
                    result.Name = new string(line.Slice(0, i));
                    offset = i + 1;
                }
                else if (offset > 0 && line[i] == ' ')
                    offset = i + 1;
                else if (line[i] == ';')
                {
                    result.Value = new string(line.Slice(offset, i - offset));
                    property = line.Slice(i + 1);
                    offset = 0;
                    break;
                }
            }
            if (offset > 0)
            {
                result.Value = new string(line.Slice(offset, line.Length - offset));
            }
            if (property.Length != line.Length)
            {
                result.Properties = GetProperties(property);
            }
            return result;
        }


        public static Tuple<string, string> AnalyzeHeader(ReadOnlySpan<byte> line)
        {
            Span<char> charbuffer = GetCharBuffer();
            var len = Encoding.UTF8.GetChars(line, charbuffer);
            return AnalyzeHeader(charbuffer.Slice(0, len));
        }
        public static Tuple<string, string> AnalyzeHeader(ReadOnlySpan<char> line)
        {
            string name = null, value = null;
            int offset = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ':' && name == null)
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

        public static Tuple<string, int, string> AnalyzeResponseLine(ReadOnlySpan<byte> line)
        {
            Span<char> charbuffer = GetCharBuffer();
            var len = Encoding.UTF8.GetChars(line, charbuffer);
            return AnalyzeResponseLine(charbuffer.Slice(0, len));
        }
        public static Tuple<string, int, string> AnalyzeResponseLine(ReadOnlySpan<char> line)
        {
            string httpversion = null, codemsg = null;
            int code = 200;
            int offset = 0;
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    if (count == 0)
                    {
                        httpversion = new string(line.Slice(offset, i - offset));
                        offset = i + 1;
                    }
                    else
                    {
                        code = int.Parse(line.Slice(offset, i - offset));
                        offset = i + 1;
                        codemsg = new string(line.Slice(offset, line.Length - offset));
                        break;
                    }
                    count++;
                }
            }
            return new Tuple<string, int, string>(httpversion, code, codemsg);
        }

        public static void AnalyzeResponseLine(ReadOnlySpan<char> line, Clients.Response response)
        {
            int offset = 0;
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    if (count == 0)
                    {
                        response.HttpVersion = new string(line.Slice(offset, i - offset));
                        offset = i + 1;
                    }
                    else
                    {
                        response.Code = new string(line.Slice(offset, i - offset));
                        offset = i + 1;
                        response.CodeMsg = new string(line.Slice(offset, line.Length - offset));
                        return;
                    }
                    count++;
                }
            }
        }

        public static void AnalyzeRequestLine(ReadOnlySpan<char> line, HttpRequest request)
        {
            int offset = 0;
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    if (count == 0)
                    {
                        request.Method = new string(line.Slice(offset, i - offset));
                        offset = i + 1;
                    }
                    else
                    {
                        request.Url = new string(line.Slice(offset, i - offset));
                        offset = i + 1;
                        request.HttpVersion = new string(line.Slice(offset, line.Length - offset));
                        return;
                    }
                    count++;
                }
            }

        }

        public static int ReadUrlQueryString(ReadOnlySpan<char> url, QueryString queryString, HttpRequest request)
        {
            int result = 0;
            ReadOnlySpan<char> qsdata = url;
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '?')
                {
                    result = i;
                    qsdata = url.Slice(i + 1, url.Length - i - 1);
                    break;
                }
            }
            if (result > 0)
            {
                string name = null;
                string value = null;
                int offset = 0;
                for (int i = 0; i < qsdata.Length; i++)
                {
                    if (qsdata[i] == '=')
                    {
                        name = new string(qsdata.Slice(offset, i - offset));
                        offset = i + 1;
                    }
                    else if (qsdata[i] == '&')
                    {
                        if (name != null && i - offset > 0)
                        {
                            value = new string(qsdata.Slice(offset, i - offset));
                            queryString.Add(name, value);
                            name = null;
                        }
                        offset = i + 1;
                    }
                }
                if (name != null && qsdata.Length - offset > 0)
                {
                    value = new string(qsdata.Slice(offset, qsdata.Length - offset));
                    queryString.Add(name, value);
                }
            }
            return result;
        }

        public static void ReadUrlPathAndExt(ReadOnlySpan<char> url, QueryString queryString, HttpRequest request, HttpOptions config)
        {
            bool urlIgnoreCase = config.UrlIgnoreCase;

            if (urlIgnoreCase)
                request.BaseUrl = CharToLower(url);
            else
                request.BaseUrl = new string(url);
            for (int i = url.Length - 1; i >= 0; i--)
            {
                if (url[i] == '.' && request.Ext == null)
                {
                    if (urlIgnoreCase)
                        request.Ext = CharToLower(url.Slice(i + 1, url.Length - i - 1));
                    else
                        request.Ext = new string(url.Slice(i + 1, url.Length - i - 1));
                    continue;
                }
                if (url[i] == '/')
                {
                    if (urlIgnoreCase)
                        request.Path = CharToLower(url.Slice(0, i + 1));
                    else
                        request.Path = new string(url.Slice(0, i + 1));
                    return;
                }
            }
        }

        public static void ReadHttpVersionNumber(ReadOnlySpan<char> buffer, QueryString queryString, HttpRequest request)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == '/')
                {
                    request.VersionNumber = new string(buffer.Slice(i + 1, buffer.Length - i - 1));
                }
            }
        }


        public struct AnalyzeHeaderLine
        {
            public short Count;

            public Char[] Buffer;

            public bool EmptyLine;

            public short Offset;

            public string Name;

            public bool Import(ReadOnlySpan<byte> bytes, PipeStream stream, Header header, Cookies cookie)
            {
                Span<char> bufferSpan = new Span<char>(Buffer);
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte b = bytes[i];
                    Char c = (char)b;
                    bufferSpan[Count] = c;
                    Count++;
                    if (bufferSpan[Count - 1] == '\n' && bufferSpan[Count - 2] == '\r')
                    {
                        stream.ReadFree(Count);
                        if (Count == 2)
                        {
                            EmptyLine = true;
                            return true;
                        }
                        string value = new string(bufferSpan.Slice(Offset, Count - Offset - 2));
                        header[Name] = value;
                        if (Name[0] == 'C' && Name[5] == 'e' && Name[1] == 'o' && Name[2] == 'o' && Name[3] == 'k' && Name[4] == 'i')
                        {
                            HttpParse.AnalyzeCookie(value, cookie);
                        }
                        Name = null;
                        Offset = 0;
                        Count = 0;
                        EmptyLine = false;
                        return true;
                    }
                    if (c == ':')
                    {
                        if (Name == null)
                        {
                            Name = new string(bufferSpan.Slice(Offset, Count - Offset - 1));
                            Offset = (short)(Count);
                        }
                    }
                    else
                    {
                        if (Name != null)
                        {
                            if (c == ' ')
                                Offset++;
                        }
                    }
                }
                return false;
            }
        }

        public struct AnalyzeMethodLine
        {
            public short Count;

            public short Spaces;

            public Char[] Buffer;

            public short UrlOffset;

            public short VersionOffset;

            private int ReadQueryString(ReadOnlySpan<char> buffer, QueryString queryString, HttpRequest request)
            {
                int result = 0;
                ReadOnlySpan<char> qsdata = buffer;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == '?')
                    {
                        result = i;
                        qsdata = buffer.Slice(i + 1, buffer.Length - i - 1);
                        break;
                    }
                }
                if (result > 0)
                {
                    string name = null;
                    string value = null;
                    int offset = 0;
                    for (int i = 0; i < qsdata.Length; i++)
                    {
                        if (qsdata[i] == '=')
                        {
                            name = new string(qsdata.Slice(offset, i - offset));
                            offset = i + 1;
                        }
                        else if (qsdata[i] == '&')
                        {
                            if (name != null && i - offset > 0)
                            {
                                value = new string(qsdata.Slice(offset, i - offset));
                                queryString.Add(name, value);
                                name = null;
                            }
                            offset = i + 1;
                        }
                    }
                    if (name != null && qsdata.Length - offset > 0)
                    {
                        value = new string(qsdata.Slice(offset, qsdata.Length - offset));
                        queryString.Add(name, value);
                    }
                }
                return result;
            }

            private void ReadPath(ReadOnlySpan<char> buffer, QueryString queryString, HttpRequest request)
            {
                request.BaseUrl = CharToLower(buffer);
                for (int i = buffer.Length - 1; i >= 0; i--)
                {
                    if (buffer[i] == '.')
                    {
                        request.Ext = CharToLower(buffer.Slice(i + 1, buffer.Length - i - 1));
                        continue;
                    }
                    if (buffer[i] == '/')
                    {
                        request.Path = CharToLower(buffer.Slice(0, i + 1));
                        return;
                    }
                }
            }

            private void ReadHttpVersionNumber(ReadOnlySpan<char> buffer, QueryString queryString, HttpRequest request)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == '/')
                    {
                        request.VersionNumber = new string(buffer.Slice(i + 1, buffer.Length - i - 1));
                    }
                }
            }

            public bool Import(ReadOnlySpan<byte> bytes, PipeStream stream, QueryString queryString, HttpRequest request)
            {
                Span<char> bufferSpan = new Span<char>(Buffer);
                ReadOnlySpan<char> UrlData = Buffer;
                ReadOnlySpan<char> VersionData = Buffer;
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte b = bytes[i];
                    Char c = (char)b;
                    bufferSpan[Count] = c;
                    Count++;
                    if (bufferSpan[Count - 1] == '\n' && bufferSpan[Count - 2] == '\r')
                    {
                        VersionData = new ReadOnlySpan<char>(Buffer, VersionOffset, Count - VersionOffset - 2);
                        request.Url = new string(UrlData);
                        request.HttpVersion = new string(VersionData);
                        ReadHttpVersionNumber(VersionData, queryString, request);
                        int len = ReadQueryString(UrlData, queryString, request);
                        if (len > 0)
                            ReadPath(UrlData.Slice(0, len), queryString, request);
                        else
                            ReadPath(UrlData, queryString, request);
                        stream.ReadFree(Count);
                        return true;
                    }
                    else if (c == ' ')
                    {
                        Spaces++;
                        if (Spaces == 1)
                        {
                            request.Method = new string(Buffer, 0, Count - 1);
                            UrlOffset = Count;
                        }
                        if (Spaces == 2)
                        {
                            VersionOffset = Count;
                            UrlData = new ReadOnlySpan<char>(Buffer, UrlOffset, VersionOffset - UrlOffset - 1);

                        }
                    }

                }
                return false;
            }
        }


        public struct ContentHeader
        {
            public string Name;

            public string Value;

            public ContentHeaderProperty[] Properties { get; set; }

        }

        public struct ContentHeaderProperty
        {
            public string Name;
            public string Value;
        }

    }
}
