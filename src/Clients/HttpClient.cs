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
        public HttpClientPool(string host, int port)
        {
            Host = host;
            Port = port;
            TimeOut = 10000;
            Async = true;
            MaxConnections = 50;
        }

        public string Host { get; set; }

        public int Port { get; set; }

        private System.Collections.Concurrent.ConcurrentQueue<IClient> mPools = new System.Collections.Concurrent.ConcurrentQueue<IClient>();

        private int mConnections = 0;

        public int MaxConnections { get; set; }

        public int Connections => mConnections;

        public int TimeOut { get; set; }

        public bool Async { get; set; }

        public IClient Pop()
        {
            IClient result;
            if (!mPools.TryDequeue(out result))
            {
                int value = System.Threading.Interlocked.Increment(ref mConnections);
                if (value > MaxConnections)
                {
                    System.Threading.Interlocked.Decrement(ref mConnections);
                    throw new Exception("Unable to reach HTTP request, exceeding maximum number of connections");
                }
                var packet = new HttpClientPacket();
                if (Async)
                {
                    AsyncTcpClient client = SocketFactory.CreateClient<AsyncTcpClient>(packet, Host, Port);
                    packet.Client = client;
                    client.Connected = c =>
                    {
                        c.Socket.NoDelay = true;
                        c.Socket.ReceiveTimeout = TimeOut;
                        c.Socket.SendTimeout = TimeOut;
                    };
                    return client;
                }
                else
                {
                    TcpClient client = SocketFactory.CreateClient<TcpClient>(packet, Host, Port);
                    packet.Client = client;
                    client.Connected = c =>
                    {
                        c.Socket.NoDelay = true;
                        c.Socket.ReceiveTimeout = TimeOut;
                        c.Socket.SendTimeout = TimeOut;
                    };
                    return client;
                }
            }
            return result;
        }

        public void Push(IClient client)
        {
            mPools.Enqueue(client);
        }
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
                    result = new HttpClientPool(uri.Host, uri.Port);
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
            Uri = host;
            Formater = new FormUrlFormater();
            Host = Uri.Host;
            Port = Uri.Port;
            mPoolKey = $"{Uri.Host}:{Uri.Port}";
            mPool = HttpClientPoolFactory.GetPool(mPoolKey, this.Uri);
            Available = true;
        }

        private HttpClientPool mPool;

        private long mSuccess;

        private long mError;

        public HttpClientPool Pool => mPool;

        private string mPoolKey;

        public string Host { get; set; }

        public int Port { get; set; }

        public IClientBodyFormater Formater { get; set; }

        public Uri Uri { get; private set; }

        public bool Available { get; set; }

        public long Success => mSuccess;

        public long Error => mError;

        internal void AddSuccess()
        {
            System.Threading.Interlocked.Increment(ref mSuccess);
        }

        internal void AddError()
        {
            System.Threading.Interlocked.Increment(ref mError);
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
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = "text/plain";
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
