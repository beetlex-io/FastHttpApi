using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            Controller = handler.Controller;
            if (!handler.SingleInstance)
            {
                Controller = actionHandlerFactory.GetController(handler.ControllerType);
                if (Controller == null)
                    Controller = this.Controller;
            }
        }

        private List<FilterAttribute> mFilters;

        public object[] Parameters { get; set; }

        public IHttpContext HttpContext { get; internal set; }

        public ActionHandler Handler { get; private set; }

        public ActionHandlerFactory ActionHandlerFactory { get; private set; }

        public Object Result { get; set; }

        public Exception Exception { get; set; }

        public object Controller { get; set; }

        internal async void Execute()
        {
            try
            {
                if (FilterExecuting())
                {
                    Result = Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                    if (Result is Task task)
                    {
                        await task;
                        if (Handler.PropertyHandler != null)
                        {
                            Result = Handler.PropertyHandler.Get(task);
                        }
                        else
                        {
                            Result = null;
                        }
                        FilterExecuted();
                    }
                    else
                    {
                        FilterExecuted();
                    }
                }
            }
            catch (Exception e_)
            {
                this.Exception = e_;
            }
        }
        private bool FilterExecuting()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    bool result = mFilters[i].Executing(this);
                    if (!result)
                        return false;
                }
            }
            return true;
        }

        private void FilterExecuted()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    mFilters[i].Executed(this);
                }
            }
        }
    }
}
