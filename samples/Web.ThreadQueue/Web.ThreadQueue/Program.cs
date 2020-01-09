using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Web.ThreadQueue
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

    [Controller]
    public class Home
    {
       
        public object NoneQueue(string name, IHttpContext context)
        {
            return $"Name:{name}|QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }

        [ThreadQueue(ThreadQueueType.Single)]
        public object SingleQueue(string name, IHttpContext context)
        {
            return $"Name:{name}|QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }

        [ThreadQueue(ThreadQueueType.Multiple, 2)]
        public object MultipleQueue(string name, IHttpContext context)
        {
            return $"Name:{name}|QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }

        [ThreadQueue("name")]
        public object UniqueQueue(string name, IHttpContext context)
        {
            return $"Name:{name}|QueueID:{context.Queue?.ID}|Time:{DateTime.Now}";
        }
    }
}
