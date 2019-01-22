using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BeetleX.FastHttpApi.Admin
{
    [Controller(BaseUrl = "/_admin/")]
    [LoginFilter]
    public class _Admin : IController
    {

        public _Admin()
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
                MaxConn = Server.Options.MaxConnections,
                WSMaxRPS = Server.Options.WebSocketMaxRPS,
                LogLevel = Server.Options.LogLevel,
                LogToConsole = Server.Options.LogToConsole,
                WriteLog = Server.Options.WriteLog,
                Exts = string.Join(';', Server.ResourceCenter.Exts.Keys.ToArray()),
                FileManage = Server.Options.FileManager,
                DefaultPages = string.Join(';', Server.ResourceCenter.DefaultPages.ToArray()),
                MaxLength = Server.Options.MaxBodyLength
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
            public string Exts { get; set; }
            public string DefaultPages { get; set; }
            public bool FileManage { get; set; }
            public int MaxLength { get; set; }
        }
        [Post]
        public void Setting(SettingInfo setting, IHttpContext context)
        {
            Server.Options.MaxBodyLength = setting.MaxLength;
            Server.Options.MaxConnections = setting.MaxConn;
            Server.Options.WebSocketMaxRPS = setting.WSMaxRPS;
            Server.Options.LogLevel = setting.LogLevel;
            Server.BaseServer.Options.LogLevel = setting.LogLevel;
            Server.Options.LogToConsole = setting.LogToConsole;
            Server.Options.WriteLog = setting.WriteLog;
            Server.Options.FileManager = setting.FileManage;

            Server.ResourceCenter.SetDefaultPages(setting.DefaultPages);
            Server.ResourceCenter.SetFileExts(setting.Exts);
            Server.SaveOptions();
            if (Server.EnableLog(EventArgs.LogType.Warring))
            {
                Server.BaseServer.Log(EventArgs.LogType.Warring, context.Session, "{0} setting {1}", context.Request.RemoteIPAddress,
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


        [Post]
        public void CloseSession(List<SessionItem> items, IHttpContext context)
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
            return new ServerCounter.ServerStatus();
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
            string vpwd = string.Format("{0}{1}", context.Server.Options.ManagerPWD, context.Request.Cookies[LOGIN_KEY]);
            vpwd = HttpParse.MD5Encrypt(vpwd);
            string vname = HttpParse.MD5Encrypt(context.Server.Options.Manager);
            if (name == vname && pwd == vpwd)
            {
                string ip = context.Request.RemoteIPAddress.Split(':')[0];
                string tokey = HttpParse.MD5Encrypt(context.Server.Options.Manager + DateTime.Now.Day + ip);
                context.Response.SetCookie(LOGIN_TOKEN, tokey, cookieTimeOut);
                context.Response.SetCookie(LOGIN_KEY, "");
                return true;
            }
            return false;
        }
        [NotAction]
        public void Init(HttpApiServer server,string path)
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

        public override bool Executing(ActionContext context)
        {
            string tokey = context.HttpContext.Request.Cookies[_Admin.LOGIN_TOKEN];
            string ip = context.HttpContext.Request.RemoteIPAddress.Split(':')[0];
            string stokey = HttpParse.MD5Encrypt(context.HttpContext.Server.Options.Manager
                + DateTime.Now.Day
                + ip);
            if (tokey == stokey)
            {
                return true;
            }
            else
            {
                ActionResult Result = new ActionResult();
                Result.Code = 403;
                Result.Data = LoginUrl;
                context.Result = Result;
                return false;
            }
        }
    }
}
