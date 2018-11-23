using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BeetleX.FastHttpApi.Clients
{
    public class HttpDispatchProxy : System.Reflection.DispatchProxy
    {
        public HttpDispatchProxy()
        {

        }

        public HttpApiClient HttpApiClient { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            return HttpApiClient.Invoke(targetMethod, args);
        }

    }
}
