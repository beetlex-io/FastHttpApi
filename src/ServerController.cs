using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [Controller(BaseUrl = "/")]
    [DefaultJsonResultFilter]
    [AccessTokenFilter]
    class ServerController : IController
    {
        [SkipFilter(typeof(AccessTokenFilter))]
        public object __ServerStatus(IHttpContext context)
        {
            if (context.Server.ServerCounter != null)
            {
                return context.Server.ServerCounter.Next();
            }
            return new ServerCounter.ServerStatus();
        }
        [Post]
        [Data.JsonDataConvert]
        public void __SetSettings(List<Setting> body, IHttpContext context)
        {
            HttpApiServer server = context.Server;
            server.Options.Settings = body;
            server.SaveOptions();
        }
        [Data.JsonDataConvert]
        public void __ChangeAccessKey(string key, IHttpContext context)
        {
            byte[] keyData = Convert.FromBase64String(key);
            HttpApiServer server = context.Server;
            byte[] aseKey = Convert.FromBase64String(server.Options.AccessKey);
            key = Utils.DecryptStringAES(keyData, aseKey, aseKey);
            server.Options.AccessKey = key;
            server.SaveOptions();
        }

        public object __GetSettings(IHttpContext context)
        {
            HttpApiServer server = context.Server;
            return server.Options.Settings;
        }
        [SkipFilter(typeof(AccessTokenFilter))]
        public object __LogConnect(string token, IHttpContext context)
        {
            if (!context.WebSocket)
                return false;
            try
            {
                if (string.IsNullOrEmpty(context.Server.Options.AccessKey))
                {
                    throw new Exception("server access key no defined!");
                }
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("access token is empty!");
                }
                string[] values = token.Split('.');
                string sign = Utils.HmacSha1Sign(Utils.EncryptToSHA1(values[1]), context.Server.Options.AccessKey);
                if (sign != values[0])
                {
                    throw new Exception("sing validation error!");
                }
                string[] times = values[1].Split(':');
                DateTime start = Utils.GetTime(long.Parse(times[0]));
                DateTime end = Utils.GetTime(long.Parse(times[1]));
                DateTime now = DateTime.Now;
                if (now > start && now < end)
                {
                    context.Server.LogOutput = context.Session;
                    return true;
                }
                else
                {
                    throw new Exception("sing expired!");
                }

            }
            catch (Exception e_)
            {
                return new ActionResult(500, e_.Message);
            }
        }

        public object __ListLog(IHttpContext context)
        {
            HttpApiServer server = context.Server;
            return server.GetCacheLog();
        }

        public object __GetOptions(IHttpContext context)
        {
            HttpApiServer server = context.Server;
            return new
            {
                server.Options.StaticResurceType,
                server.Options.SessionTimeOut,
                server.Options.MaxBodyLength,
                server.Options.LogLevel,
                server.Options.WebSocketMaxRPS,
                server.Options.MaxConnections,
                server.Options.WriteLog,
                server.Options.LogToConsole,
                server.Options.DefaultPage,
                server.Options.CacheLogLength
            };
        }



        [Post]
        [Data.JsonDataConvert]
        public void __UploadModule(string name, string md5, bool eof, string base64Data, IHttpContext context)
        {
            HttpApiServer server = context.Server;
            server.ModuleManager.SaveFile(name, md5, eof, Convert.FromBase64String(base64Data));
        }

        [Post]
        [Data.JsonDataConvert]
        public void __SetActionMaxrps(List<SetMaxrps> body, IHttpContext context)
        {
            HttpApiServer server = context.Server;
            if (body != null)
            {
                foreach (var i in body)
                {
                    foreach (var item in server.ActionFactory.Handlers)
                    {
                        if (item.ID == i.ID)
                        {
                            item.MaxRPS = i.Value;
                        }
                    }
                }
                server.Options.MaxrpsSettings.Clear();
                foreach (var item in server.ActionFactory.Handlers)
                {
                    server.Options.MaxrpsSettings.Add(new ActionMaxrps { Url = item.SourceUrl, MaxRps = item.MaxRPS });
                }
                server.SaveOptions();
            }
        }
        [Post]
        [Data.JsonDataConvert]
        public void __SetOptions(ServerSetting body, IHttpContext context)
        {
            HttpApiServer server = context.Server;
            server.BaseServer.Options.LogLevel = body.LogLevel;
            server.Options.StaticResurceType = body.StaticResurceType;
            server.Options.SessionTimeOut = body.SessionTimeOut;
            server.Options.MaxBodyLength = body.MaxBodyLength;
            server.Options.LogLevel = body.LogLevel;
            server.Options.WebSocketMaxRPS = body.WebSocketMaxRPS;
            server.Options.MaxConnections = body.MaxConnections;
            server.Options.WriteLog = body.WriteLog;
            server.Options.LogToConsole = body.LogToConsole;
            server.Options.CacheLogLength = body.CacheLogLength;
            server.ResourceCenter.SetDefaultPages(body.DefaultPage);
            server.ResourceCenter.SetFileExts(body.StaticResurceType);
            server.SaveOptions();
        }
        [NotAction]
        public void Init(HttpApiServer server, string path)
        {

        }
    }

    class ServerSetting
    {
        public bool LogToConsole { get; set; }
        public string StaticResurceType { get; set; }
        public string DefaultPage { get; set; }
        public int SessionTimeOut { get; set; }
        public int MaxBodyLength { get; set; }
        public LogType LogLevel { get; set; }
        public int WebSocketMaxRPS { get; set; }
        public int MaxConnections { get; set; }
        public bool WriteLog { get; set; }
        public int CacheLogLength { get; set; }
    }

    class SetMaxrps
    {
        public int ID { get; set; }

        public int Value { get; set; }
    }

    /// <summary>
    /// sing(sh1(start:end),key).start:end;
    /// </summary>
    public class AccessTokenFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            HttpRequest reqeust = context.HttpContext.Request;
            string token = context.HttpContext.Request.Header[HeaderTypeFactory.AUTHORIZATION];
            if (string.IsNullOrEmpty(reqeust.Server.Options.AccessKey))
            {
                context.Result = new UnauthorizedResult("server access key no defined!");
                return false;
            }
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult("access token is empty!");
                return false;
            }
            string[] values = token.Split('.');
            string sign = Utils.HmacSha1Sign(Utils.EncryptToSHA1(values[1]), reqeust.Server.Options.AccessKey);
            if (sign != values[0])
            {
                context.Result = new UnauthorizedResult("sing validation error!");
                return false;
            }
            string[] times = values[1].Split(':');
            DateTime start = Utils.GetTime(long.Parse(times[0]));
            DateTime end = Utils.GetTime(long.Parse(times[1]));
            DateTime now = DateTime.Now;
            if (now > start && now < end)
                return true;
            else
            {
                context.Result = new UnauthorizedResult("sing expired!");
                return false;
            }
        }
    }
}
