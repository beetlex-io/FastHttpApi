using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ActionContext
    {

        internal ActionContext(ActionHandler handler, IHttpContext context, ActionHandlerFactory actionHandlerFactory)
        {
            Handler = handler;
            mFilters = handler.Filters;
            HttpContext = context;
            ActionHandlerFactory = actionHandlerFactory;
            Parameters = handler.GetParameters(context);

        }

        private int mIndex = -1;

        private List<FilterAttribute> mFilters;

        public object[] Parameters { get; set; }

        public IHttpContext HttpContext { get; internal set; }

        public ActionHandler Handler { get; private set; }

        public ActionHandlerFactory ActionHandlerFactory { get; private set; }

        public Object Result { get; set; }

        public void Execute()
        {
            mIndex++;
            if (mIndex == mFilters.Count)
            {
                Result = Handler.Invoke(HttpContext, ActionHandlerFactory, Parameters);
            }
            else
            {
                mFilters[mIndex].Execute(this);
            }
        }
    }
}
