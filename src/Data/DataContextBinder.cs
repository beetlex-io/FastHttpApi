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
                context.SetValue("body", data);
                if (data is JObject)
                {
                    foreach (JProperty property in data)
                    {
                        if (property.Value != null)
                        {
                            context.SetValue(property.Name, property);
                        }
                    }
                }
            }
        }

        public static void BindFormUrl(IDataContext context, string data)
        {
            context.SetValue("body", data);
            HttpParse.AsynczeFromUrlEncoded(data, context);
        }

        public static DataConvertAttribute GetConvertAttribute(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return new JsonDataConvertAttribute();
            }
            else if (contentType.IndexOf("application/x-www-form-urlencoded", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return new FormUrlDataConvertAttribute();
            }
            else if (contentType.IndexOf("multipart/form-data", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return new MultiDataConvertAttribute();
            }
            else
            {
                return new JsonDataConvertAttribute();
            }
        }
    }
}
