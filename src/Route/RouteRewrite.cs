using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class RouteRewrite
    {

        public RouteRewrite(HttpApiServer server)
        {
            mServer = server;
            this.UrlIgnoreCase = mServer.Options.UrlIgnoreCase;
        }

        public bool UrlIgnoreCase { get; set; }

        private Dictionary<int, RouteGroup> mRoutes = new Dictionary<int, RouteGroup>();

        private HttpApiServer mServer;

        public int Count => mRoutes.Count;

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
            mServer.Log(EventArgs.LogType.Info, $"rewrite url [{item.Url}] to [{item.Rewrite}]");
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


        public RouteRewrite Add(string url, string rewriteurl, string ext = null)
        {
            UrlRoute route = new UrlRoute { Rewrite = rewriteurl, Url = url, Ext = ext };
            Add(route);
            return this;
        }

        public bool Match(HttpRequest request, ref RouteMatchResult result, QueryString queryString)
        {
            RouteGroup rg = null;
            if (mRoutes.TryGetValue(request.Path.GetHashCode(), out rg))
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
    }
}

