using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public partial class HttpApiServer
    {
        private Dictionary<string, Func<IHttpContext, object>> mMapUrlAction = new Dictionary<string, Func<IHttpContext, object>>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, Func<IHttpContext, Task<object>>> mMapUrlTaskAction = new Dictionary<string, Func<IHttpContext, Task<object>>>(StringComparer.OrdinalIgnoreCase);

        private string OnGetMapURL(string route)
        {
            var ra = new RouteTemplateAttribute(route);
            if (route[0] != '/')
                route = '/' + route;
            var reurl = ra.Analysis(null);
            string url = route;
            if (!string.IsNullOrEmpty(reurl))
            {
                url = route.Substring(0, route.IndexOf("{"));
                if (url[url.Length - 1] == '/')
                {
                    url = url.Substring(0, url.Length - 1);
                }
                UrlRewrite.Add(null, reurl, url);
            }
            return url;
        }

        private bool OnExecuteMap(HttpRequest request)
        {
            if (mMapUrlAction.Count == 0 && mMapUrlTaskAction.Count == 0)
                return false;
            HttpContext context = new HttpContext(this, request, request.Response, request.Data);
            if (mMapUrlAction.TryGetValue(request.BaseUrl, out Func<IHttpContext, object> hanlder))
            {
                OnExecuteMapAction(hanlder, context);
                return true;
            }
            if (mMapUrlTaskAction.TryGetValue(request.BaseUrl, out Func<IHttpContext, Task<object>> action))
            {
                OnExecuteMapFun(action, context);
                return true;
            }
            return false;
        }

        private void OnExecuteMapAction(Func<IHttpContext, object> action, IHttpContext context)
        {
            try
            {
                var result = action(context);
                context.Result(result);
            }
            catch (Exception e_)
            {
                //context.Result(new InnerErrorResult("500", e_, this.Options.OutputStackTrace));
                context.Response.InnerError("500","execute map action error!", e_, this.Options.OutputStackTrace);
                GetLog(EventArgs.LogType.Error)?.Log(EventArgs.LogType.Error, context.Session, $"HTTP Map {context.Request.BaseUrl} execute error {e_.Message} {e_.StackTrace}");
            }
        }

        private async void OnExecuteMapFun(Func<IHttpContext, Task<object>> action, IHttpContext context)
        {
            try
            {
                var result = await action(context);
                context.Result(result);
            }
            catch (Exception e_)
            {
                //context.Result(new InnerErrorResult("500", e_, this.Options.OutputStackTrace));
                context.Response.InnerError("500", "execute map action error!", e_, this.Options.OutputStackTrace);
                GetLog(EventArgs.LogType.Error)?.Log(EventArgs.LogType.Error, context.Session, $"HTTP Map {context.Request.BaseUrl} execute error {e_.Message} {e_.StackTrace}");
            }
        }

        public HttpApiServer Map(string url, Func<IHttpContext, object> action)
        {
            var mapurl = OnGetMapURL(url);
            mMapUrlAction[mapurl] = action;
            return this;
        }

        public HttpApiServer Map(string url, Func<IHttpContext, Task<object>> action)
        {
            var mapurl = OnGetMapURL(url);
            mMapUrlTaskAction[mapurl] = action;
            return this;
        }
    }
}
