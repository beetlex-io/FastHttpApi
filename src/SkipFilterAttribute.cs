using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SkipFilterAttribute : Attribute
    {

        public SkipFilterAttribute(params Type[] types)
        {
            Types = types;
            if (Types == null)
                Types = new Type[0];
        }

        public Type[] Types { get; set; }
    }
}
