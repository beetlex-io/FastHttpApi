using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BeetleX.FastHttpApi.Clients
{
    public class HttpClusterApi : IDisposable
    {
        public HttpClusterApi()
        {
            DefaultNode = new ApiNode("*");

            DetectionTime = 2000;
            mDetectionTimer = new System.Threading.Timer(OnVerifyClients, null, DetectionTime, DetectionTime);

            TimeOut = 30000;
        }

        private long mVersion;



        private System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler> mHandlers = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler>();

        private void ChangeVersion()
        {
            System.Threading.Interlocked.Increment(ref mVersion);
        }

        public int TimeOut { get; set; }

        public long Version => mVersion;

        internal ClientActionHanler GetHandler(MethodInfo method)
        {
            ClientActionHanler result;
            if (!mHandlers.TryGetValue(method, out result))
            {
                result = new ClientActionHanler(method);
                mHandlers[method] = result;
            }
            return result;
        }

        private System.Threading.Timer mUploadNodeTimer;

        private ApiClusterInfo mLastClusterInfo = new ApiClusterInfo();

        public int DetectionTime { get; set; }

        public INodeSourceHandler NodeSourceHandler { get; set; }

        public bool UpdateSuccess { get; set; }

        public Exception UpdateExption { get; set; }

        private void UpdateNodeInfo(ApiClusterInfo info)
        {

            List<string> removeUrls = new List<string>();
            removeUrls.Add("*");
            foreach (string key in mNodes.Keys)
            {
                removeUrls.Add(key);
            }
            foreach (var item in info.Urls)
            {
                string url = item.Name.ToLower();
                if (item.Hosts.Count > 0)
                {
                    removeUrls.Remove(url);
                    SetNode(url, item.GetNode());
                }
            }
            foreach (string item in removeUrls)
            {
                RemoveNode(item);
            }
            ChangeVersion();
        }

        private async void OnUploadNode_Callback(object sate)
        {
            mUploadNodeTimer.Change(-1, -1);
            try
            {
                var info = await NodeSourceHandler.Load();
                if (info.Version != mLastClusterInfo.Version)
                {
                    mLastClusterInfo = info;
                    UpdateNodeInfo(info);
                }
                UpdateSuccess = true;
                UpdateExption = null;
            }
            catch (Exception e_)
            {
                UpdateSuccess = false;
                UpdateExption = e_;
            }
            finally
            {
                mUploadNodeTimer.Change(NodeSourceHandler.UpdateTime * 1000, NodeSourceHandler.UpdateTime * 1000);
            }
        }

        public async Task<HttpClusterApi> LoadNodeSource(string cluster, params string[] hosts)
        {
            HTTPRemoteSourceHandler hTTPRemoteSourceHandler = new HTTPRemoteSourceHandler(cluster, hosts);
            var result = await LoadNodeSource(hTTPRemoteSourceHandler);
            return result;

        }
        public async Task<HttpClusterApi> LoadNodeSource(INodeSourceHandler nodeSourceHandler)
        {
            NodeSourceHandler = nodeSourceHandler;
            var info = await NodeSourceHandler.Load();
            UpdateNodeInfo(info);
            mLastClusterInfo = info;
            mUploadNodeTimer = new System.Threading.Timer(OnUploadNode_Callback, null, NodeSourceHandler.UpdateTime * 1000, NodeSourceHandler.UpdateTime * 1000);
            return this;
        }

        private System.Threading.Timer mDetectionTimer;


        private ConcurrentDictionary<string, ApiNodeAgent> mAgents = new ConcurrentDictionary<string, ApiNodeAgent>();

        private ConcurrentDictionary<string, IApiNode> mNodes = new ConcurrentDictionary<string, IApiNode>();

        public ConcurrentDictionary<string, IApiNode> Nodes => mNodes;

        public IApiNode DefaultNode { get; internal set; }

        private IApiNode OnGetNode(string url)
        {
            url = url.ToLower();
            if (mNodes.TryGetValue(url, out IApiNode node))
                return node;
            return null;
        }

        private void RemoveNode(string url)
        {
            ChangeVersion();
            if (url == "*")
                DefaultNode = new ApiNode("*");
            else
                mNodes.TryRemove(url, out IApiNode apiNode);
            //SetNode(url, new ApiNode(url));
            if (mAgents.TryGetValue(url, out ApiNodeAgent agent))
            {
                agent.Node = DefaultNode;//new ApiNode(url);
            }
        }

        private IApiNode MatchNode(string url)
        {
            foreach (string key in mNodes.Keys)
            {
                if (Regex.IsMatch(url, key, RegexOptions.IgnoreCase))
                {
                    return mNodes[key];
                }
            }
            return null;
        }

        public ApiNodeAgent GetAgent(string url)
        {
            IApiNode node = MatchNode(url);
            if (node == null)
                node = DefaultNode;
            if (!mAgents.TryGetValue(node.Url, out ApiNodeAgent agent))
            {
                agent = new ApiNodeAgent();
                agent.Url = node.Url;
                agent.Node = node;
                mAgents[node.Url] = agent;
            }
            agent.Version = this.Version;
            return agent;
        }

        public HttpClusterApi SetNode(string url, IApiNode node)
        {
            if (url == "*")
                DefaultNode = node;
            else
                mNodes[url.ToLower()] = node;
            if (mAgents.TryGetValue(url, out ApiNodeAgent agent))
            {
                agent.Node = node;
            }
            else
            {
                agent = new ApiNodeAgent();
                agent.Url = url;
                agent.Node = node;
                mAgents[url] = agent;
            }
            ChangeVersion();
            return this;
        }

        private IApiNode CreateUrlNode(string url)
        {
            IApiNode node = new ApiNode(url);
            mNodes[node.Url] = node;
            ApiNodeAgent nodeAgent = new ApiNodeAgent();
            nodeAgent.Node = node;
            nodeAgent.Url = url;
            mAgents[node.Url] = nodeAgent;
            ChangeVersion();
            return node;
        }

        public IApiNode GetUrlNode(string url)
        {
            url = url.ToLower();
            if (url == "*")
                return DefaultNode;
            IApiNode node = OnGetNode(url);
            if (node == null)
            {
                node = CreateUrlNode(url);
            }
            return node;
        }
        public HttpClusterApi AddHost(string url, string host, int weight = 10)
        {
            ChangeVersion();
            if (url == "*")
            {
                DefaultNode.Add(host, weight);
            }
            else
            {
                url = url.ToLower();
                IApiNode node = OnGetNode(url);
                if (node == null)
                {
                    node = CreateUrlNode(url);
                }
                node.Add(host, weight);
            }
            return this;
        }

        public HttpClusterApi AddHost(string url, params string[] host)
        {
            ChangeVersion();
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
                    foreach (string item in host)
                    {
                        AddHost(url, item);
                    }
                }
            }
            return this;
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

        private void OnVerifyClients(object state)
        {
            mDetectionTimer.Change(-1, -1);
            try
            {
                foreach (IApiNode node in mNodes.Values)
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

        public ClusterStats Status()
        {
            ClusterStats result = new ClusterStats();
            foreach (var node in mNodes.Values)
            {
                result.Items.Add(new ClusterStats.UrlStats() { Url = node.Url, Node = node });
            }
            result.Items.Add(new ClusterStats.UrlStats() { Url = "*", Node = DefaultNode });
            result.Items.Sort();
            return result;
        }

        public void Dispose()
        {
            if (mUploadNodeTimer != null)
                mUploadNodeTimer.Dispose();
            if (mDetectionTimer != null)
                mDetectionTimer.Dispose();
            mHandlers.Clear();

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

            public IApiNode Node { get; set; }

            public int CompareTo(object obj)
            {
                return Url.CompareTo(((UrlStats)obj).Url);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("HttpClusterApi");
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                stringBuilder.AppendLine($"  |-Url:{item.Url}");
                foreach (var client in item.Node.Hosts.ToArray())
                {
                    string available = client.Available ? "Y" : "N";
                    stringBuilder.AppendLine($"  |--{client.Host}[{available}][W:{client.Weight}] [{client}]");
                }
            }
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
            ClientActionHanler handler = Cluster.GetHandler((MethodInfo)targetMethod);
            var rinfo = handler.GetRequestInfo(args);
            if (handler.NodeAgent == null || handler.NodeAgent.Version != Cluster.Version)
                handler.NodeAgent = Cluster.GetAgent(rinfo.Url);
            HttpHost host = handler.NodeAgent.Node.GetClient();
            if (host == null)
            {
                Exception error = new HttpClientException(null, null, $"request {rinfo.Url} no http nodes are available");
                if (handler.Async)
                {
                    if (handler.MethodType == typeof(ValueTask))
                        return new ValueTask(Task.FromException(error));
                    else
                    {
                        Type gtype = typeof(AnyCompletionSource<>);
                        Type type = gtype.MakeGenericType(handler.ReturnType);
                        IAnyCompletionSource source = (IAnyCompletionSource)Activator.CreateInstance(type);
                        source.Error(error);
                        return source.GetTask();
                    }
                }
                else
                {
                    throw error;
                }
            }
            if (!handler.Async)
            {
                var request = rinfo.GetRequest(host);
                var task = request.Execute();
              
                if (!task.Wait(Cluster.TimeOut))
                {
                    throw new HttpClientException(request, host.Uri, $"{rinfo.Method} {rinfo.Url} request time out {Cluster.TimeOut}!");
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

    public class ApiNodeAgent
    {
        public long Version { get; set; }

        public string Url { get; set; }

        public IApiNode Node { get; set; }
    }

    public interface IApiNode
    {
        string Url { get; set; }

        IApiNode Add(string host, int weight);

        IApiNode Add(string host);

        IApiNode Add(IEnumerable<string> hosts);

        HttpHost GetClient();

        List<HttpHost> Hosts { get; }

        void Verify();
    }

    public class ApiNode : IApiNode
    {
        public ApiNode(string url)
        {
            Url = url.ToLower();
        }
        public string Url { get; set; }

        public List<HttpHost> Hosts => mClients;

        private List<HttpHost> mClients = new List<HttpHost>();

        private long mID = 1;

        public const int TABLE_SIZE = 50;

        private HttpHost[] mHttpHostTable;

        public bool Available { get; private set; }

        public long Status
        {
            get
            {
                long result = 0;
                foreach (var item in mClients.ToArray())
                {
                    if (item.Available)
                        result |= item.ID;
                    else
                        result |= 0;
                }
                if (result > 0)
                    Available = true;
                return result;
            }
        }

        private long mLastStatus = 0;

        internal void RefreshWeightTable()
        {
            var status = Status;
            if (mLastStatus == status)
                return;
            else
                mLastStatus = status;
            HttpHost[] table = new HttpHost[TABLE_SIZE];
            int sum = 0;
            mClients.Sort((x, y) => y.Weight.CompareTo(x.Weight));
            List<HttpHost> aclients = new List<HttpHost>();
            for (int i = 0; i < mClients.Count; i++)
            {
                if (mClients[i].Available)
                {
                    sum += mClients[i].Weight;
                    aclients.Add(mClients[i]);
                }
            }
            int count = 0;
            for (int i = 0; i < aclients.Count; i++)
            {
                int size = (int)((double)aclients[i].Weight / (double)sum * (double)TABLE_SIZE);
                for (int k = 0; k < size; k++)
                {
                    table[count] = aclients[i];
                    count++;
                    if (count >= TABLE_SIZE)
                        goto END;
                }
            }
            int index = 0;
            while (count < TABLE_SIZE)
            {
                table[count] = aclients[index % aclients.Count];
                index++;
                count++;
            }
        END:
            Shuffle(table);
            mHttpHostTable = table;
        }

        private void Shuffle(HttpHost[] list)
        {
            Random rng = new Random();
            int n = list.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                HttpHost value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public IApiNode Add(IEnumerable<string> hosts)
        {
            if (hosts != null)
            {
                foreach (var item in hosts)
                {
                    Add(item, 10);
                }
            }
            return this;
        }

        public IApiNode Add(string host, int weight)
        {
            Uri url = new Uri(host);
            if (mClients.Find(c => c.Host == url.Host && c.Port == url.Port) == null)
            {
                var item = new HttpHost(host);
                item.ID = mID << mClients.Count;
                item.Weight = weight;
                item.MaxRPS = 0;
                mClients.Add(item);
                RefreshWeightTable();
            }
            return this;
        }

        public IApiNode Add(string host)
        {
            Add(host, 10);
            return this;
        }

        private long mIndex;

        public HttpHost GetClient()
        {
            if (Available)
            {
                HttpHost[] table = mHttpHostTable;
                if (table == null || table.Length == 0)
                    return null;
                int count = 0;
                long index = System.Threading.Interlocked.Increment(ref mIndex);
                while (count < TABLE_SIZE)
                {

                    HttpHost client = table[index % TABLE_SIZE];
                    if (client.Available)
                        return client;
                    index++;
                    count++;
                }
            }
            return null;
        }

        public void Verify()
        {
            int count = 0;
            for (int i = 0; i < mClients.Count; i++)
            {
                HttpHost client = mClients[i];
                if (!client.Available && !client.InVerify)
                {
                    client.InVerify = true;
                    count++;
                    Task.Run(() => OnVerify(client));
                }
            }
            RefreshWeightTable();
        }

        private async void OnVerify(HttpHost host)
        {
            try
            {
                var request = host.Get("/", null, null, null, null);
                var result = await request.Execute();
            }
            catch
            {

            }
            finally
            {
                host.InVerify = false;
            }
        }
    }

}
