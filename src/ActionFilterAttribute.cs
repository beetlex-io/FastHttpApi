using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class FilterAttribute : Attribute
    {
        public abstract void Execute(ActionContext context);

    }
}
