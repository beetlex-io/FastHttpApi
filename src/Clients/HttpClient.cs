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
            TimeOut = 5000;
        }

        public string Host { get; set; }

        public int Port { get; set; }

        private System.Collections.Concurrent.ConcurrentStack<AsyncTcpClient> mPools = new System.Collections.Concurrent.ConcurrentStack<AsyncTcpClient>();

        private int mConnections = 0;

        private int mMinConnections = 10;

        private int mMaxConnections = 50;

        private int mPoolConnectins = 0;

        public void SetPool(int min, int max)
        {
            mMinConnections = min;
            mMaxConnections = max;
        }
        public int TimeOut { get; set; }

        public AsyncTcpClient Pop()
        {
            AsyncTcpClient result;
            if (!mPools.TryPop(out result))
            {
                int value = System.Threading.Interlocked.Increment(ref mConnections);
                if (value > mMaxConnections)
                {
                    System.Threading.Interlocked.Decrement(ref mConnections);
                    throw new Exception("Unable to reach HTTP request, exceeding maximum number of connections");
                }
                var packet = new HttpClientPacket();
                result = SocketFactory.CreateClient<AsyncTcpClient>(packet, Host, Port);
                packet.Client = result;
                result.Connected = c => { c.Socket.NoDelay = true; };
                result.Connect();
                result.Socket.SendTimeout = TimeOut;
                result.Socket.ReceiveTimeout = TimeOut;
            }
            else
            {
                var value = System.Threading.Interlocked.Decrement(ref mPoolConnectins);
            }
            return result;
        }

        public void Push(AsyncTcpClient client)
        {
            var value = System.Threading.Interlocked.Increment(ref mPoolConnectins);
            if (value > mMinConnections)
            {
                System.Threading.Interlocked.Decrement(ref mPoolConnectins);
                System.Threading.Interlocked.Decrement(ref mConnections);
                client.DisConnect();
            }
            else
            {
                mPools.Push(client);
            }
        }
    }

    public class HttpClientPoolFactory
    {

        private static System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool> mPools = new System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool>();

        public static System.Collections.Concurrent.ConcurrentDictionary<string, HttpClientPool> Pools => mPools;

        public static void SetPoolSize(string host, int minConn, int maxConn)
        {
            Uri url = new Uri(host);
            GetPool(null, url).SetPool(minConn, maxConn);

        }

        public static void SetTimeout(string host, int timeout)
        {
            Uri url = new Uri(host);
            GetPool(null, url).TimeOut = timeout;

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


    public class HttpApiClient : IDisposable
    {
        public HttpApiClient(string host)
        {
            Uri = new Uri(host);

            Header = new Header();
            Formater = new FormUrlFormater();
            Host = Uri.Host;
            mPoolKey = $"{Uri.Host}:{Uri.Port}";
            mPool = HttpClientPoolFactory.GetPool(mPoolKey, this.Uri);
        }


        private HttpClientPool mPool;

        public HttpClientPool Pool => mPool;

        private string mPoolKey;

        public string Host { get; set; }


        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> mAPI = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

        public WEBAPI CreateWebapi<WEBAPI>()
        {
            Type type = typeof(WEBAPI);
            object result;
            if (!mAPI.TryGetValue(type, out result))
            {
                result = DispatchProxy.Create<WEBAPI, HttpDispatchProxy>();
                mAPI[type] = result;
                ((HttpDispatchProxy)result).HttpApiClient = this;
            }
            return (WEBAPI)result;
        }

        public Header Header { get; private set; }

        public IClientBodyFormater Formater { get; set; }

        public Uri Uri { get; private set; }

        [ThreadStatic]
        private static Header mResponseHeader;

        public Header ResponseHeader
        {
            get { return mResponseHeader; }
            set { mResponseHeader = value; }
        }

        public Task<Response> Put(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, Dictionary<string, object> data, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Method = Request.PUT;
            request.Formater = formater == null ? this.Formater : formater;
            request.Header = new Header();
            Header.CopyTo(request.Header);
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = request.Formater.ContentType;
            request.Header[HeaderTypeFactory.HOST] = Host;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.QuestryString = queryString;
            request.Url = url;
            request.Body = data;
            return Execute(request, bodyType);
        }

        public Task<Response> Post(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, Dictionary<string, object> data, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Method = Request.POST;
            request.Formater = formater == null ? this.Formater : formater;
            request.Header = new Header();
            Header.CopyTo(request.Header);
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = request.Formater.ContentType;
            request.Header[HeaderTypeFactory.HOST] = Host;
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.QuestryString = queryString;
            request.Url = url;
            request.Body = data;
            return Execute(request, bodyType);
        }

        public Task<Response> Delete(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Header = new Header();
            request.Formater = formater == null ? this.Formater : formater;
            request.Method = Request.DELETE;
            request.Header[HeaderTypeFactory.HOST] = Host;
            request.QuestryString = queryString;
            Header.CopyTo(request.Header);
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.Url = url;
            return Execute(request, bodyType);
        }

        public Task<Response> Get(string url, Dictionary<string, string> header, Dictionary<string, string> queryString, IClientBodyFormater formater, Type bodyType = null)
        {
            Request request = new Request();
            request.Header = new Header();
            request.Formater = formater == null ? this.Formater : formater;
            request.Header[HeaderTypeFactory.CONTENT_TYPE] = "text/plain";
            request.Header[HeaderTypeFactory.HOST] = Host;
            request.QuestryString = queryString;
            Header.CopyTo(request.Header);
            if (header != null)
                foreach (var item in header)
                    request.Header[item.Key] = item.Value;
            request.Url = url;
            return Execute(request, bodyType);
        }

        public async Task<Response> Execute(Request request, Type bodyType = null)
        {
            AsyncTcpClient tcpClient = mPool.Pop();
            try
            {
                var result = await tcpClient.Send(request).ReceiveMessage<Response>();
                ResponseHeader = result.Header;
                int code = int.Parse(result.Code);
                if (result.Length > 0)
                {
                    try
                    {
                        if (code < 400)
                            result.Body = request.Formater.Deserialization(result.Stream, bodyType, result.Length);
                        else
                            result.Body = result.Stream.ReadString(result.Length);
                    }
                    finally
                    {
                        result.Stream.ReadFree(result.Length);
                        if (result.Chunked)
                            result.Stream.Dispose();
                        result.Stream = null;
                    }
                }
                if (!result.KeepAlive)
                    tcpClient.DisConnect();
                if (code >= 400)
                    throw new System.Net.WebException($"{request.Method} {request.Url} {result.Code} {result.CodeMsg} {result.Body}");
                return result;
            }
            finally
            {
                if (tcpClient != null)
                    mPool.Push(tcpClient);
            }
        }

        public void Dispose()
        {

        }

        private System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler> mHandlers = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler>();

        private ClientActionHanler GetHandler(MethodInfo method)
        {
            ClientActionHanler result;
            if (!mHandlers.TryGetValue(method, out result))
            {
                result = new ClientActionHanler(method);
                mHandlers[method] = result;
            }
            return result;
        }

        public Task<Response> Invoke(MethodInfo targetMethod, object[] args)
        {
            ClientActionHanler handler = GetHandler(targetMethod);
            var request = handler.GetRequest(args);
            Task<Response> response;
            switch (request.Method)
            {
                case Request.POST:
                    response = Post(request.Url, request.Header, request.QueryString, request.Data, handler.Formater, request.Type);
                    break;
                case Request.PUT:
                    response = Put(request.Url, request.Header, request.QueryString, request.Data, handler.Formater, request.Type);
                    break;
                case Request.DELETE:
                    response = Delete(request.Url, request.Header, request.QueryString, handler.Formater, request.Type);
                    break;
                default:
                    response = Get(request.Url, request.Header, request.QueryString, handler.Formater, request.Type);
                    break;
            }
            return response;
        }

    }

    public abstract class HttpApiBase
    {
        public HttpApiBase(string host)
        {
            Client = new HttpApiClient(host);
        }

        protected HttpApiClient Client { get; set; }

        protected async ValueTask<T> OnExecute<T>(MethodBase targetMethod, params object[] args)
        {
            var response = await Client.Invoke((MethodInfo)targetMethod, args);
            return (T)response.Body;
        }
    }
}
