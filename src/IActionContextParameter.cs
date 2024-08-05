using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IActionParameter : IDisposable
    {
        ActionContext Context { get; set; }
        void Init(IHttpContext context);
    }

}
