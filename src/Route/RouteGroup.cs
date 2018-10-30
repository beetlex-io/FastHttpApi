using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class RouteGroup
    {

        public RouteGroup()
        {
            Routes = new List<UrlRoute>();
        }

        public string Ext { get; set; }

        public List<UrlRoute> Routes { get; private set; }


        public bool Match(string url, ref RouteMatchResult result, QueryString queryString)
        {
            for (int i = 0; i < Routes.Count; i++)
            {
                UrlRoute urlRoute = Routes[i];
                if (urlRoute.Match(url, queryString))
                {
                    result.Ext = urlRoute.Ext;
                    result.RewriteUrl = urlRoute.Rewrite;
                    result.RewriteUrlLower = urlRoute.Rewrite;
                    return true;
                }
            }
            return false;
        }

    }
}
