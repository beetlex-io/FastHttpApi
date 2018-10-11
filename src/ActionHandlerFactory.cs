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
                Type[] types = item.GetTypes();
                foreach (Type type in types)
                {
                    ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
                    if (ca != null)
                    {
                        Register(config, type, Activator.CreateInstance(type), ca.BaseUrl, server);
                    }
                }
            }
        }

        public void Register(HttpConfig config, HttpApiServer server, object controller)
        {
            Type type = controller.GetType();
            ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
            if (ca != null)
            {
                Register(config, type, controller, ca.BaseUrl, server);
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


        private void Register(HttpConfig config, Type controllerType, object controller, string rooturl, HttpApiServer server)
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
            IEnumerable<FilterAttribute> fas = controllerType.GetCustomAttributes<FilterAttribute>(false);
            filters.AddRange(fas);
            IEnumerable<SkipFilterAttribute> skipfilters = controllerType.GetCustomAttributes<SkipFilterAttribute>(false);
            foreach (SkipFilterAttribute item in skipfilters)
            {
                RemoveFilter(filters, item.Types);
            }
            object obj = controller;
            if (obj is IController)
            {
                ((IController)obj).Init(server);
            }
            foreach (MethodInfo mi in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (string.Compare("Equals", mi.Name, true) == 0
                    || string.Compare("GetHashCode", mi.Name, true) == 0
                    || string.Compare("GetType", mi.Name, true) == 0
                    || string.Compare("ToString", mi.Name, true) == 0 || mi.Name.IndexOf("set_") >= 0
                    || mi.Name.IndexOf("get_") >= 0)
                    continue;
                if (mi.GetCustomAttribute<NotActionAttribute>(false) != null)
                    continue;
                string sourceUrl = rooturl + mi.Name;
                string url = sourceUrl.ToLower();
                ActionHandler handler = GetAction(url);
                if (handler != null)
                {
                    server.Log(EventArgs.LogType.Error, "{0} already exists!duplicate definition {1}.{2}!", url, controllerType.Name,
                        mi.Name);
                    continue;
                }
                handler = new ActionHandler(obj, mi);
                handler.SourceUrl = sourceUrl;
                handler.Filters.AddRange(filters);
                fas = mi.GetCustomAttributes<FilterAttribute>(false);
                handler.Filters.AddRange(fas);
                skipfilters = mi.GetCustomAttributes<SkipFilterAttribute>(false);
                foreach (SkipFilterAttribute item in skipfilters)
                {
                    RemoveFilter(handler.Filters, item.Types);
                }
                mMethods[url] = handler;
                server.Log(EventArgs.LogType.Info, "register {0}.{1} to {2}", controllerType.Name, mi.Name, url);
            }

        }

        private List<Admin.UrlInfo> mUrlsInfo;

        internal List<Admin.UrlInfo> GetUrlInfos()
        {
            if (mUrlsInfo == null)
            {
                mUrlsInfo = new List<Admin.UrlInfo>();
                foreach (ActionHandler handler in mMethods.Values)
                {
                    mUrlsInfo.Add(new Admin.UrlInfo(handler));
                }
                mUrlsInfo.Sort();
            }
            return mUrlsInfo;
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
                if (server.EnableLog(EventArgs.LogType.Warring))
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "websocket {0} not support, url info notfound!", request.ClientIPAddress);
                result.Code = 403;
                result.Error = "not support, url info notfound!";
                request.Session.Send(dataFrame);
                return result;
            }
            result.Url = url.Value<string>();
            string baseurl = HttpParse.CharToLower(result.Url);
            if (baseurl[0] != '/')
                baseurl = "/" + baseurl;
            result.Url = baseurl;
            JToken data = token["params"];
            if (data == null)
                data = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject("{}");
            JToken requestid = data["_requestid"];
            if (requestid != null)
                result.ID = requestid.Value<string>();
            ActionHandler handler = GetAction(baseurl);
            if (handler == null)
            {
                if (server.EnableLog(EventArgs.LogType.Warring))
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "websocket {0} execute {1} notfound", request.ClientIPAddress, result.Url);
                result.Code = 404;
                result.Error = "url " + baseurl + " notfound!";
                request.Session.Send(dataFrame);
            }
            else
            {
                try
                {
                    WebsocketContext dc = new WebsocketContext(server, request, data);
                    dc.ActionUrl = baseurl;
                    dc.RequestID = result.ID;
                    ActionContext context = new ActionContext(handler, dc);
                    context.Execute();
                    if (!dc.AsyncResult)
                    {
                        if (context.Result is ActionResult)
                        {
                            result = (ActionResult)context.Result;
                            result.ID = dc.RequestID;
                            if (result.Url == null)
                                result.Url = dc.ActionUrl;
                            dataFrame.Body = result;
                        }
                        else
                        {
                            result.Data = context.Result;
                        }
                        dataFrame.Send(request.Session);
                        //request.Session.Send(dataFrame);
                    }
                }
                catch (Exception e_)
                {
                    if (server.EnableLog(EventArgs.LogType.Error))
                        server.BaseServer.Log(EventArgs.LogType.Error, request.Session, "websocket {0} execute {1} inner error {2}@{3}", request.ClientIPAddress, request.Url, e_.Message, e_.StackTrace);
                    result.Code = 500;
                    result.Error = e_.Message;
                    if (server.ServerConfig.OutputStackTrace)
                    {
                        result.StackTrace = e_.StackTrace;
                    }
                    dataFrame.Send(request.Session);
                    //request.Session.Send(dataFrame);
                }
            }
            return result;
        }


        public void Execute(HttpRequest request, HttpResponse response, HttpApiServer server)
        {
            ActionHandler handler = GetAction(request.BaseUrl);
            if (handler == null)
            {
                if(server.EnableLog(EventArgs.LogType.Warring))
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} execute {1} action  not found", request.ClientIPAddress, request.Url);
                response.NotFound();
                
            }
            else
            {
                try
                {

                    HttpContext pc = new HttpContext(server, request, response);
                    pc.ActionUrl = request.BaseUrl;
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
                    if (server.EnableLog(EventArgs.LogType.Error))
                        response.Session.Server.Log(EventArgs.LogType.Error, response.Session, "{0} execute {1} action inner error {2}@{3}", request.ClientIPAddress, request.Url, e_.Message, e_.StackTrace);
                }
            }
        }
    }
}
