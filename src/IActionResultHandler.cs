using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IActionResultHandler
    {
        void Success(object result);
        void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500);
    }

    struct WSActionResultHandler : IActionResultHandler
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

        public WebsocketJsonContext DataContext;

        public HttpApiServer Server;

        public HttpRequest Request;

        public ActionResult Result;

        public WebSockets.DataFrame DataFrame;

        public void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500)
        {
            if (Server.EnableLog(logType))
                Server.Log(logType, "{0} ws execute {1} inner error {2}@{3}", Request.RemoteIPAddress, Result.Url, e_.Message, e_.StackTrace);
            Result.Code = code;
            Result.Error = e_.Message;
            if (Server.Options.OutputStackTrace)
            {
                Result.StackTrace = e_.StackTrace;
            }
            DataFrame.Send(Request.Session);
        }

        public long StartTime;

        public void Success(object result)
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
            DataFrame.Send(Request.Session);
            if (Server.EnableLog(EventArgs.LogType.Info))
                Server.Log(EventArgs.LogType.Info, "{0} ws execute {1} action use time:{2}ms", Request.RemoteIPAddress,
                    DataContext.ActionUrl, Server.BaseServer.GetRunTime() - StartTime);
        }
    }

    struct HttpActionResultHandler : IActionResultHandler
    {

        public HttpActionResultHandler(HttpApiServer server, HttpRequest request, HttpResponse response, long startTime)
        {
            Server = server;
            Request = request;
            Response = response;
            StartTime = startTime;
        }

        public HttpApiServer Server;

        public HttpRequest Request;

        public HttpResponse Response;

        public long StartTime;

        public void Error(Exception e_, EventArgs.LogType logType = EventArgs.LogType.Error, int code = 500)
        {
            if (Server.EnableLog(logType))
                Server.Log(logType,
                    $"{Request.RemoteIPAddress} http {Request.Method} { Request.Url} inner error {e_.Message}@{e_.StackTrace}");
            InnerErrorResult result = new InnerErrorResult($"http execute {Request.BaseUrl} error ", e_, Server.Options.OutputStackTrace);
            result.Code = code.ToString();
            Response.Result(result);
        }

        public void Success(object result)
        {
            if (Server.EnableLog(EventArgs.LogType.Info))
                Server.BaseServer.Log(EventArgs.LogType.Info, Request.Session,
                    $"{Request.RemoteIPAddress} http {Request.Method} {Request.BaseUrl} use time:{Server.BaseServer.GetRunTime() - StartTime}ms");
            Response.Result(result);
        }
    }
}
