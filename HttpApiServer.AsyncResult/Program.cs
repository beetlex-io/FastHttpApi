using BeetleX.FastHttpApi;
using System;
using System.Threading.Tasks;

namespace HttpApiServer.AsyncResult
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.ServerConfig.BodySerializer = new JsonBodySerializer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Debug();
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }

    [Controller]
    public class Test
    {
        public string hello(string name)
        {
            return string.Format("[{0}] hello {1}", DateTime.Now, name);
        }

        public void asyncHello(string name, HttpResponse response)
        {
            response.Async();
            Task.Run(() =>
            {
                Console.WriteLine("sleep ...");
                System.Threading.Thread.Sleep(5000);
                response.Result(string.Format("[{0}] hello {1}", DateTime.Now, name));
            });
        }
    }
}
