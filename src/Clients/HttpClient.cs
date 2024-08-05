using BeetleX.Clients;
using BeetleX.FastHttpApi.Clients;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{

    public class HttpClientPool
    {
        public HttpClientPool(string host, int port, bool ssl = false)
        {
            Host = host;
            Port = port;
            TimeOut = 10000;
            Async = true;
            MaxConnections = 100;
            Clients = new List<HttpClient>();
            SSL = ssl;
        }

        public bool SSL { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        private System.Collections.Concurrent.ConcurrentQueue<HttpClient> mPools = new System.Collections.Concurrent.ConcurrentQueue<HttpClient>();

        public List<HttpClient> Clients { get; private set; }

        private int mConnections = 0;

        public int MaxConnections { get; set; }

        public int Connections => mConnections;

        public int TimeOut { get; set; }

        public bool Async { get; set; }

        public HttpClient Pop(bool recursion = false)
        {
            HttpClient result;
            if (!mPools.TryDequeue(out result))
            {
                int value = System.Threading.Interlocked.Increment(ref mConnections);
                if (value > MaxConnections)
                {
                    System.Threading.Interlocked.Decrement(ref mConnections);
                    if (recursion)
                    {
                        throw new Exception($"Unable to reach {Host}:{Port} HTTP request, exceeding maximum number of connections");
                    }
                    else
                    {
                        for (int i = 0; i < Clients.Count; i++)
                        {
                            HttpClient client = Clients[i];
                            if (client.IsTimeout && client.Using)
                            {
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        client.RequestCommpletionSource.Error(new TimeoutException($"{Host}:{Port} request timeout!"));
                                    }
                                    finally
                                    {
                                        client.Client.DisConnect();
                                    }
                                });
                            }
                        }
                        System.Threading.Thread.Sleep(50);
                        return Pop(true);
                    }
                }
                var packet = new HttpClientPacket();
                if (Async)
                {
                    AsyncTcpClient client;
                    if (SSL)
                    {
                        client = SocketFactory.CreateSslClient<AsyncTcpClient>(packet, Host, Port, Host);
                    }
                    else
                    {
                        client = SocketFactory.CreateClient<AsyncTcpClient>(packet, Host, Port);
                    }
                    packet.Client = client;
                    client.Connected = c =>
                    {
                        c.Socket.NoDelay = true;
                        c.Socket.ReceiveTimeout = TimeOut;
                        c.Socket.SendTimeout = TimeOut;
                    };
                    result = new HttpClient();
                    result.Client = client;
                    result.Node = new LinkedListNode<HttpClient>(result);
                    Clients.Add(result);
                }
                else
                {
                    TcpClient client;
                    if (SSL)
                    {
                        client = SocketFactory.CreateSslClient<TcpClient>(packet, Host, Port, Host);
                    }
                    else
                    {
                        client = SocketFactory.CreateClient<TcpClient>(packet, Host, Port);
                    }
                    packet.Client = client;
                    client.Connected = c =>
                    {
                        c.Socket.NoDelay = true;
                        c.Socket.ReceiveTimeout = TimeOut;
                        c.Socket.SendTimeout = TimeOut;
                    };
                    result = new HttpClient();
                    result.Client = client;
                    result.Node = new LinkedListNode<HttpClient>(result);
                    Clients.Add(result);
                }
            }
            result.Using = true;
            result.TimeOut = BeetleX.TimeWatch.GetElapsedMilliseconds() + TimeOut * 1000;
            return result;
        }

        public void Push(HttpClient client)
        {
            client.Using = false;
            mPools.Enqueue(client);
        }
    }

    public class HttpClient
    {
        public IClient Client { get; set; }

        public LinkedListNode<HttpClient> Node { get; set; }

        public HttpClientPool Pool { get; set; }

        internal long TimeOut { get; set; }

        public bool Using { get; set; }

        public bool IsTimeout
        {
            get
            {
                return BeetleX.TimeWatch.GetElapsedMilliseconds() > TimeOut;
            }
        }

        internal IAnyCompletionSource RequestCommpletionSource { get; set; }

    }

    public class HttpClientPoolFactory
    {

        private static System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool> mPools = new System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool>();

        public static System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool> Pools => mPools;

        public static void SetPoolInfo(Uri host, int maxConn, int timeout, bool async = true)
        {
            HttpClientPool pool = GetPool(null, host);
            pool.MaxConnections = maxConn;
            pool.TimeOut = timeout;
            pool.Async = async;
        }

        public static void SetPoolInfo(string host, int maxConn, int timeout, bool async = true)
        {
            SetPoolInfo(new Uri(host), maxConn, timeout, async);
        }

        public static HttpClientPool GetPool(string key, Uri uri)
        {
            if (string.IsNullOrEmpty(key))
                key = $"{uri.Host}:{uri.Port}";
            HttpClientPool result;
            if (mPools.TryGetValue(key, out result))
                return result;
            return CreatePool(key, uri);
        }

        private static HttpClientPool CreatePool(string key, Uri uri)
        {
            lock (typeof(HttpClientPoolFactory))
            {
                HttpClientPool result;
                if (!mPools.TryGetValue(key, out result))
                {
                    bool ssl = uri.Scheme.ToLower() == "https";
                    result = new HttpClientPool(uri.Host, uri.Port, ssl);
                    mPools[key] = result;
                }
                return result;
            }
        }
    }

    public class HttpHost
    {
        public HttpHost(string host) : this(new Uri(host))
        {

        }

        public HttpHost(Uri host)
        {
            this.Uri = host;
            Formater = new FormUrlFormater();
            Host = this.Uri.Host;
            Port = this.Uri.Port;
            mPoolKey = $"{this.Uri.Host}:{this.Uri.Port}";
            mPool = HttpClientPoolFactory.GetPool(mPoolKey, this.Uri);
            Available = true;
            InVerify = false;
        }

        private HttpClientPool mPool;

        public long ID { get; set; }

        private long mSuccess;

        private long mLastSuccess;

        private long mError;

        private int mSocketErrors;

        public int Weight { get; set; }

        public HttpClientPool Pool => mPool;

        private string mPoolKey;

        public static int DisconnectErrors { get; set; } = 5;

        public string Host { get; set; }

        public int Port { get; set; }

        public IClientBodyFormater Formater { get; set; }

        public Uri Uri { get; private set; }

        public bool Available { get; set; }

        public long Success => mSuccess;

        public long Error => mError;

        internal int SocketErrors => mSocketErrors;

        internal bool InVerify { get; set; }

        //private int mRps;

        //private long mLastTime;

        //public int RPS => mRps;

        public int MaxRPS { get; set; }

        //public bool ValidateRps()
        //{
        //    if (MaxRPS == 0)
        //        return true;
        //    long now = TimeWatch.GetElapsedMilliseconds();
        //    if (now - mLastTime >= 1000)
        //        return true;
        //    return mRps < MaxRPS;
        //}

        internal void AddSuccess()
        {
            mSuccess++;
            Available = true;
            //long now = TimeWatch.GetElapsedMilliseconds();
            //if (now - mLastTime >= 1000)
            //{
            //    mRps = 1;
            //    mLastTime = now;
            //}
            //else
            //{
            //    mRps++;
            //}
        }

        internal void AddError(bool socketError)
        {
            if (socketError)
            {
                mSocketErrors++;

            }
            else
            {
                mSocketErrors = 0;

            }
            if (mSocketErrors >= DisconnectErrors)
                Available = false;
            else
                Available = true;
            mError++;
        }

        public override string ToString()
        {
            string result = $"{mSuccess - mLastSuccess}/{mSuccess}";
            mLastSuccess = mSuccess;
            return result;
        }

        public Request Put(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, Dictionary<string, object> data, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Method = Request.PUT;
            request.Formater = formater == null ? this.Formater : formater;
            request.Header = new Header();
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = request.Formater.ContentType;
            request.Header[HeaderTypeFactory.HOST] = Host;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.QuestryString = queryString;
            request.Url = url;
            request.Body = data;
            request.BodyType = bodyType;
            request.HttpHost = this;
            return request;
        }

        public Request Post(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, Dictionary<string, object> data, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Method = Request.POST;
            request.Formater = formater == null ? this.Formater : formater;
            request.Header = new Header();

            request.Header[HeaderTypeFactory.CONTENT_TYPE] = request.Formater.ContentType;
            request.Header[HeaderTypeFactory.HOST] = Host;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.QuestryString = queryString;
            request.Url = url;
            request.Body = data;
            request.BodyType = bodyType;
            request.HttpHost = this;
            return request;
        }

        public Request Delete(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Header = new Header();
            request.Formater = formater == null ? this.Formater : formater;
            request.Method = Request.DELETE;
            request.Header[HeaderTypeFactory.HOST] = Host;
            request.QuestryString = queryString;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.Url = url;
            request.BodyType = bodyType;
            request.HttpHost = this;
            return request;
        }

        public Request Get(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Header = new Header();
            request.Formater = formater == null ? this.Formater : formater;
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = request.Formater.ContentType;
            request.Header[HeaderTypeFactory.HOST] = Host;
            request.QuestryString = queryString;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.Url = url;
            request.BodyType = bodyType;
            request.HttpHost = this;
            return request;
        }

    }


}
