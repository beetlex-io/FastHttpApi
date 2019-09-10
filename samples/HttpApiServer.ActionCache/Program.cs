using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.ActionCache
{
    [Options(AllowOrigin = "*")]
    [BeetleX.FastHttpApi.Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogToConsole = true;
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Debug;
            mApiServer.Options.SetDebug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.WriteLine(Environment.ProcessorCount);
            Console.Read();
        }

        [ActionCacheFilter]
        public object GetTime([CacheKeyParameter]string name)
        {
            Console.WriteLine($"{name} get time");
            return DateTime.Now;
        }
    }

    public class ActionCacheFilter:FilterAttribute
    {

        private static System.Collections.Concurrent.ConcurrentDictionary<string, object> mCache = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();

        public override bool Executing(ActionContext context)
        {
            string key = context.Handler.GetCackeKey(context.Parameters);
            if(mCache.TryGetValue(key,out object result))
            {
                context.Result = result;
                return false;
            }
            return base.Executing(context);
        }

        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            string key = context.Handler.GetCackeKey(context.Parameters);
            mCache[key] = context.Result;
        }
    }
}
