using System;
using System.Collections.Generic;
using System.Text;
using BeetleX.Buffers;
namespace BeetleX.FastHttpApi
{



    public class JsonBodySerializer : IBodySerializer
    {
        public JsonBodySerializer()
        {
            ContentType = "application/json";
        }

        public string ContentType { get; set; }

        public object GetInnerError(Exception e, HttpResponse response, bool outputStackTrace)
        {
            return new ActionResult { Url = response.Request.BaseUrl, Code = 500, Error = e.Message, StackTrace = outputStackTrace ? e.StackTrace : null };
        }

        public object GetNotSupport(HttpResponse response)
        {
            return new ActionResult { Url = response.Request.BaseUrl, Code = 403, Error = response.Request.Method + " method type not support" };
        }

        public object GetNotFoundData(HttpResponse response)
        {
            return new ActionResult { Url = response.Request.BaseUrl, Code = 404 };
        }

        public class ErrorResult
        {
            public string url { get; set; }
            public int code { get; set; }
            public string error { get; set; }
            public string stackTrace { get; set; }
        }

        public virtual int Serialize(PipeStream stream, object data)
        {

            int length = stream.CacheLength;
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            stream.Write(value);
            return stream.CacheLength - length;
        }

        public virtual bool TryDeserialize(PipeStream stream, int length, Type type, out object data)
        {
            data = null;
            if (stream.Length >= length)
            {
                string value = stream.ReadString(length);
                if (type != null)
                {
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
                }
                else
                {
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject(value);
                }
                return true;
            }
            return false;
        }


    }
}
