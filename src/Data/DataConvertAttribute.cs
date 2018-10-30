using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class DataConvertAttribute : Attribute
    {
        public abstract void Execute(IDataContext dataContext, HttpRequest request);
    }

    public class JsonDataConvertAttribute : DataConvertAttribute
    {
        public override void Execute(IDataContext dataContext, HttpRequest request)
        {
            if (request.Length > 0)
            {
                string value = request.Stream.ReadString(request.Length);
                JToken token = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(value);
                DataContextBind.BindJson(dataContext, token);
            }
        }
    }

    public class FormUrlDataConvertAttribute : DataConvertAttribute
    {
        public override void Execute(IDataContext dataContext, HttpRequest request)
        {
            if (request.Length > 0)
            {
                string data = request.Stream.ReadString(request.Length);
                DataContextBind.BindFormUrl(dataContext, data);
            }
        }
    }
}
