using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BeetleX.FastHttpApi.Admin
{
    [Controller(BaseUrl = "/_admin/")]
    [LoginFilter]
    public class AdminController : IController
    {

        public AdminController()
        {

        }

        public static string LOGIN_KEY = "_LOGIN_KEY";

        public static string LOGIN_TOKEN = "_LOGIN_TOKEN";

        public HttpApiServer Server { get; set; }

        internal ActionHandlerFactory HandleFactory { get; set; }

        [Description("获取所有接口信息,需要后台管理权")]
        public object ListApi()
        {
            List<UrlInfo> items = new List<UrlInfo>();
            foreach (ActionHandler item in HandleFactory.Handlers)
            {
                items.Add(new UrlInfo(item));

            }
            return items;
        }
        [Description("获取后台登陆凭证")]
        [SkipFilter(typeof(LoginFilter))]
        public string GetKey(IHttpContext context)
        {
            string key = Guid.NewGuid().ToString("N");
            context.Response.SetCookie(LOGIN_KEY, key);
            return key;

        }

        public object GetSettingInfo()
        {
            return new SettingInfo
            {
                MaxConn = Server.ServerConfig.MaxConnections,
                WSMaxRPS = Server.ServerConfig.WebSocketMaxRPS,
                LogLevel = Server.ServerConfig.LogLevel,
                LogToConsole = Server.ServerConfig.LogToConsole,
                WriteLog = Server.ServerConfig.WriteLog
            };
        }

        public class SettingInfo
        {
            public int MaxConn { get; set; }
            public int WSMaxRPS { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public BeetleX.EventArgs.LogType LogLevel { get; set; }
            public bool LogToConsole { get; set; }
            public bool WriteLog { get; set; }
        }

        public void Setting([BodyParameter] SettingInfo setting, IHttpContext context)
        {
            Server.ServerConfig.MaxConnections = setting.MaxConn;
            Server.ServerConfig.WebSocketMaxRPS = setting.WSMaxRPS;
            Server.ServerConfig.LogLevel = setting.LogLevel;
            Server.BaseServer.Config.LogLevel = setting.LogLevel;
            Server.ServerConfig.LogToConsole = setting.LogToConsole;
            Server.ServerConfig.WriteLog = setting.WriteLog;
            if (Server.EnableLog(EventArgs.LogType.Warring))
            {
                Server.BaseServer.Log(EventArgs.LogType.Warring, context.Session, "{0} setting {1}", context.Request.ClientIPAddress,
                    Newtonsoft.Json.JsonConvert.SerializeObject(setting));
            }
        }

        public void LogConnect(IHttpContext context)
        {
            Server.LogOutput = context.Session;
            ActionResult log = new ActionResult();
            log.Data = new { LogType = BeetleX.EventArgs.LogType.Info.ToString(), Time = DateTime.Now.ToString("H:mm:ss"), Message = "log connect!" };
            context.Server.CreateDataFrame(log).Send(context.Session);
        }

        public void LogDisConnect(IHttpContext context)
        {
            Server.LogOutput = null;
            ActionResult log = new ActionResult();
            log.Data = new { LogType = BeetleX.EventArgs.LogType.Info.ToString(), Time = DateTime.Now.ToString("H:mm:ss"), Message = "log disconnect!" };
            context.Server.CreateDataFrame(log).Send(context.Session);
        }



        public void CloseSession([BodyParameter]List<SessionItem> items, IHttpContext context)
        {
            foreach (SessionItem item in items)
            {
                ISession session = context.Server.BaseServer.GetSession(item.ID);
                if (session != null)
                    session.Dispose();
            }
        }

        public class SessionItem
        {
            public long ID { get; set; }

            public string IPAddress { get; set; }
        }
        [SkipFilter(typeof(LoginFilter))]
        public object GetServerInfo(IHttpContext context)
        {
            if (context.Server.ServerCounter != null)
            {
                return context.Server.ServerCounter.Next();
            }
            return new ServerCounter.Info();
        }

        public object ListConnection(int index, IHttpContext context)
        {
            int size = 20;
            ISession[] sessions = context.Server.BaseServer.GetOnlines();
            int pages = sessions.Length / size;
            if (sessions.Length % size > 0)
                pages++;
            List<object> items = new List<object>();
            for (int i = index * size; i < (index * size + 20) && i < sessions.Length; i++)
            {
                ISession item = sessions[i];
                HttpToken token = (HttpToken)item.Tag;
                items.Add(new
                {
                    item.ID,
                    item.Name,
                    Type = token.WebSocket ? "WebSocket" : "http",
                    CreateTime = DateTime.Now - token.CreateTime,
                    IPAddress = ((System.Net.IPEndPoint)item.RemoteEndPoint).Address.ToString()

                });
            }
            return new { Index = index, Pages = pages, Items = items, context.Server.BaseServer.Count };
        }


        [Description("管理后台登陆")]
        [SkipFilter(typeof(LoginFilter))]
        public bool Login(string name, string pwd, IHttpContext context)
        {
            return LoginProcess(name, pwd, context, null);
        }
        [NotAction]
        public static bool LoginProcess(string name, string pwd, IHttpContext context, DateTime? cookieTimeOut)
        {
            string vpwd = string.Format("{0}{1}", context.Server.ServerConfig.ManagerPWD, context.Request.Cookies[LOGIN_KEY]);
            vpwd = HttpParse.MD5Encrypt(vpwd);
            string vname = HttpParse.MD5Encrypt(context.Server.ServerConfig.Manager);
            if (name == vname && pwd == vpwd)
            {
                string ip = context.Request.ClientIPAddress.Split(':')[0];
                string tokey = HttpParse.MD5Encrypt(context.Server.ServerConfig.Manager + DateTime.Now.Day + ip);
                context.Response.SetCookie(LOGIN_TOKEN, tokey, cookieTimeOut);
                context.Response.SetCookie(LOGIN_KEY, "");
                return true;
            }
            return false;
        }
        [NotAction]
        public void Init(HttpApiServer server)
        {
            this.Server = server;
            this.HandleFactory = Server.ActionFactory;

        }
    }



    public class LoginFilter : FilterAttribute
    {
        public LoginFilter()
        {
            LoginUrl = "/_admin/login.html";
        }

        public string LoginUrl { get; set; }

        public override void Execute(ActionContext context)
        {
            string tokey = context.HttpContext.Request.Cookies[AdminController.LOGIN_TOKEN];
            string ip = context.HttpContext.Request.ClientIPAddress.Split(':')[0];
            string stokey = HttpParse.MD5Encrypt(context.HttpContext.Server.ServerConfig.Manager
                + DateTime.Now.Day
                + ip);
            if (tokey == stokey)
            {
                context.Execute();
            }
            else
            {
                ActionResult Result = new ActionResult();
                Result.Code = 403;
                Result.Data = LoginUrl;
                context.Result = Result;
            }
        }
    }
}
