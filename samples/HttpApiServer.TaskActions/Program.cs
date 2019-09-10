using BeetleX.FastHttpApi;
using System;
using System.Threading.Tasks;

namespace HttpApiServer.TaskActions
{
    [BeetleX.FastHttpApi.Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Debug;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Options.SetDebug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        [Get(Route = "{name}")]
        public Task<String> Hello(string name)
        {
            string result = $"hello {name} {DateTime.Now}";
            return Task.FromResult(result);
        }

        public async Task<String> Wait()
        {
            await Task.Delay(2000);
            return $"{DateTime.Now}";
        }
    }
}
