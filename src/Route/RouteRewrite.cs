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
            this.UrlIgnoreCase = mServer.Options.UrlIgnoreCase;
        }

        public bool UrlIgnoreCase { get; set; }

        private ConcurrentDictionary<int, RouteGroup> mRoutes = new ConcurrentDictionary<int, RouteGroup>();

        private HttpApiServer mServer;

        public int Count => mRoutes.Count;

        public void Clear()
        {
            mRoutes.Clear();
        }

        public void AddRegion(UrlRoute[] routes)
        {
            if (routes != null)
                foreach (UrlRoute item in routes)
                {
                    Add(item);
                }
        }

        private void Add(UrlRoute item)
        {
            mServer.Log(EventArgs.LogType.Info, $"HTTP set rewrite url [{item.Url}] to [{item.Rewrite}]");
            item.UrlIgnoreCase = this.UrlIgnoreCase;
            item.Init();
            RouteGroup rg = null;
            mRoutes.TryGetValue(item.ID, out rg);
            if (rg == null)
            {
                rg = new RouteGroup();
                rg.Ext = item.Ext;
                mRoutes[item.ID] = rg;
            }
            rg.Add(item);
        }

        public void Remove(string url)
        {
            UrlRoute item = new UrlRoute { Rewrite = null, Url = url, Ext = null };
            item.Init();
            if (mRoutes.TryGetValue(item.ID, out RouteGroup rg))
            {
                mServer.Log(EventArgs.LogType.Info, $"HTTP remove rewrite url {item.Url}");
                rg.Remove(item);
            }
        }

        public RouteRewrite Add(string url, string rewriteurl, string ext = null)
        {
            var extTag = url.IndexOf(".");
            if (extTag > 0)
                ext = url.Substring(extTag + 1, url.Length - extTag - 1);
            UrlRoute route = new UrlRoute { Rewrite = rewriteurl, Url = url, Ext = ext };
            Add(route);
            return this;
        }

        public bool Match(HttpRequest request, ref RouteMatchResult result, QueryString queryString)
        {
            RouteGroup rg = null;
            var id = UrlRoute.GetPathID(request.Path);
            if (mRoutes.TryGetValue(id, out rg))
            {
                if (string.Compare(rg.Ext, request.Ext, true) == 0)
                {
                    if (rg.Match(request.BaseUrl, ref result, queryString))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<Config> GetRoutes()
        {
            List<Config> items = new List<Config>();
            foreach (var r in mRoutes.Values)
                items.AddRange(from a in r.Routes select new Config { Url = a.Url, Rewrite = a.Rewrite, Ext = a.Ext });
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
                            Add(item.Url, item.Rewrite, item.Ext);
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
            public string Url { get; set; }

            public string Rewrite { get; set; }

            public string Ext { get; set; }
        }

    }
}

