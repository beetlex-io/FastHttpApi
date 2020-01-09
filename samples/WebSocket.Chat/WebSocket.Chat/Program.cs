using BeetleX;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;

namespace WebSocket.Chat
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.UseBeetlexHttp(o =>
                    {
                        o.LogToConsole = true;
                        o.ManageApiEnabled = false;
                        o.Port = 80;
                        o.SetDebug();
                        o.LogLevel = BeetleX.EventArgs.LogType.Warring;
                    },
                    typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
    }

    [BeetleX.FastHttpApi.Controller]
    public class Home : BeetleX.FastHttpApi.IController
    {
        [BeetleX.FastHttpApi.NotAction]
        public void Init(HttpApiServer server, string path)
        {
            for (int i = 0; i < 10; i++)
            {
                string key = $"{i:00}";
                mRooms[key] = new Room { Name = key, HttpServer = server };
            }
            server.HttpDisconnect += (o, e) =>
            {
                GetUser(e.Session)?.Exit();
            };
        }

        private ConcurrentDictionary<string, Room> mRooms = new ConcurrentDictionary<string, Room>(StringComparer.OrdinalIgnoreCase);

        public object Rooms()
        {
            return from a in mRooms.Values orderby a.Name select new {a.Name};
        }

        public void Enter(string room, IHttpContext context)
        {
            User user = GetUser(context.Session);
            mRooms.TryGetValue(room, out Room result);
            result?.Enter(user);
        }

        public void Talk(string message, IHttpContext context)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var user = GetUser(context.Session);
                Command cmd = new Command { Type = "talk", Message = message, User = user };
                user?.Room?.Send(cmd);
            }
        }
        public void Login(string name, IHttpContext context)
        {
            User user = new User();
            user.Name = name;
            user.Session = context.Session;
            user.Address = context.Request.RemoteIPAddress;
            SetUser(context.Session, user);
        }
        private User GetUser(ISession session)
        {
            return (User)session["__user"];
        }

        private void SetUser(ISession session, User user)
        {
            session["__user"] = user;
        }
    }

    public class Command
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public User User { get; set; }

        public string Room { get; set; }
    }



    public class User
    {
        public string Name { get; set; }

        public string Address { get; set; }

        [JsonIgnore]
        public ISession Session { get; set; }

        [JsonIgnore]
        public Room Room { get; set; }

        public void Send(BeetleX.FastHttpApi.WebSockets.DataFrame frame)
        {
            frame.Send(Session);
        }

        public void Exit()
        {
            Room?.Exit(this);
        }
    }

    public class Room
    {

        public string Name { get; set; }

        public List<User> Users { get; private set; } = new List<User>();

        public HttpApiServer HttpServer { get; set; }

        public void Send(Command cmd)
        {
            cmd.Room = Name;
            var frame = HttpServer.CreateDataFrame(cmd);
            lock (Users)
            {
                foreach (var item in Users)
                    item.Send(frame);
            }

        }

        public User[] GetOnlines()
        {
            lock (Users)
                return Users.ToArray();
        }

        public void Enter(User user)
        {
            if (user == null)
                return;
            if (user.Room != this)
            {
                user.Room?.Exit(user);
                lock (Users)
                    Users.Add(user);
                user.Room = this;
                Command quit = new Command { Type = "enter",Message=$"enter room", User = user };
                Send(quit);
            }
        }

        public void Exit(User user)
        {
            if (user == null)
                return;
            lock (Users)
                Users.Remove(user);
            user.Room = null;
            Command quit = new Command { Type = "quit", Message = $"exit room", User = user };
            Send(quit);
        }
    }

}
