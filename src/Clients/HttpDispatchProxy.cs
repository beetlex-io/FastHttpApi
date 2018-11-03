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

        private System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler> mHandlers = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, ClientActionHanler>();

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            ClientActionHanler handler = GetHandler(targetMethod);
            var request = handler.GetRequest(args);
            Response response;
            switch (request.Method)
            {
                case Request.POST:
                    response = HttpApiClient.Post(request.Url, request.Header, request.QueryString, request.Data, handler.Formater, request.Type);
                    break;
                case Request.PUT:
                    response = HttpApiClient.Put(request.Url, request.Header, request.QueryString, request.Data, handler.Formater, request.Type);
                    break;
                case Request.DELETE:
                    response = HttpApiClient.Delete(request.Url, request.Header, request.QueryString, handler.Formater, request.Type);
                    break;
                default:
                    response = HttpApiClient.Get(request.Url, request.Header, request.QueryString, handler.Formater, request.Type);
                    break;
            }
            return response.Body;
        }

        private ClientActionHanler GetHandler(MethodInfo method)
        {
            ClientActionHanler result;
            if (!mHandlers.TryGetValue(method, out result))
            {
                result = new ClientActionHanler(method);
                mHandlers[method] = result;
            }
            return result;
        }
    }
}
