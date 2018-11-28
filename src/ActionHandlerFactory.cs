using BeetleX.FastHttpApi.Data;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public class ActionHandlerFactory
    {
        public ActionHandlerFactory(HttpApiServer server)
        {
            Server = server;
        }

        public HttpApiServer Server { get; set; }

        private System.Collections.Generic.Dictionary<string, ActionHandler> mMethods = new Dictionary<string, ActionHandler>();

        public void Register(params Assembly[] assemblies)
        {
            foreach (Assembly item in assemblies)
            {
                Type[] types = item.GetTypes();
                foreach (Type type in types)
                {
                    ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
                    if (ca != null)
                    {
                        try
                        {
                            EventControllerInstanceArgs e = new EventControllerInstanceArgs();
                            e.Type = type;
                            OnControllerInstance(e);
                            if (e.Controller == null)
                            {
                                Register(Server.ServerConfig, type, Activator.CreateInstance(type), ca.BaseUrl, Server, ca);
                            }
                            else
                            {
                                Register(Server.ServerConfig, type, e.Controller, ca.BaseUrl, Server, ca);
                            }
                        }
                        catch (Exception e_)
                        {
                            if (Server.EnableLog(EventArgs.LogType.Error))
                            {
                                string msg = $"{type} controller register error {e_.Message} {e_.StackTrace}";
                                Server.Log(EventArgs.LogType.Error, msg);
                            }
                        }
                    }
                }
            }
        }

        public object GetController(Type type)
        {
            EventControllerInstanceArgs e = new EventControllerInstanceArgs();
            e.Type = type;
            OnControllerInstance(e);
            return e.Controller;
        }

        private void AddHandlers(string url, ActionHandler handler)
        {
            lock (mMethods)
            {
                mMethods[url] = handler;
            }
        }

        protected virtual void OnControllerInstance(EventControllerInstanceArgs e)
        {
            ControllerInstance?.Invoke(this, e);
        }

        public event System.EventHandler<EventControllerInstanceArgs> ControllerInstance;

        public void Clear()
        {
            lock (mMethods)
            {
                mMethods.Clear();
            }
        }

        public void Remove(string assemblyName)
        {
            foreach (ActionHandler item in Handlers)
            {
                if (item.AssmblyName == assemblyName)
                {
                    Remove(item);
                }
            }
        }

        public void Remove(ActionHandler handler)
        {
            if (handler != null)
            {
                lock (mMethods)
                {
                    if (mMethods.ContainsKey(handler.Url))
                    {
                        mMethods.Remove(handler.Url);
                        if (Server.EnableLog(EventArgs.LogType.Info))
                        {
                            Server.Log(EventArgs.LogType.Info, $"remove {handler.Url} action handler");
                        }
                    }

                }
            }
        }

        public ActionHandler[] Handlers
        {
            get
            {
                lock (mMethods)
                {
                    ActionHandler[] result = new ActionHandler[mMethods.Values.Count];
                    mMethods.Values.CopyTo(result, 0);
                    return result;
                }
            }
        }

        public void Register(object controller)
        {
            Type type = controller.GetType();
            ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
            if (ca != null)
            {
                Register(this.Server.ServerConfig, type, controller, ca.BaseUrl, this.Server, ca);
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

        private void Register(HttpConfig config, Type controllerType, object controller, string rooturl, HttpApiServer server, ControllerAttribute ca)
        {
            DataConvertAttribute controllerDataConvert = controllerType.GetCustomAttribute<DataConvertAttribute>(false);
            OptionsAttribute controllerOptionsAttribute = controllerType.GetCustomAttribute<OptionsAttribute>(false);
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
                bool noconvert = false;
                DataConvertAttribute actionConvert = mi.GetCustomAttribute<DataConvertAttribute>();
                OptionsAttribute methodOptionsAttribute = mi.GetCustomAttribute<OptionsAttribute>();
                if (mi.GetCustomAttribute<NoDataConvertAttribute>(false) != null)
                {
                    noconvert = true;
                    actionConvert = null;
                }
                else
                {
                    if (actionConvert == null)
                        actionConvert = controllerDataConvert;
                }
                string sourceUrl = rooturl + mi.Name;
                string url = sourceUrl;
                string method = HttpParse.GET_TAG;
                string route = null;
                GetAttribute get = mi.GetCustomAttribute<GetAttribute>(false);
                if (get != null)
                {
                    method = HttpParse.GET_TAG;
                    route = get.Route;
                }
                PostAttribute post = mi.GetCustomAttribute<PostAttribute>(false);
                if (post != null)
                {
                    method = HttpParse.POST_TAG;
                    route = post.Route;
                }
                DelAttribute del = mi.GetCustomAttribute<DelAttribute>(false);
                if (del != null)
                {
                    method = HttpParse.DELETE_TAG;
                    route = del.Route;
                }
                PutAttribute put = mi.GetCustomAttribute<PutAttribute>(false);
                if (put != null)
                {
                    method = HttpParse.PUT_TAG;
                    route = put.Route;
                }

                if (server.ServerConfig.UrlIgnoreCase)
                {
                    url = sourceUrl.ToLower();
                }
                RouteTemplateAttribute ra = null;
                if (!string.IsNullOrEmpty(route))
                {
                    ra = new RouteTemplateAttribute(route);
                    string reurl = ra.Analysis(url);
                    if (reurl != null)
                        server.UrlRewrite.Add(reurl, url);
                }
                ActionHandler handler = GetAction(url);
                if (handler != null)
                {
                    server.Log(EventArgs.LogType.Warring, "{0} already exists!replaced with {1}.{2}!", url, controllerType.Name,
                        mi.Name);
                }

                handler = new ActionHandler(obj, mi);
                if (mi.ReturnType == typeof(Task) || mi.ReturnType.BaseType == typeof(Task))
                {
                    handler.Async = true;
                    PropertyInfo pi = mi.ReturnType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                    if (pi != null)
                        handler.PropertyHandler = new PropertyHandler(pi);

                }
                handler.Path = rooturl;
                if (methodOptionsAttribute == null)
                    handler.OptionsAttribute = controllerOptionsAttribute;
                else
                    handler.OptionsAttribute = methodOptionsAttribute;
                handler.NoConvert = noconvert;
                handler.SingleInstance = ca.SingleInstance;
                handler.DataConvert = actionConvert;
                handler.Route = ra;
                handler.Method = method;
                handler.SourceUrl = sourceUrl;
                handler.Filters.AddRange(filters);
                fas = mi.GetCustomAttributes<FilterAttribute>(false);
                handler.Filters.AddRange(fas);
                handler.Url = url;
                skipfilters = mi.GetCustomAttributes<SkipFilterAttribute>(false);
                foreach (SkipFilterAttribute item in skipfilters)
                {
                    RemoveFilter(handler.Filters, item.Types);
                }
                AddHandlers(url, handler);
                server.Log(EventArgs.LogType.Info, "register {0}.{1} to {2}", controllerType.Name, mi.Name, url);
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
                if (server.EnableLog(EventArgs.LogType.Warring))
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} ws not support, url info notfound!", request.ClientIPAddress);
                result.Code = 403;
                result.Error = "not support, url info notfound!";
                request.Session.Send(dataFrame);
                return result;
            }
            result.Url = url.Value<string>();
            string baseurl = result.Url;
            if (server.ServerConfig.UrlIgnoreCase)
                baseurl = HttpParse.CharToLower(result.Url);
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
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} ws execute {1} notfound", request.ClientIPAddress, result.Url);
                result.Code = 404;
                result.Error = "url " + baseurl + " notfound!";
                request.Session.Send(dataFrame);
            }
            else
            {
                try
                {
                    Data.DataContxt dataContxt = new Data.DataContxt();
                    DataContextBind.BindJson(dataContxt, data);
                    WebsocketJsonContext dc = new WebsocketJsonContext(server, request, dataContxt);
                    dc.ActionUrl = baseurl;
                    dc.RequestID = result.ID;
                    ActionContext context = new ActionContext(handler, dc, this);
                    long startTime = server.BaseServer.GetRunTime();
                    WSActionResultHandler wSActionResultHandler = new WSActionResultHandler(dc, server, request, result, dataFrame, startTime);
                    ExecuteActionContext(context, wSActionResultHandler);
                }
                catch (Exception e_)
                {
                    if (server.EnableLog(EventArgs.LogType.Error))
                        server.BaseServer.Log(EventArgs.LogType.Error, request.Session, "{0} ws execute {1} inner error {2}@{3}", request.ClientIPAddress, request.Url, e_.Message, e_.StackTrace);
                    result.Code = 500;
                    result.Error = e_.Message;
                    if (server.ServerConfig.OutputStackTrace)
                    {
                        result.StackTrace = e_.StackTrace;
                    }
                    dataFrame.Send(request.Session);
                }
            }
            return result;
        }

        public void Execute(HttpRequest request, HttpResponse response, HttpApiServer server)
        {
            ActionHandler handler = GetAction(request.BaseUrl);
            if (handler == null)
            {
                if (server.EnableLog(EventArgs.LogType.Warring))
                    server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"{request.ClientIPAddress} {request.Method} {request.Url}  not found");
                if (!server.OnHttpRequesNotfound(request, response).Cancel)
                {
                    NotFoundResult notFoundResult = new NotFoundResult($"{request.Method} {request.Url} not found");
                    response.Result(notFoundResult);
                }
            }
            else
            {
                try
                {
                    if (request.Method != handler.Method)
                    {
                        if (request.Method == HttpParse.OPTIONS_TAG && handler.OptionsAttribute != null)
                        {
                            if (server.EnableLog(EventArgs.LogType.Info))
                                server.BaseServer.Log(EventArgs.LogType.Info, request.Session, $"{request.ClientIPAddress}{request.Method}{request.Url} request");
                            response.Result(handler.OptionsAttribute);
                        }
                        else
                        {
                            if (server.EnableLog(EventArgs.LogType.Warring))
                                server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"{request.ClientIPAddress}{request.Method} {request.Url} not support");
                            NotSupportResult notSupportResult = new NotSupportResult($"{request.Method}{request.Url} not support");
                            response.Result(notSupportResult);
                        }
                        return;
                    }
                    if (!handler.NoConvert && handler.DataConvert == null)
                    {
                        handler.DataConvert = DataContextBind.GetConvertAttribute(request.ContentType);
                    }
                    if (!handler.NoConvert)
                        handler.DataConvert.Execute(request.Data, request);
                    HttpContext pc = new HttpContext(server, request, response, request.Data);
                    long startTime = server.BaseServer.GetRunTime();
                    pc.ActionUrl = request.BaseUrl;
                    HttpActionResultHandler actionResult = new HttpActionResultHandler(Server, request, response, startTime);
                    ActionContext context = new ActionContext(handler, pc, this);
                    if (handler.OptionsAttribute != null)
                        handler.OptionsAttribute.SetResponse(request, response);
                    ExecuteActionContext(context, actionResult);
                }
                catch (Exception e_)
                {
                    if (server.EnableLog(EventArgs.LogType.Error))
                        server.Log(EventArgs.LogType.Error, $"{request.ClientIPAddress} http {request.Method} { request.Url} inner error {e_.Message}@{e_.StackTrace}");
                    InnerErrorResult result = new InnerErrorResult($"http execute {request.BaseUrl} error ", e_, server.ServerConfig.OutputStackTrace);
                    response.Result(result);

                }
            }
        }

        private async void ExecuteActionContext(ActionContext context, IActionResultHandler handler)
        {
            try
            {
                context.Execute();
                var result = context.Result;
                if (result is Task task)
                {
                    await task;
                    if (context.Handler.PropertyHandler != null)
                    {
                        result = context.Handler.PropertyHandler.Get(task);
                    }
                    else
                    {
                        result = null;
                    }
                    handler.Success(result);
                }
                else if (result is Exception error)
                {
                    handler.Error(error);
                }
                else
                {
                    handler.Success(result);
                }
            }
            catch (Exception e_)
            {
                handler.Error(e_);
            }
        }
    }
}
