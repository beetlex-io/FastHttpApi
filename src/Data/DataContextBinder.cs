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
                foreach (JProperty property in data)
                {
                    if (property.Value != null)
                    {
                        context.Add(property.Name, property.Value.ToString());
                    }
                }
        }

        public static void BindFormUrl(IDataContext context, string data)
        {
            HttpParse.AsynczeFromUrlEncoded(data, context);
        }
    }
}
