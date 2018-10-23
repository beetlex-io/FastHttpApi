using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Route
{
    public class RouteRewrite
    {

        public RouteRewrite(HttpApiServer server)
        {
            mServer = server;
        }

        private HttpApiServer mServer;

        private List<RouteGroup> Routes = new List<RouteGroup>();

        public void AddRegion(UrlRoute[] routes)
        {
            if (Routes != null)
            {
                if (routes != null)
                    foreach (UrlRoute item in routes)
                    {
                        Add(item);
                    }
            }
        }

        private void Add(UrlRoute item)
        {
            if (mServer.EnableLog(EventArgs.LogType.Info))
            {
                mServer.BaseServer.Log(EventArgs.LogType.Info, null, "rewrite setting {0} to {1}", item.Url, item.Rewrite);
            }
            item.Init();
            RouteGroup rg = Routes.Find(r => string.Compare(r.Ext, item.Ext, true) == 0);
            if (rg == null)
            {
                rg = new RouteGroup();
                rg.Ext = item.Ext;
                Routes.Add(rg);
            }
            rg.Routes.Add(item);
        }

        public void Add(string pattern, string rewritePattern)
        {
            UrlRoute route = new UrlRoute { Rewrite = rewritePattern, Url = pattern };
            Add(route);
        }

        public bool Match(HttpRequest request, out string rewriteUrl)
        {
            rewriteUrl = null;
            if (Routes.Count == 0)
                return false;
            RouteGroup rg;

            for (int i = 0; i < Routes.Count; i++)
            {
                rg = Routes[i];
                if (string.Compare(rg.Ext, request.Ext, true) == 0)
                {
                    if (rg.Match(request.Url, out rewriteUrl))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

