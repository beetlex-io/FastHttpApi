using System;
using System.Collections.Generic;
using System.Text;
using BeetleX.Buffers;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class OptionsAttribute : Attribute, IResult
    {
        public OptionsAttribute()
        {
            HasBody = false;
        }

        public virtual IHeaderItem ContentType => ContentTypes.TEXT_UTF8;

        public int Length { get; set; }

        public virtual bool HasBody { get; set; }

        public string AllowOrigin { get; set; }

        public string AllowMethods { get; set; }

        public string AllowHeaders { get; set; }

        public string AllowMaxAge { get; set; }

        public bool? AllowCredentials { get; set; }

        public string Vary { get; set; } = "Origin";

        public virtual void Setting(HttpResponse response)
        {
            if (!string.IsNullOrEmpty(AllowOrigin))
            {
                response.Header["Access-Control-Allow-Origin"] = AllowOrigin;
                if (AllowOrigin != "*")
                    response.Header["Vary"] = Vary;
            }
            if (!string.IsNullOrEmpty(AllowMethods))
            {
                response.Header["Access-Control-Allow-Methods"] = AllowMethods;
            }

            if (!string.IsNullOrEmpty(AllowHeaders))
            {
                response.Header["Access-Control-Allow-Headers"] = AllowHeaders;
            }

            if (!string.IsNullOrEmpty(AllowMaxAge))
            {
                response.Header["Access-Control-Max-Age"] = AllowMaxAge;
            }
            if (AllowCredentials != null)
            {
                if (AllowCredentials.Value)
                {
                    response.Header["Access-Control-Allow-Credentials"] = "true";
                }
                else
                {
                    response.Header["Access-Control-Allow-Credentials"] = "false";
                }
            }
        }

        public virtual void Write(PipeStream stream, HttpResponse response)
        {

        }

        public virtual void SetResponse(HttpRequest request, HttpResponse response)
        {
            HttpApiServer server = request.Server;
            if (server.EnableLog(EventArgs.LogType.Debug))
                server.Log(EventArgs.LogType.Debug, $"{request.RemoteIPAddress} {request.Method} {request.Url} set options");
            response.Header["Access-Control-Allow-Origin"] = AllowOrigin;
        }
    }
}
