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

        public void Register(params Assembly[] assemblies)
        {
            foreach (Assembly item in assemblies)
            {
                foreach (Type type in item.GetTypes())
                {
                    ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
                    if (ca != null)
                    {
                        Register(type, ca.BaseUrl);
                    }
                }
            }
        }


        private void Register(Type controller, string rooturl)
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
            object obj = Activator.CreateInstance(controller);
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
                    throw new BXException(url + " already exists! {0}@{1} duplicate definition!", controller, mi.Name);
                }
                handler = new ActionHandler(obj, mi);
                mMethods[url] = handler;
            }

        }

        private ActionHandler GetAction(string url)
        {
            ActionHandler result = null;
            mMethods.TryGetValue(url.ToLower(), out result);
            return result;
        }

        public void Execute(HttpRequest request, HttpResponse response, HttpApiServer sever)
        {
            ActionHandler handler = GetAction(request.BaseUrl);
            if (handler == null)
            {
                response.NotFound();
                sever.BaseServer.Log(EventArgs.LogType.Warring, request.Session, request.Url + " not found");
            }
            else
            {
                try
                {
                    object result = handler.Invoke(request, response);
                    if (result != null)
                        response.Result(result);
                }
                catch (Exception e_)
                {
                    response.InnerError(e_, sever.ServerConfig.OutputStackTrace);
                    response.Session.Server.Log(EventArgs.LogType.Error, response.Session, "{0} inner error {1}@{2}", request.Url, e_.Message, e_.StackTrace);
                }
            }
        }
    }
}
