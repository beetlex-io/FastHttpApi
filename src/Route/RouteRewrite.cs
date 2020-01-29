using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeetleX.FastHttpApi
{



    public class RouteRewrite
    {

        private const string REWRITE_FILE = "rewrite.json";

        public RouteRewrite(HttpApiServer server)
        {
            mServer = server;
            mRouteCached = new LRUCached(mServer.Options.RewriteIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            mRouteCached.MaxSize = mServer.Options.RewriteCachedSize;
        }

        private long mVersion = 0;

        private LRUCached mRouteCached;

        private ConcurrentDictionary<string, RouteGroup> mRoutes = new ConcurrentDictionary<string, RouteGroup>(StringComparer.OrdinalIgnoreCase);

        private RouteGroup[] mMatchRoutes = new RouteGroup[0];

        private HttpApiServer mServer;

        public int MaxCacheSize
        {
            get
            {
                return mRouteCached.MaxSize;
            }
            set
            {
                mRouteCached.MaxSize = value;
            }
        }

        public int Count => mRoutes.Count;

        public void Clear()
        {
            mMatchRoutes = new RouteGroup[0];
            mRoutes.Clear();
            ChangeVersion();
        }

        public void AddRegion(UrlRoute[] routes)
        {
            if (routes != null)
                foreach (UrlRoute item in routes)
                {
                    Add(item);
                }
        }

        private void ChangeVersion()
        {
            mMatchRoutes = (from a in mRoutes.Values orderby a.PathLevel ascending select a).ToArray(); //mRoutes.Values.ToArray();
            System.Threading.Interlocked.Increment(ref mVersion);
        }

        private void Add(UrlRoute item)
        {
            mServer.GetLog(EventArgs.LogType.Info)?.Log(EventArgs.LogType.Info, $"HTTP set rewrite url [{item.Host}{item.Url}] to [{item.Rewrite}]");
            item.Init();
            RouteGroup rg = null;
            mRoutes.TryGetValue(item.Path, out rg);
            if (rg == null)
            {
                rg = new RouteGroup();
                rg.Path = item.Path;
                rg.PathLevel = item.PathLevel;
                mRoutes[item.Path] = rg;
            }
            rg.Add(item);
            ChangeVersion();
        }

        public void Remove(string host, string url)
        {
            UrlRoute item = new UrlRoute { Host = host, Rewrite = null, Url = url, Ext = null };
            item.Init();
            if (mRoutes.TryGetValue(item.Path, out RouteGroup rg))
            {
                mServer.GetLog(EventArgs.LogType.Info)?.Log(EventArgs.LogType.Info, $"HTTP remove rewrite url {item.Url}");
                rg.Remove(item);
                ChangeVersion();
            }
        }

        public void Remove(string url)
        {
            Remove(null, url);
        }

        public RouteRewrite Add(string url, string rewriteurl)
        {
            return Add(null, url, rewriteurl, null);
        }

        public RouteRewrite Add(string host, string url, string rewriteurl, string ext)
        {
            var extTag = url.IndexOf(".");
            if (extTag > 0)
                ext = url.Substring(extTag + 1, url.Length - extTag - 1);
            UrlRoute route = new UrlRoute { Rewrite = rewriteurl, Url = url, Ext = ext, Host = host };
            Add(route);
            return this;
        }

        public RouteRewrite Add(string host, string url, string rewriteurl)
        {
            return Add(host, url, rewriteurl, null);
        }

        public bool Match(HttpRequest request, out RouteMatchResult result, QueryString queryString)
        {
            result = null;
            RouteGroup rg = null;
            UrlRoute urlRoute = null;
            string key = $"{request.GetHostBase()}{request.BaseUrl}";
            if (mRouteCached.TryGetValue(key, out UrlRouteAgent cached))
            {
                cached.ActiveTime = TimeWatch.GetTotalSeconds();
                if (cached.Route == null && cached.Version == this.mVersion)
                {
                    if (request.Server.EnableLog(EventArgs.LogType.Info))
                    {
                        request.Server.Log(EventArgs.LogType.Info, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {key} rewrite cached miss");
                    }
                    return false;
                }
                if (cached.Route != null && cached.Version == this.mVersion)
                {
                    if (cached.Parameters.Count > 0)
                        foreach (var item in cached.Parameters)
                            queryString.Add(item.Key, item.Value);
                    result = cached.MatchResult;
                    if (request.Server.EnableLog(EventArgs.LogType.Info))
                    {
                        request.Server.Log(EventArgs.LogType.Info, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {key} rewrite cached hit");
                    }
                    return true;

                }
            }
            bool iscached = true;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            result = new RouteMatchResult();
            if (mRoutes.TryGetValue(request.Path, out rg))
            {
                urlRoute = rg.Match(request.BaseUrl, ref result, parameters, request.Ext, request);
            }
            if (urlRoute == null)
            {
                RouteGroup[] rgs = mMatchRoutes;
                if (rgs != null)
                {
                    for (int i = 0; i < rgs.Length; i++)
                    {
                        rg = rgs[i];
                        if (rg.PathLevel == request.PathLevel)
                        {
                            iscached = rg.Cached;
                            urlRoute = rg.Match(request.BaseUrl, ref result, parameters, request.Ext, request);
                            if (urlRoute != null)
                            {
                                if (urlRoute.Prefix != null && urlRoute.Prefix.Type == UrlPrefix.PrefixType.Host)
                                    iscached = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (urlRoute != null)
                result.HasQueryString = urlRoute.HasQueryString;
            if (urlRoute != null && urlRoute.Prefix?.Type == UrlPrefix.PrefixType.Param)
                return urlRoute != null;
            var agent = new UrlRouteAgent { Route = urlRoute, Version = this.mVersion, MatchResult = result, Parameters = parameters };
            if (parameters.Count > 0)
                foreach (var ps in parameters)
                    queryString.Add(ps.Key, ps.Value);
            if (iscached)
            {
                UrlRouteAgent exits = (UrlRouteAgent)mRouteCached.ExistOrAdd(key, agent);
                if (request.Server.EnableLog(EventArgs.LogType.Info))
                {
                    request.Server.Log(EventArgs.LogType.Info, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {key} rewrite save to cached");
                }
                if (exits != null)
                {
                    exits.Route = urlRoute;
                    exits.Version = mVersion;
                    exits.MatchResult = result;
                    exits.Parameters = parameters;
                }
            }
            return urlRoute != null;
        }

        public List<Config> GetRoutes()
        {
            List<Config> items = new List<Config>();
            foreach (var r in mRoutes.Values)
                items.AddRange(from a in r.Routes select new Config { Host = a.Host, Url = a.Url, Rewrite = a.Rewrite });
            return items;
        }

        public void Load()
        {
            if (System.IO.File.Exists(REWRITE_FILE))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(REWRITE_FILE))
                {
                    string data = reader.ReadToEnd();
                    List<Config> items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Config>>(data);
                    foreach (var item in items)
                    {
                        if (!string.IsNullOrEmpty(item.Url) && !string.IsNullOrEmpty(item.Rewrite))
                            Add(item.Host, item.Url, item.Rewrite, null);
                    }
                }
            }
        }

        public void Save()
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(REWRITE_FILE, false))
            {
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(GetRoutes()));
                writer.Flush();
            }

        }

        public class Config
        {

            public string Host { get; set; }

            public string Url { get; set; }

            public string Rewrite { get; set; }

        }

        class UrlRouteAgent
        {

            public Dictionary<string, string> Parameters { get; set; }

            public Double ActiveTime { get; set; }

            public RouteMatchResult MatchResult { get; set; }

            public UrlRoute Route { get; set; }

            public long Version { get; set; }
        }

    }
}

