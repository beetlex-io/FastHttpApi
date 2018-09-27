using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ActionContext
    {

        internal ActionContext(ActionHandler handler, IHttpContext context)
        {
            mHandler = handler;
            mFilters = handler.Filters;
            HttpContext = context;

        }

        private int mIndex = -1;

        private List<FilterAttribute> mFilters;

        public IHttpContext HttpContext { get; internal set; }

        private ActionHandler mHandler;

        public Object Result { get; set; }

        public void Execute()
        {
            mIndex++;
            if (mIndex == mFilters.Count)
            {
                Result = mHandler.Invoke(HttpContext);
            }
            else
            {
                mFilters[mIndex].Execute(this);
            }
        }
    }
}
