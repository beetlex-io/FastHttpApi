using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.ThreadQueue
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.SetDebug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
    [Controller]
    public class Home
    {
        [ThreadQueue(ThreadQueueType.Single)]
        public object SingleQueue(IHttpContext context)
        {
            return $"QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }

        [ThreadQueue(ThreadQueueType.Multiple, 2)]
        public object MultipleQueue(IHttpContext context)
        {
            return $"QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }

        [ThreadQueue("name")]
        public object UniqueQueue(string name, IHttpContext context)
        {
            return $"QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }
    }
}
