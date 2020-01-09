using System;
using System.Collections.Generic;
using System.Text;
using BeetleX.FastHttpApi;
using Microsoft.Extensions.Caching.Memory;
using BeetleX.FastHttpApi.Hosting;
namespace Web.Caching
{
    public class CachedAttribute:BeetleX.FastHttpApi.FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            IMemoryCache cache = GetCache(context);
            if (cache !=null)
            {
                string key = context.Handler.GetCackeKey(context.Parameters);
                if(cache.TryGetValue(key,out object result))
                {
                    context.Result = result;
                    return false;
                }
            }

            return base.Executing(context);
        }

        private IMemoryCache GetCache(ActionContext context)
        {
            return (IMemoryCache)context.HttpContext.Server.ServiceProvider().GetService(typeof(IMemoryCache));
        }
        public override void Executed(ActionContext context)
        {
            if (context.Exception == null)
            {
                IMemoryCache cache = GetCache(context);
                if (cache != null)
                {
                    string key = context.Handler.GetCackeKey(context.Parameters);
                    cache.Set(key, context.Result);
                }
            }
            base.Executed(context);

        }
    }
}
