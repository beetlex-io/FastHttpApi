using BeetleX.Buffers;
using BeetleX.FastHttpApi.Hosting;
using BeetleX.FastHttpApi.WebSockets;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;

namespace Websocket.CustomFrameDeserialize
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
                    s =>
                    {
                        s.FrameSerializer = new DataFrameSerializer();
                        s.WebSocketReceive = OnWebsocketReceive;
                    },
                    typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
        static void OnWebsocketReceive(object sender , WebSocketReceiveArgs e)
        {
            Console.WriteLine(e.Frame.Body);
            var result = $"hello {e.Frame.Body} {DateTime.Now}";
            var frame = e.Server.CreateDataFrame(result);
            e.Server.SendToWebSocket(frame, e.Request);
        }
    }

    public class DataFrameSerializer : IDataFrameSerializer
    {
        public object FrameDeserialize(DataFrame data, PipeStream stream)
        {
            var len = (int)data.Length;
            return stream.ReadString(len);

        }

        public void FrameRecovery(byte[] buffer)
        {

        }

        public ArraySegment<byte> FrameSerialize(DataFrame packet, object body)
        {
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(value));
        }
    }
}
