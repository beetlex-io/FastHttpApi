using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    public class JsonDataConext : DataContxt
    {
        public JsonDataConext(Newtonsoft.Json.Linq.JToken token)
        {
            foreach (JProperty property in token)
            {
                if (property.Value != null)
                {
                    Add(property.Name, property.Value.ToString());
                }
            }
        }
    }
}
