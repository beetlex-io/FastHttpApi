using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Route
{
    class RouteGroup
    {

        public RouteGroup()
        {
            Routes = new List<UrlRoute>();
        }

        public string Ext { get; set; }

        public List<UrlRoute> Routes { get; private set; }


        public bool Match(string url, out string result)
        {
            result = null;
            for (int i = 0; i < Routes.Count; i++)
            {
                if (Routes[i].Match(url, out result))
                    return true;
            }
            return false;
        }

    }
}
