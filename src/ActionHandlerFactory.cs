using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class ActionHandlerFactory
    {
        static ActionHandlerFactory()
        {

        }

        private System.Collections.Concurrent.ConcurrentDictionary<string, ActionHandler> mMethods = new System.Collections.Concurrent.ConcurrentDictionary<string, ActionHandler>();

        public void Register(HttpConfig config, HttpApiServer server, params Assembly[] assemblies)
        {
            foreach (Assembly item in assemblies)
            {
                foreach (Type type in item.GetTypes())
                {
                    ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
                    if (ca != null)
                    {
                        Register(config, type, ca.BaseUrl, server);
                    }
                }
            }
        }

        public static void RemoveFilter(List<FilterAttribute> filters, Type[] types)
        {
            List<FilterAttribute> removeItems = new List<FilterAttribute>();
            filters.ForEach(a =>
            {
                foreach (Type t in types)
                {
                    if (a.GetType() == t)
                    {
                        removeItems.Add(a);
                        break;
                    }
                }
            });
            foreach (FilterAttribute item in removeItems)
                filters.Remove(item);
        }


        private void Register(HttpConfig config, Type controller, string rooturl, HttpApiServer server)
        {
            if (string.IsNullOrEmpty(rooturl))
                rooturl = "/";
            else
            {
                if (rooturl[0] != '/')
                    rooturl = "/" + rooturl;
                if (rooturl[rooturl.Length - 1] != '/')
                    rooturl += "/";
            }
            List<FilterAttribute> filters = new List<FilterAttribute>();
            filters.AddRange(config.Filters);
            IEnumerable<FilterAttribute> fas = controller.GetCustomAttributes<FilterAttribute>(false);
            filters.AddRange(fas);
            IEnumerable<SkipFilterAttribute> skipfilters = controller.GetCustomAttributes<SkipFilterAttribute>(false);
            foreach (SkipFilterAttribute item in skipfilters)
            {
                RemoveFilter(filters, item.Types);
            }
            object obj = Activator.CreateInstance(controller);
            if (obj is IController)
            {
                ((IController)obj).Init(server);
            }
            foreach (MethodInfo mi in controller.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (string.Compare("Equals", mi.Name, true) == 0
                    || string.Compare("GetHashCode", mi.Name, true) == 0
                    || string.Compare("GetType", mi.Name, true) == 0
                    || string.Compare("ToString", mi.Name, true) == 0)
                    continue;
                string url = rooturl + mi.Name;
                url = url.ToLower();
                ActionHandler handler = GetAction(url);
                if (handler != null)
                {

                    server.Log(EventArgs.LogType.Error, "{0} already exists!duplicate definition {1}.{2}!", url, controller.Name,
                        mi.Name);
                    continue;
                }
                handler = new ActionHandler(obj, mi);
                handler.Filters.AddRange(filters);
                fas = mi.GetCustomAttributes<FilterAttribute>(false);
                handler.Filters.AddRange(fas);
                skipfilters = mi.GetCustomAttributes<SkipFilterAttribute>(false);
                foreach (SkipFilterAttribute item in skipfilters)
                {
                    RemoveFilter(handler.Filters, item.Types);
                }
                mMethods[url] = handler;
                server.Log(EventArgs.LogType.Info, "register {0}.{1} to {2}", controller.Name, mi.Name, url);
            }

        }

        private ActionHandler GetAction(string url)
        {
            ActionHandler result = null;
            mMethods.TryGetValue(url, out result);
            return result;
        }


        public ActionResult ExecuteWithWS(HttpRequest request, HttpApiServer server, JToken token)
        {
            ActionResult result = new ActionResult();
            JToken url = token["url"];
            WebSockets.DataFrame dataFrame = server.CreateDataFrame(result);
            if (url == null)
            {
                result.Code = 403;
                result.Error = "not fupport url info notfound!";
                return result;
            }
            result.Url = url.Value<string>();
            string baseurl = HttpParse.CharToLower(result.Url);
            ActionHandler handler = GetAction(baseurl);
            if (handler == null)
            {
                server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, baseurl + " not found");
                result.Code = 404;
                result.Error = "url " + baseurl + " notfound!";
                request.Session.Send(dataFrame);
            }
            else
            {
                try
                {
                    JToken data = token["params"];
                    if (data == null)
                        data = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject("{}");
                    WebsocketJsonDataContext dc = new WebsocketJsonDataContext(server, request, data);
                    ActionContext context = new ActionContext(handler, dc);
                    context.Execute();
                    if (!dc.AsyncResult)
                    {
                        result.Data = context.Result;
                        request.Session.Send(dataFrame);
                    }
                }
                catch (Exception e_)
                {
                    server.BaseServer.Log(EventArgs.LogType.Error, request.Session, "{0} inner error {1}@{2}", request.Url, e_.Message, e_.StackTrace);
                    result.Code = 500;
                    result.Error = e_.Message;
                    if (server.ServerConfig.OutputStackTrace)
                    {
                        result.StackTrace = e_.StackTrace;
                    }
                    request.Session.Send(dataFrame);
                }
            }
            return result;
        }


        public void Execute(HttpRequest request, HttpResponse response, HttpApiServer server)
        {
            ActionHandler handler = GetAction(request.BaseUrl);
            if (handler == null)
            {
                response.NotFound();
                server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, request.Url + " not found");
            }
            else
            {
                try
                {
                    HttpDataContext pc = new HttpDataContext(server, request, response);
                    ActionContext context = new ActionContext(handler, pc);
                    context.Execute();
                    if (!response.AsyncResult)
                    {
                        object result = context.Result;
                        response.Result(result);
                    }
                }
                catch (Exception e_)
                {
                    response.InnerError(e_, server.ServerConfig.OutputStackTrace);
                    response.Session.Server.Log(EventArgs.LogType.Error, response.Session, "{0} inner error {1}@{2}", request.Url, e_.Message, e_.StackTrace);
                }
            }
        }
    }
}
