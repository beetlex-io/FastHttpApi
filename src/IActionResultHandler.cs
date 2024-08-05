using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public interface IActionResultHandler
    {
        void Success(object result);
        void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500);

    }


    class WSActionResultHandler : IActionResultHandler
    {
        public WSActionResultHandler(WebsocketJsonContext jsonContext, HttpApiServer server, HttpRequest request,
             ActionResult result, WebSockets.DataFrame dataFrame, long startTime)
        {
            DataContext = jsonContext;
            Server = server;
            Request = request;
            Result = result;
            DataFrame = dataFrame;
            StartTime = startTime;
        }

        public ActionHandler ActionHandler { get; set; }

        public WebsocketJsonContext DataContext;

        public HttpApiServer Server;

        public HttpRequest Request;

        public ActionResult Result;

        public WebSockets.DataFrame DataFrame;

        public void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500)
        {

            Server.OnActionExecutedError(DataContext, ActionHandler, e_, code, StartTime);
            if (Server.EnableLog(logType))
                Server.Log(logType, Request.Session, $"Websocket {Request.ID} {Request.RemoteIPAddress} execute {DataContext.ActionUrl} inner error {e_.Message}@{e_.StackTrace}");
            Result.Code = code;
            Result.Error = e_.Message;
            if (Server.Options.OutputStackTrace)
            {
                Result.StackTrace = e_.StackTrace;
            }
            DataFrame.Send(Request.Session, true);
        }

        public long StartTime;

        public void Success(object result)
        {
            Server.OnActionExecutedSuccess(DataContext, ActionHandler, StartTime);
            var data = Request.Server.WSActionResultHandler?.Invoke(Request, ActionHandler, result);
            if (data != null)
            {
                DataFrame.Body = data;
            }
            else
            {
                if (result is ActionResult)
                {
                    Result = (ActionResult)result;
                    Result.ID = DataContext.RequestID;
                    if (Result.Url == null)
                        Result.Url = DataContext.ActionUrl;
                    DataFrame.Body = Result;
                }
                else
                {
                    Result.Data = result;
                }
            }
            DataFrame.Send(Request.Session, false);
            if (Server.EnableLog(EventArgs.LogType.Info))
                Server.Log(EventArgs.LogType.Info, Request.Session, $"Websocket {Request.ID} {Request.RemoteIPAddress} execute {DataContext.ActionUrl} action use time:{ Server.BaseServer.GetRunTime() - StartTime}ms");
        }


    }

    class HttpActionResultHandler : IActionResultHandler
    {

        public HttpActionResultHandler(HttpContext context, HttpApiServer server, HttpRequest request, HttpResponse response, long startTime)
        {
            Server = server;
            Request = request;
            Response = response;
            StartTime = startTime;
            Context = context;
        }

        public HttpContext Context;

        public HttpApiServer Server;

        public HttpRequest Request;

        public HttpResponse Response;

        public long StartTime;

        public void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500)
        {
            Server.OnActionExecutedError(Context, Request.ActionHandler, e_, code, StartTime);
            if (Server.EnableLog(logType))
                Server.Log(logType, Request.Session,
                    $"HTTP {Request.ID} {Request.RemoteIPAddress} {Request.Method} { Request.Url} inner error {e_.Message}@{e_.StackTrace}");
            Response.InnerError(code.ToString(), $"http execute {Request.BaseUrl} inner error!", e_, Server.Options.OutputStackTrace);
        }

        public void Success(object result)
        {
            Server.OnActionExecutedSuccess(Context, Request.ActionHandler, StartTime);
            if (Server.EnableLog(EventArgs.LogType.Info))
                Server.BaseServer.Log(EventArgs.LogType.Info, Request.Session,
                    $"HTTP {Request.ID} {Request.RemoteIPAddress} {Request.Method} {Request.BaseUrl} use time:{Server.BaseServer.GetRunTime() - StartTime}ms");
            if (result is FileResult fileResult)
            {
                Server.ResourceCenter.OutputFile(fileResult, Request, Response);
            }
            else
            {
                Response.Result(result);
            }
        }
    }
}
