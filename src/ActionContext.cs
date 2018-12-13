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


        private void OnExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {
                        Result = Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                    }
                    catch (Exception error)
                    {
                        Exception = error;
                    }
                    finally
                    {
                        FilterExecuted();
                    }
                }
                if (Exception != null)
                    resultHandler.Error(Exception);
                else
                    resultHandler.Success(Result);
            }
            catch (Exception e_)
            {
                resultHandler.Error(e_);
            }
        }

        private async void OnAsyncExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {
                        var task = (Task)Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                        await task;
                        if (Handler.PropertyHandler != null)
                            Result = Handler.PropertyHandler.Get(task);
                        else
                            Result = null;
                    }
                    catch (Exception error)
                    {
                        Exception = error;
                    }
                    finally
                    {
                        FilterExecuted();
                    }
                }
                if (Exception != null)
                    resultHandler.Error(Exception);
                else
                    resultHandler.Success(Result);
            }
            catch (Exception e_)
            {
                resultHandler.Error(e_);
            }
        }

        internal void Execute(IActionResultHandler resultHandler)
        {
            if (Handler.Async)
            {
                OnAsyncExecute(resultHandler);
            }
            else
            {
                OnExecute(resultHandler);
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
                int start = mFilters.Count - 1;
                for (int i = start; i >= 0; i--)
                {
                    mFilters[i].Executed(this);
                }
            }
        }
    }
}
