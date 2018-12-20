using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BeetleX.Buffers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeetleX.FastHttpApi.Clients
{
    public interface IClientBodyFormater
    {
        string ContentType { get; }

        void Serialization(object data, PipeStream stream);

        object Deserialization(BeetleX.Buffers.PipeStream stream, Type type, int length);
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class FormaterAttribute : Attribute, IClientBodyFormater
    {
        public abstract string ContentType { get; }

        public abstract void Serialization(object data, PipeStream stream);

        public abstract object Deserialization(BeetleX.Buffers.PipeStream stream, Type type, int length);
    }

    public class FormUrlFormater : FormaterAttribute
    {
        public override string ContentType => "application/x-www-form-urlencoded";

        public override object Deserialization(PipeStream stream, Type type, int length)
        {
            return stream.ReadString(length);
        }
        public override void Serialization(object data, PipeStream stream)
        {
            System.Collections.IDictionary keyValuePairs = data as IDictionary;
            if (keyValuePairs != null)
            {
                int i = 0;
                foreach (object key in keyValuePairs.Keys)
                {
                    object value = keyValuePairs[key];
                    if (value != null)
                    {
                        if (i > 0)
                            stream.Write("&");
                        stream.Write(key.ToString() + "=");
                        if (value is string)
                        {
                            stream.Write(System.Net.WebUtility.UrlEncode((string)value));
                        }
                        else
                        {
                            stream.Write(System.Net.WebUtility.UrlEncode(value.ToString()));
                        }
                        i++;
                    }
                }
            }
            else
            {
                stream.Write(data.ToString());
            }
        }
    }

    public class JsonFormater : FormaterAttribute
    {
        public override string ContentType => "application/json";

        public override object Deserialization(PipeStream stream, Type type, int length)
        {
            using (stream.LockFree())
            {
                if (type == null)
                {
                    using (System.IO.StreamReader streamReader = new System.IO.StreamReader(stream))
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
                        object token = jsonSerializer.Deserialize(reader);
                        return token;
                    }
                }
                else
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        object result = serializer.Deserialize(streamReader, type);
                        return result;
                    }
                }
            }
        }

        public override void Serialization(object data, PipeStream stream)
        {
            using (stream.LockFree())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    IDictionary dictionary = data as IDictionary;
                    JsonSerializer serializer = new JsonSerializer();
                    if (dictionary != null && dictionary.Count == 1)
                    {
                        object[] vlaues = new object[dictionary.Count];
                        dictionary.Values.CopyTo(vlaues, 0);
                        serializer.Serialize(writer, vlaues[0]);
                    }
                    else
                    {
                        serializer.Serialize(writer, data);
                    }
                }
            }
        }
    }
}
