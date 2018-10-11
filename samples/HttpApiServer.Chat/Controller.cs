using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeetleX.FastHttpApi.WebSockets;
using System.ComponentModel;

namespace HttpApiServer.Chat
{
    [Controller]
    public class Controller : IController
    {
        /// <summary>
        ///  获取在线人数
        /// </summary>
        /// <returns>{ID, Name, IPAddress}</returns>
        [Description("获取所有在线人数")]
        public object Onlines(IHttpContext context)
        {
            return from i in context.Server.GetWebSockets()
                   select new { i.Session.ID, i.Session.Name, IPAddress = i.Session.RemoteEndPoint.ToString() };

        }
        /// <summary>
        /// 获取房间在线人数
        /// </summary>
        /// <param name="roomName">房间名称</param>
        /// <returns>{ID, Name, IPAddress}</returns>
        [Description("获取指定房间的在线人数")]
        public object GetRoomOnlines(string roomName, IHttpContext context)
        {
            Room room;
            if (mRooms.TryGetValue(roomName, out room))
            {
                return from i in room.Sessions
                       select new { i.Session.ID, i.Session.Name, IPAddress = i.Session.RemoteEndPoint.ToString() };
            }
            return new BeetleX.ISession[0];
        }

        private System.Collections.Concurrent.ConcurrentDictionary<string, Room> mRooms = new System.Collections.Concurrent.ConcurrentDictionary<string, Room>();

        private List<BeetleX.ISession> mAdminList = new List<BeetleX.ISession>();

        private BeetleX.FastHttpApi.HttpApiServer mServer;
        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>true|false</returns>
        [Description("用户登陆")]
        public bool Login(string userName, IHttpContext context)
        {
            context.Session.Name = userName;
            if (context.Session.Name == "admin")
            {
                lock (mAdminList)
                    mAdminList.Add(context.Session);
            }
            return true;
        }
        /// <summary>
        /// 获取所有房间信息
        /// </summary>
        /// <returns>{Name,Count}</returns>
        [Description("获取所有房间信息")]
        public object ListRooms()
        {
            return from r in mRooms.Values select new { r.Name, r.Sessions.Count };
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="sessions">[id1,id2,id3]</param>
        [Description("关闭连接")]
        public void CloseSession([BodyParameter]List<int> sessions, IHttpContext context)
        {
            foreach (int i in sessions)
            {
                BeetleX.ISession session = context.Server.BaseServer.GetSession(i);
                if (session != null)
                    session.Dispose();
            }
        }
        /// <summary>
        /// 关闭房间
        /// </summary>
        /// <param name="roomName">房间名称</param>
        [Description("关闭房间")]
        public void CloseRoom(string roomName, IHttpContext context)
        {
            Room room;
            if (mRooms.Remove(roomName, out room))
            {
                room.Sessions.Clear();
                Command cmd = new Command();
                cmd.Type = "Delete";
                cmd.Room = roomName;
                context.SendToWebSocket(new ActionResult(cmd));
            }
        }
        /// <summary>
        /// 退出房间
        /// </summary>
        /// <param name="roomName">房间名称</param>
        /// <returns>{Code:200,Error}</returns>
        [Description("退出房间")]
        public object CheckOutRoom(string roomName, IHttpContext context)
        {
            Room room;
            if (mRooms.TryGetValue(roomName, out room))
            {
                room.CheckOut(context);
            }
            else
            {
                return new ActionResult(404, "房间不存在");
            }
            return true;
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="roomName">进入房间</param>
        /// <returns>{Code:200,Error}</returns>
        [Description("进入房间")]
        public object CheckInRoom(string roomName, IHttpContext context)
        {
            Room room;
            if (mRooms.TryGetValue(roomName, out room))
            {
                room.CheckIn(context);
            }
            else
            {
                return new ActionResult(404, "房间不存在");
            }
            return true;

        }
        /// <summary>
        /// 创建记房间
        /// </summary>
        /// <param name="roomName">房间名称</param>
        /// <returns>{Code:200,Error}</returns>
        [Description("创建房间")]
        public object CreateRoom(string roomName, IHttpContext context)
        {
            roomName = roomName.ToLower();
            if (mRooms.Count > 200)
            {
                return new ActionResult(503, "房间已经满，不能再创建");
            }
            if (mRooms.ContainsKey(roomName))
            {
                return new ActionResult(504, "房间已经存在");
            }
            Room room = new Room();
            room.Name = roomName;
            room.Controller = this;
            mRooms[room.Name] = room;
            context.SendToWebSocket(new ActionResult(new Command { Type = "CreateRoom", Name = roomName }));
            return true;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息内容</param>
        [Description("发送消息")]
        public void SendMessage(string message, IHttpContext context)
        {
            string name;
            if (context.WebSocket)
            {
                name = context.Session.Name;
            }
            else
            {
                name = "http user";
            }
            Room room = GetRoom(context.Session);
            if (room != null)
            {
                Command cmd = room.Talk(name, message, context);
            }
        }
        [NotAction]
        public void SendToAdmin(Command cmd)
        {
            ActionResult result = new ActionResult { Data = cmd };
            DataFrame frame = mServer.CreateDataFrame(result);
            lock (mAdminList)
                foreach (BeetleX.ISession session in mAdminList)
                {
                    frame.Send(session);
                }
        }

        private Room GetRoom(BeetleX.ISession session)
        {
            string room = (string)session["room"];
            if (!string.IsNullOrEmpty(room))
            {
                Room result;
                mRooms.TryGetValue(room, out result);
                result.Controller = this;
                return result;
            }
            return null;
        }

        private void OnHttpDisconnect(object sender, BeetleX.EventArgs.SessionEventArgs e)
        {
            BeetleX.ISession session = e.Session;
            HttpToken token = (HttpToken)e.Session.Tag;
            if (session.Name != null && token != null)
            {
                Room room = GetRoom(e.Session);
                room?.CheckOut(token.WebSocketRequest, mServer);
            }
            lock (mAdminList)
                mAdminList.Remove(session);
        }

        private void OnHttpConnected(object sender, BeetleX.EventArgs.ConnectedEventArgs e)
        {

        }
        [NotAction]
        public void Init(BeetleX.FastHttpApi.HttpApiServer server)
        {
            server.HttpConnected += OnHttpConnected;
            server.HttpDisconnect += OnHttpDisconnect;
            mServer = server;
        }
        public class Command
        {
            public string Room { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public string Message { get; set; }
        }

        public class Room : IComparable
        {

            public Controller Controller
            {
                get; set;
            }

            public Room()
            {
                Sessions = new List<HttpRequest>();

            }


            public string Name { get; set; }

            public List<HttpRequest> Sessions { get; private set; }

            public int CompareTo(object obj)
            {
                return Name.CompareTo(((Room)obj).Name);
            }

            public Command Talk(string username, string message, IHttpContext context)
            {
                Command cmd = new Command();
                cmd.Type = "Talk";
                cmd.Message = message;
                cmd.Room = Name;
                cmd.Name = username;
                SendMessage(cmd, context.Server);
                Controller.SendToAdmin(cmd);
                return cmd;
            }


            public void CheckIn(IHttpContext context)
            {
                if (!Sessions.Contains(context.Request))
                {
                    Command cmd = new Command();
                    cmd.Type = "CheckIn";
                    cmd.Room = Name;
                    cmd.Name = context.Session.Name;
                    context.Session["room"] = this.Name;
                    lock (this.Sessions)
                        Sessions.Add(context.Request);
                    SendMessage(cmd, context.Server);
                    Controller.SendToAdmin(cmd);
                }
            }

            public void CheckOut(IHttpContext context)
            {
                Command cmd = new Command();
                cmd.Type = "CheckOut";
                cmd.Room = Name;
                cmd.Name = context.Session.Name;
                lock (this.Sessions)
                    Sessions.Remove(context.Request);
                SendMessage(cmd, context.Server);
                Controller.SendToAdmin(cmd);
            }


            public void CheckOut(HttpRequest request, BeetleX.FastHttpApi.HttpApiServer server)
            {
                ActionResult result = new ActionResult();
                result.Url = "/CheckOutRoom";
                Command cmd = new Command();
                cmd.Type = "CheckOut";
                cmd.Room = Name;
                cmd.Name = request.Session.Name;
                result.Data = cmd;
                lock (this.Sessions)
                    Sessions.Remove(request);
                SendMessage(result, server);
                Controller.SendToAdmin(cmd);

            }
            private void SendMessage(Command msg, BeetleX.FastHttpApi.HttpApiServer server)
            {
                ActionResult ar = new ActionResult { Data = msg };
                SendMessage(ar, server);
            }
            private void SendMessage(ActionResult msg, BeetleX.FastHttpApi.HttpApiServer server)
            {
                DataFrame df = server.CreateDataFrame(msg);
                lock (this.Sessions)
                    for (int i = 0; i < Sessions.Count; i++)
                    {
                        df.Send(Sessions[i].Session);
                    }
            }
        }

    }
}
