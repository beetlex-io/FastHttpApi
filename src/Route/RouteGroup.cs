using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class RouteGroup
    {

        public RouteGroup()
        {
            mRoutes = new List<UrlRoute>();
        }

        public string Ext { get; set; }

        private List<UrlRoute> mRoutes;

        private UrlRoute[] mMatchRoute = new UrlRoute[0];

        public UrlRoute[] Routes => mMatchRoute;

        public void Remove(UrlRoute route)
        {
            mRoutes.RemoveAll(p => string.Compare(p.Url, route.Url, true) == 0);
            mMatchRoute = mRoutes.ToArray();
        }


        public void Add(UrlRoute route)
        {
            for (int i = 0; i < mRoutes.Count; i++)
            {
                if (string.Compare(mRoutes[i].Url, route.Url, true) == 0)
                {
                    mRoutes[i] = route;
                    return;
                }
            }
            mRoutes.Add(route);
            mMatchRoute = mRoutes.ToArray();
        }

        public bool Match(string url, ref RouteMatchResult result, QueryString queryString)
        {
            var items = mMatchRoute;
            for (int i = 0; i < items.Length; i++)
            {
                UrlRoute urlRoute = items[i];
                Dictionary<string, string> ps = new Dictionary<string, string>();
                if (urlRoute.Match(url, ps))
                {
                    if (ps.Count > 0)
                        foreach (var item in ps)
                            queryString.Add(item.Key, item.Value);
                    result.Ext = urlRoute.ReExt;
                    result.RewriteUrl = urlRoute.GetRewriteUrl(ps);
                    result.RewriteUrlLower = HttpParse.CharToLower(result.RewriteUrl);
                    return true;
                }
            }
            return false;
        }

    }
}
