using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.Clients
{
    public class HttpClusterApi
    {
        public HttpClusterApi()
        {
            DefaultNode = new HttpNode();
            DetectionTime = 10000;
            mDetectionTimer = new System.Threading.Timer(OnVerifyClients, null, DetectionTime, DetectionTime);
        }

        public int DetectionTime { get; set; }

        private System.Threading.Timer mDetectionTimer;

        private ConcurrentDictionary<string, IHttpNode> mNodes = new ConcurrentDictionary<string, IHttpNode>();

        public ConcurrentDictionary<string, IHttpNode> Nodes => mNodes;

        public IHttpNode DefaultNode { get; set; }

        private IHttpNode GetNode(string url)
        {

            string key = url.ToLower();
            IHttpNode result;
            if (!mNodes.TryGetValue(key, out result))
                return DefaultNode;
            return result;
        }

        public HttpClusterApi AddHost(string url, IHttpNode node)
        {
            if (url == "*")
                DefaultNode = node;
            else
                mNodes[url.ToLower()] = node;
            return this;
        }

        public HttpClusterApi AddHost(string url, params string[] host)
        {
            if (url == "*")
            {
                if (host != null)
                {
                    foreach (string item in host)
                    {
                        DefaultNode.Add(item);
                    }
                }
            }
            else
            {
                if (host != null)
                {
                    IHttpNode node = GetNode(url);
                    if (node == null)
                    {
                        node = new HttpNode();
                        mNodes[url.ToLower()] = node;
                    }
                    foreach (string item in host)
                    {
                        node.Add(item);
                    }
                }
            }
            return this;
        }

        public virtual HttpHost GetClient(string url)
        {
            IHttpNode node = GetNode(url);
            if (node == null)
                return null;
            return node.GetClient();
        }

        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> mAPI = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

        public T Create<T>()
        {
            Type type = typeof(T);
            object result;
            if (!mAPI.TryGetValue(type, out result))
            {
                result = DispatchProxy.Create<T, HttpClusterApiProxy>();
                mAPI[type] = result;
                ((HttpClusterApiProxy)result).Cluster = this;
            }
            return (T)result;
        }

        protected async Task<T> OnExecute<T>(MethodBase targetMethod, params object[] args)
        {
            var request = ClientActionFactory.GetHandler((MethodInfo)targetMethod).GetRequestInfo(args);
            HttpHost host = GetClient(request.Url);
            if (host == null)
                throw new HttpClientException(null, null, "no http nodes are available");
            var r = request.GetRequest(host);
            var response = await r.Execute();
            return (T)response.Body;
        }

        private void OnVerifyClients(object state)
        {
            mDetectionTimer.Change(-1, -1);
            try
            {
                foreach (IHttpNode node in mNodes.Values)
                {
                    node.Verify();
                }

                if (DefaultNode != null)
                    DefaultNode.Verify();
            }
            catch { }
            finally
            {
                mDetectionTimer.Change(DetectionTime, DetectionTime);
            }

        }

        public ClusterStats Stats()
        {
            ClusterStats result = new ClusterStats();
            foreach (string key in mNodes.Keys)
            {
                if (mNodes.TryGetValue(key, out IHttpNode node))
                {
                    result.Items.Add(new ClusterStats.UrlStats() { Url = key, Node = node });
                }
            }
            result.Items.Add(new ClusterStats.UrlStats() { Url = "*", Node = DefaultNode });
            result.Items.Sort();
            return result;
        }
    }

    public class ClusterStats
    {

        public ClusterStats()
        {
            Items = new List<UrlStats>();
        }
        public List<UrlStats> Items { get; private set; }

        public class UrlStats : IComparable
        {
            public string Url { get; set; }

            public IHttpNode Node { get; set; }

            public int CompareTo(object obj)
            {
                return Url.CompareTo(((UrlStats)obj).Url);
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("* HttpClusterApi stats");
            return stringBuilder.ToString();
        }
    }


    public class HttpClusterApiProxy : System.Reflection.DispatchProxy
    {
        public HttpClusterApiProxy()
        {

        }

        public HttpClusterApi Cluster { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            ClientActionHanler handler = ClientActionFactory.GetHandler((MethodInfo)targetMethod);
            var rinfo = handler.GetRequestInfo(args);
            HttpHost host = Cluster.GetClient(rinfo.Url);
            if (host == null)
                throw new HttpClientException(null, null, "no http nodes are available");
            if (!handler.Async)
            {
                var request = rinfo.GetRequest(host);
                var task = request.Execute();
                int timeout = host.Pool.TimeOut;
                if (!task.Wait(timeout))
                {
                    throw new HttpClientException(request, host.Uri, $"{rinfo.Method} {rinfo.Url} request time out {timeout}!");
                }
                if (task.Result.Exception != null)
                    throw task.Result.Exception;
                return task.Result.Body;
            }
            else
            {
                var request = rinfo.GetRequest(host);
                var task = request.Execute();
                if (handler.MethodType == typeof(ValueTask))
                {
                    AnyCompletionSource<object> source = new AnyCompletionSource<object>();
                    source.WaitResponse(task);
                    return new ValueTask(source.Task);
                }
                else
                {
                    Type gtype = typeof(AnyCompletionSource<>);
                    Type type = gtype.MakeGenericType(handler.ReturnType);
                    IAnyCompletionSource source = (IAnyCompletionSource)Activator.CreateInstance(type);
                    source.WaitResponse(task);
                    return source.GetTask();
                }
            }

        }
    }

    public interface IHttpNode
    {
        void Add(string host);

        HttpHost GetClient();

        IList<HttpHost> Clients { get; }

        void Verify();
    }

    public class HttpNode : IHttpNode
    {
        public IList<HttpHost> Clients => mClients;

        private List<HttpHost> mClients = new List<HttpHost>();

        public void Add(string host)
        {
            Uri url = new Uri(host);
            if (mClients.Find(c => c.Host == url.Host && c.Port == url.Port) == null)
            {
                mClients.Add(new HttpHost(host));
            }
        }

        [ThreadStatic]
        private static int mIndex;

        public HttpHost GetClient()
        {
            int count = 0;
            while (count < mClients.Count)
            {
                mIndex++;
                HttpHost client = mClients[mIndex % mClients.Count];
                if (client.Available)
                    return client;
                count++;
            }
            return null;
        }

        public void Verify()
        {
            for (int i = 0; i < mClients.Count; i++)
            {
                HttpHost client = mClients[i];
                if (!client.Available)
                    OnVerify(client);
            }
        }

        private void OnVerify(HttpHost host)
        {
            try
            {
                var request = host.Get("/", null, null, null, null);
                var result = request.Execute();
                result.Wait(2000);
            }
            catch
            {

            }
        }
    }

}
