using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.ApiViews
{
    [Controller(BaseUrl = "/_info/")]
    public class ApiInfoController
    {
        internal ActionHandlerFactory HandleFactory { get; set; }

        public object ListApi()
        {
            return HandleFactory.GetUrlInfos();
        }
    }
}
