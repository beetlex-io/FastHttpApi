using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public enum LoadedState
    {
        None,
        Method,
        Header,
        Completed
    }

    public class HttpRequest
    {
        public HttpRequest(ISession session, IBodySerializer formater)
        {
            Header = new Header();
            this.Session = session;
            mState = LoadedState.None;
            Serializer = formater;
        }

        private LoadedState mState;

        private int mLength;

        private QueryString mQueryString = new QueryString();

        private Cookies mCookies = new Cookies();

        private PipeStream mStream;

        public bool KeepAlive { get; set; }

        public Header Header { get; private set; }

        public Cookies Cookies => mCookies;

        public int Length => mLength;

        public string Method { get; set; }

        public string BaseUrl { get; set; }

        public string Url { get; set; }

        public string HttpVersion { get; set; }

        public string Ext { get; set; }

        public string IfNoneMatch => Header[HeaderType.IF_NONE_MATCH];

        public QueryString QueryString => mQueryString;

        internal ISession Session { get; private set; }

        public IBodySerializer Serializer { get; set; }

        public LoadedState Read(PipeStream stream)
        {
            mStream = stream;
            LoadMethod(stream);
            LoadHeaer(stream);
            LoadBody(stream);
            return mState;
        }



        private void LoadMethod(PipeStream stream)
        {
            if (mState == LoadedState.None)
            {
                IndexOfResult index = stream.IndexOf(HeaderType.LINE_BYTES);
                if (index.End != null)
                {
                    ReadOnlySpan<Char> line = HttpParse.ReadCharLine(index);
                    stream.ReadFree(index.Length);
                    Tuple<string, string, string> result = HttpParse.AnalyzeRequestLine(line);
                    Method = result.Item1;
                    Url = result.Item2;
                    BaseUrl = HttpParse.GetBaseUrl(Url);
                    Ext = HttpParse.GetBaseUrlExt(BaseUrl);
                    HttpVersion = result.Item3;
                    HttpParse.AnalyzeQueryString(Url, mQueryString);
                    mState = LoadedState.Method;
                }
                //if (stream.TryReadLine(out line))
                //{
                //    Tuple<string, string, string> result = HttpParse.AnalyzeRequestLine(line);
                //    Method = result.Item1;
                //    Url = result.Item2;
                //    BaseUrl = HttpParse.GetBaseUrl(Url);
                //    Ext = HttpParse.GetBaseUrlExt(BaseUrl);
                //    HttpVersion = result.Item3;
                //    HttpParse.AnalyzeQueryString(Url, mQueryString);
                //    mState = LoadedState.Method;

                //}
            }
        }

        private void LoadHeaer(PipeStream stream)
        {
            if (mState == LoadedState.Method)
            {
                if (this.Header.Read(stream, mCookies))
                {
                    mState = LoadedState.Header;
                    int.TryParse(Header[HeaderType.CONTENT_LENGTH], out mLength);
                    KeepAlive = string.Compare(Header[HeaderType.CONNECTION], "Keep-Alive", true) == 0;
                }
            }
        }

        private void LoadBody(PipeStream stream)
        {
            if (mState == LoadedState.Header)
            {
                if (mLength == 0)
                {
                    mState = LoadedState.Completed;
                }
                else
                {
                    if (stream.Length == mLength)
                    {
                        mState = LoadedState.Completed;
                    }
                }
            }
        }

        private object mBody;

        public object GetBody(Type type)
        {
            if (mBody == null)
            {
                if (mLength == 0)
                    return null;
                else
                {
                    if (Serializer.TryDeserialize(mStream, mLength, type, out mBody))
                    {
                        mLength = 0;
                    }
                    return mBody;
                }
            }
            else
                return mBody;

        }

        public T GetBody<T>()
        {
            return (T)GetBody(typeof(T));
        }

        public HttpResponse CreateResponse()
        {
            HttpResponse response = new HttpResponse(this.Serializer);

            response.HttpVersion = this.HttpVersion;
            response.Serializer = this.Serializer;
            response.Session = this.Session;
            response.HttpVersion = this.HttpVersion;
            response.Request = this;
            if (this.KeepAlive)
                response.Header[HeaderType.CONNECTION] = "Keep-Alive";
            response.Header[HeaderType.HOST] = Header[HeaderType.HOST];
            return response;
        }
    }
}
