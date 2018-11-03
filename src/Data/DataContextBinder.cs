using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    class DataContextBind
    {
        public static void BindJson(IDataContext context, JToken data)
        {
            if (data != null)
            {
                context.Add("body", data);
                if (data is JObject)
                {
                    foreach (JProperty property in data)
                    {
                        if (property.Value != null)
                        {
                            context.Add(property.Name, property);
                        }
                    }
                }
            }
        }

        public static void BindFormUrl(IDataContext context, string data)
        {
            context.Add("body", data);
            HttpParse.AsynczeFromUrlEncoded(data, context);
        }
    }
}
