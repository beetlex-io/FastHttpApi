using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ActionContext
    {

        internal ActionContext(ActionHandler handler, HttpRequest request, HttpResponse response)
        {
            mHandler = handler;
            mFilters = handler.Filters;
            Request = request;
            Response = response;
        }

        private int mIndex = -1;

        private List<FilterAttribute> mFilters;

        private ActionHandler mHandler;

        public HttpResponse Response { get; private set; }

        public HttpRequest Request { get; private set; }

        internal Object Result { get; set; }

        public void Execute()
        {
            mIndex++;
            if (mIndex == mFilters.Count)
            {
                Result = mHandler.Invoke(this.Request, this.Response);
            }
            else
            {
                mFilters[mIndex].Execute(this);
            }
        }
    }
}
