using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IBodySerializer
    {

        int Serialize(PipeStream stream, object data);

        bool TryDeserialize(PipeStream stream, int length, Type type, out object data);

        object GetNotFoundData(HttpResponse response);

        object GetInnerError(Exception e, HttpResponse response, bool outputStackTrace);

        object GetNotSupport(HttpResponse response);

        string ContentType { get; set; }
    }

    public class StringSerializer : IBodySerializer
    {

        public StringSerializer()
        {
            ContentType = "text/html";
        }

        public string ContentType { get; set; }

        public bool TryDeserialize(PipeStream stream, int length, Type type, out object data)
        {
            data = null;
            if (stream.Length >= length)
            {
                data = stream.ReadString(length);
                return true;
            }
            return false;
        }

        public int Serialize(PipeStream stream, object data)
        {
            string body = data as string;
            if (body == null)
                body = data.ToString();
            int length = stream.CacheLength;
            stream.Write(body);
            return stream.CacheLength - length;
        }

        public object GetNotFoundData(HttpResponse response)
        {
            return response.Request.Url + " 404 not found!";
        }

        public object GetInnerError(Exception e, HttpResponse response, bool outputStackTrace)
        {
            return response.Request.Url + " 500 inner error " + e.Message + (outputStackTrace ? e.StackTrace : "");
        }

        public object GetNotSupport(HttpResponse response)
        {
            return response.Request.Method + " method type not support!";
        }
    }
}
