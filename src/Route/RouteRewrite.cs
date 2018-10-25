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
        }



        private Dictionary<string, RouteGroup> mRoutes = new Dictionary<string, RouteGroup>();

        private HttpApiServer mServer;

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
            if (mServer.EnableLog(EventArgs.LogType.Info))
            {
                mServer.Log(EventArgs.LogType.Info, null, "rewrite setting {0} to {1}", item.Url, item.Rewrite);
            }
            item.Init();
            RouteGroup rg = null;
            mRoutes.TryGetValue(item.Path, out rg);
            if (rg == null)
            {
                rg = new RouteGroup();
                rg.Ext = item.Ext;
                mRoutes[item.Path] = rg;
            }
            rg.Routes.Add(item);
        }

        public void Add(string pattern, string rewritePattern)
        {
            UrlRoute route = new UrlRoute { Rewrite = rewritePattern, Url = pattern };
            Add(route);
        }

        public bool Match(HttpRequest request, out string rewriteUrl, out string rewriteLower, QueryString queryString)
        {
            rewriteUrl = null;
            rewriteLower = null;
            RouteGroup rg = null;
            if (mRoutes.TryGetValue(request.Path, out rg))
            {
                if (string.Compare(rg.Ext, request.Ext, true) == 0)
                {
                    if (rg.Match(request.Url, out rewriteUrl, out rewriteLower, queryString))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

