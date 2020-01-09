using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace WebSocket.UploadFiles
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
                        o.WebSocketMaxRPS = 0;
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
        public void Upload(string name, string data, bool completed, int index, int buffersize, long filesize)
        {
            byte[] buffer = System.Convert.FromBase64String(data);
            using (System.IO.Stream stream =
                index == 0 ? System.IO.File.Create(name) : System.IO.File.Open(name, System.IO.FileMode.OpenOrCreate))
            {
                if (stream.Length > buffersize * index)
                    stream.Position = buffersize * index;
                else
                    stream.Seek(0, System.IO.SeekOrigin.End);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            System.Threading.Thread.Sleep(20);
        }
    }

}
