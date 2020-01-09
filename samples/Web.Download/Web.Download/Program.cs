using BeetleX.Buffers;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Web.Download
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
        public object List()
        {
            return Northwind.Data.DataHelper.Defalut.Customers;
        }

        public object Download()
        {
            return new DownLoadJson(Northwind.Data.DataHelper.Defalut.Customers);
        }
    }

    public class DownLoadJson : BeetleX.FastHttpApi.IResult
    {
        public DownLoadJson(object data)
        {
            Data = data;
        }
        public object Data { get; set; }
        public IHeaderItem ContentType => ContentTypes.OCTET_STREAM;
        public int Length { get; set; }
        public bool HasBody => true;
        public void Setting(HttpResponse response)
        {
            response.Header.Add("Content-Disposition", "attachment;filename=data.json");
        }
        public void Write(PipeStream stream, HttpResponse response)
        {
            var text = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            stream.Write(text);
        }
    }
}
