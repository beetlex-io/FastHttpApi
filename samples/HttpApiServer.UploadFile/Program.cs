using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Data;
using System;
using System.Linq;

namespace HttpApiServer.UploadFile
{
    [BeetleX.FastHttpApi.Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogToConsole = true;
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Info;
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }

        [Post]
        [MultiDataConvert]
        public object UploadFile(string name, IHttpContext context)
        {
            foreach (var file in context.Request.Files)
                using (System.IO.Stream stream = System.IO.File.Create(file.FileName))
                {
                    file.Data.CopyTo(stream);
                }
            return $"{DateTime.Now} {name} {string.Join(",", (from fs in context.Request.Files select fs.FileName).ToArray())}";
        }
    }
}
