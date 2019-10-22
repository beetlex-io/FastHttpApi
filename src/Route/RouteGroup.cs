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

        public int PathLevel { get; set; }

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

        public UrlRoute Match(string url, ref RouteMatchResult result, Dictionary<string, string> parameters, string ext, HttpRequest request)
        {

            var items = mMatchRoute;
            for (int i = 0; i < items.Length; i++)
            {
                UrlRoute urlRoute = items[i];
                if (string.Compare(urlRoute.Ext, ext, true) == 0)
                {
                    if (urlRoute.Match(url, parameters))
                    {
                        result.Ext = urlRoute.ReExt;
                        result.RewriteUrl = urlRoute.GetRewriteUrl(parameters);
                        return urlRoute;
                    }
                }
            }
            return null;
        }

    }
}
