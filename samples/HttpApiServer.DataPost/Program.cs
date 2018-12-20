using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Data;
using System;

namespace HttpApiServer.DataPost
{
    [Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;
        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Warring;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        public object GetTime()
        {
            return DateTime.Now;
        }
        // Get /hello?name=henry 
        // or
        // Get /hello/henry
        [Get(Route = "{name}")]
        public object Hello(string name, IHttpContext context)
        {
            return $"hello {name} {DateTime.Now}";
        }

        [Get(Route = "{id}-{value}")]
        public object SetValue(string id, string value, IHttpContext context)
        {
            return $"{id}={value} {DateTime.Now}";
        }

        [Post]
        [NoDataConvert]
        public object PostStream(IHttpContext context)
        {
            Console.WriteLine(context.Data);
            string value = context.Request.Stream.ReadString(context.Request.Length);
            return value;
        }
        //json {"name":"","value":""}
        [Post]
        [JsonDataConvert]
        public object Post(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
        //name=aaa&value=aaa
        [Post]
        [FormUrlDataConvert]
        public object PostForm(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
    }
}
