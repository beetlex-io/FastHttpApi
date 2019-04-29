using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FilterAttribute : Attribute
    {
        public virtual bool Executing(ActionContext context)
        {
            return true;
        }

        public virtual void Executed(ActionContext context)
        {

        }
    }

    public class DefaultJsonResultFilter : FilterAttribute
    {
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            if (!(context.Result is JsonResult))
                context.Result = new JsonResult(context.Result);
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class FilterRouteAttribute : Attribute
    {
        public FilterRouteAttribute(string url)
        {
            Url = url;
        }
        public string Url { get; set; }
    }

}
