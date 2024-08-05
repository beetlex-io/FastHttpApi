using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace BeetleX.FastHttpApi
{

    public class RouteGroup
    {

        public RouteGroup()
        {
            mRoutes = new List<UrlRoute>();
        }

        public bool Cached { get; set; } = true;

        public string Path { get; set; }

        public int PathLevel { get; set; }

        private List<UrlRoute> mRoutes;

        private UrlRoute[] mMatchRoute = new UrlRoute[0];

        public UrlRoute[] Routes => mMatchRoute;

        public void Remove(UrlRoute route)
        {
            mRoutes.RemoveAll(p => string.Compare(p.ID, route.ID, true) == 0);
            Refresh();
        }

        private void Refresh()
        {
            mMatchRoute = (from a in mRoutes orderby a.GetPrefixCode() descending select a).ToArray();
            foreach (var item in mMatchRoute)
            {
                if (item.Prefix != null && item.Prefix.Type == UrlPrefix.PrefixType.Param)
                {
                    Cached = false;
                    return;
                }
            }
            Cached = true;
        }


        public void Add(UrlRoute route)
        {
            for (int i = 0; i < mRoutes.Count; i++)
            {
                if (string.Compare(mRoutes[i].ID, route.ID, true) == 0)
                {
                    mRoutes[i] = route;
                    return;
                }
            }
            mRoutes.Add(route);
            Refresh();
        }

        public UrlRoute Match(string url, ref RouteMatchResult result, Dictionary<string, string> parameters, string ext, HttpRequest request)
        {

            var items = mMatchRoute;
            for (int i = 0; i < items.Length; i++)
            {
                UrlRoute urlRoute = items[i];
                if (string.Compare(urlRoute.Ext, ext, true) == 0)
                {
                    if (urlRoute.Prefix != null)
                    {
                        var prefixValue = urlRoute.Prefix.GetPrefix(request);
                        if (string.Compare(prefixValue, urlRoute.Prefix.Value, true) != 0)
                            continue;
                    }
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

        public override string ToString()
        {
            return Path;
        }

    }
}
