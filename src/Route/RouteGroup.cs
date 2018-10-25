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


        public bool Match(string url, out string result, out string rewriteLower, QueryString queryString)
        {
            result = null;
            rewriteLower = null;
            for (int i = 0; i < Routes.Count; i++)
            {
                if (Routes[i].Match(url, out result, out rewriteLower, queryString))
                    return true;
            }
            return false;
        }

    }
}
