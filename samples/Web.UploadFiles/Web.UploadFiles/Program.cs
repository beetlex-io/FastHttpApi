using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
namespace Web.UploadFiles
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
    public class Home
    {
        public object Upload(BeetleX.FastHttpApi.IHttpContext context)
        {
            foreach (var file in context.Request.Files)
                using (System.IO.Stream stream = System.IO.File.Create(file.FileName))
                {
                    file.Data.CopyTo(stream);
                    stream.Flush();
                }
            return from a in context.Request.Files select new { a.FileName, a.Data.Length };
        }
    }
}
