using BeetleX.Tracks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public class ActionContext : IDisposable
    {

        internal ActionContext(ActionHandler handler, IHttpContext context, ActionHandlerFactory actionHandlerFactory)
        {
            Handler = handler;
            mFilters = handler.Filters;
            HttpContext = context;
            ActionHandlerFactory = actionHandlerFactory;
            Controller = handler.Controller;
            if (HttpContext.WebSocket)
            {
                ActionType = "WS";
            }
            else
            {
                ActionType = "HTTP";
            }

        }

        public void Init()
        {
            FilterInit();
            Parameters = Handler.GetParameters(HttpContext, this);
            if (Handler.InstanceType != InstanceType.Single)
            {
                if (Handler.InstanceType == InstanceType.Session)
                {
                    var factory = SessionControllerFactory.GetFactory(HttpContext.Session);
                    Controller = factory[Handler.ControllerUID];
                    if (Controller == null)
                    {
                        Controller = ActionHandlerFactory.GetController(Handler.ControllerType, HttpContext);
                        if (Controller == null)
                            Controller = Activator.CreateInstance(Handler.ControllerType);
                        factory[Handler.ControllerUID] = Controller;
                    }
                }
                else
                {
                    Controller = ActionHandlerFactory.GetController(Handler.ControllerType, HttpContext);
                    if (Controller == null)
                        Controller = Activator.CreateInstance(Handler.ControllerType);
                }
            }
            if (Controller == null)
                Controller = Handler.Controller;
        }

        private string ActionType = "";

        private List<FilterAttribute> mFilters;

        public object[] Parameters { get; private set; }

        public IHttpContext HttpContext { get; internal set; }

        public ActionHandler Handler { get; private set; }

        public ActionHandlerFactory ActionHandlerFactory { get; private set; }

        public Object Result { get; set; }

        public Exception Exception { get; set; }

        public bool HasError { get; private set; }

        public object Controller { get; set; }

        public string GetCacheKey()
        {
            return Handler.GetCackeKey(Parameters);
        }

        private void OnExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {
                        using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Execute"))
                        {
                            Result = Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                        }

                    }
                    catch (Exception error)
                    {
                        Exception = error;
                        HasError = true;
                    }
                    finally
                    {
                        FilterExecuted();
                        ParametersDisposed();
                    }
                }
                if (Exception != null)
                {
                    Handler.IncrementError();
                    resultHandler.Error(Exception);
                }
                else
                {
                    using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Responsed"))
                    {
                        resultHandler.Success(Result);
                    }
                }
            }
            catch (Exception e_)
            {
                Handler.IncrementError();
                resultHandler.Error(e_);
            }
            finally
            {
                using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Disposed"))
                {
                    DisposedController();
                    Dispose();
                }
            }
        }

        private async Task OnAsyncExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {

                        using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Execute"))
                        {
                            var task = (Task)Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                            await task;
                            if (Handler.PropertyHandler != null)
                                Result = Handler.PropertyHandler.Get(task);
                            else
                                Result = null;
                        }
                    }
                    catch (Exception error)
                    {
                        Exception = error;
                        HasError = true;
                    }
                    finally
                    {
                        FilterExecuted();
                        ParametersDisposed();
                    }
                }
                if (Exception != null)
                {
                    Handler.IncrementError();
                    resultHandler.Error(Exception);
                }
                else
                {
                    using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Response"))
                    {
                        resultHandler.Success(Result);
                    }
                }
            }
            catch (Exception e_)
            {
                Handler.IncrementError();
                resultHandler.Error(e_);
            }
            finally
            {
                using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null, "Disposed"))
                {
                    DisposedController();
                    Dispose();
                }
            }
        }

        private void DisposedController()
        {
            if (Handler.InstanceType == InstanceType.None)
            {
                try
                {
                    if (Controller is IDisposable disposable)
                        disposable?.Dispose();
                }
                catch (Exception e_)
                {
                    if (HttpContext.Server.EnableLog(EventArgs.LogType.Error))
                    {
                        var request = HttpContext.Request;
                        HttpContext.Server.Log(EventArgs.LogType.Error, request.Session,
                            $"HTTP {request.RemoteIPAddress} {request.Method} {request.BaseUrl} controller disposed error {e_.Message}@{e_.StackTrace}");
                    }
                }
            }
        }

        struct ActionTask : IEventWork
        {
            public ActionTask(ActionContext context, IActionResultHandler resultHandler, TaskCompletionSource<object> completionSource)
            {
                Context = context;
                ResultHandler = resultHandler;
                CompletionSource = completionSource;
            }

            public TaskCompletionSource<object> CompletionSource { get; set; }

            public ActionContext Context { get; set; }

            public IActionResultHandler ResultHandler { get; set; }

            public void Dispose()
            {

            }

            public async Task Execute()
            {
                try
                {
                    if (Context.Handler.Async)
                    {
                        await Context.OnAsyncExecute(ResultHandler);
                    }
                    else
                    {
                        Context.OnExecute(ResultHandler);
                    }
                }
                finally
                {

                    CompletionSource?.TrySetResult(new object());
                }
            }
        }


        internal async Task Execute(IActionResultHandler resultHandler)
        {
            if (Handler.ValidateRPS())
            {
                Handler.IncrementRequest();
                if (Handler.ThreadQueue == null || Handler.ThreadQueue.Type == ThreadQueueType.None)
                {
                    if (Handler.Async)
                    {
                        await OnAsyncExecute(resultHandler);
                    }
                    else
                    {
                        OnExecute(resultHandler);
                    }
                }
                else
                {
                    ActionTask actionTask = new ActionTask(this, resultHandler, new TaskCompletionSource<object>());
                    var queue = Handler.ThreadQueue.GetQueue(this.HttpContext);
                    if (Handler.ThreadQueue.Enabled(queue))
                    {
                        this.HttpContext.Queue = queue;
                        queue.Enqueue(actionTask);
                        await actionTask.CompletionSource.Task;
                    }
                    else
                    {
                        Handler.IncrementError();
                        resultHandler.Error(new Exception($"{Handler.SourceUrl} process error,out of queue limit!"), EventArgs.LogType.Warring, 500);
                    }
                }
            }
            else
            {
                Handler.IncrementError();
                resultHandler.Error(new Exception($"{Handler.SourceUrl} process error,out of max rps!"), EventArgs.LogType.Warring, 509);
            }

        }

        private int mFilterIndex;

        private void FilterInit()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    mFilters[i].Init(HttpContext, Handler);
                }
            }
        }

        private void FilterDisposed()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    try
                    {
                        mFilters[i].Disposed(this);
                    }
                    catch (Exception e_)
                    {
                        if (HttpContext.Server.EnableLog(EventArgs.LogType.Error))
                        {
                            var request = HttpContext.Request;
                            HttpContext.Server.Log(EventArgs.LogType.Error, request.Session,
                                $"HTTP {request.RemoteIPAddress} {request.Method} {request.BaseUrl} {mFilters[i]} filter disposed error {e_.Message}@{e_.StackTrace}");
                        }
                    }
                }
            }
        }

        private void ParametersDisposed()
        {
            if (Parameters != null)
            {
                for (int i = 0; i < Parameters.Length; i++)
                {
                    try
                    {
                        if (Parameters[i] != null && Parameters[i] is IActionParameter parameter)
                        {
                            parameter.Dispose();
                        }
                    }
                    catch (Exception e_)
                    {
                        this.Exception = e_;
                        if (HttpContext.Server.EnableLog(EventArgs.LogType.Error))
                        {
                            var request = HttpContext.Request;
                            HttpContext.Server.Log(EventArgs.LogType.Error, request.Session,
                                $"HTTP {request.RemoteIPAddress} {request.Method} {request.BaseUrl} {Parameters[i]} parameter disposed error {e_.Message}@{e_.StackTrace} inner error:{e_.InnerException?.Message}");
                        }
                    }
                }
            }
        }

        private bool FilterExecuting()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null,
                        "Filter", mFilters[i].GetType().Name, "Executing"))
                    {
                        bool result = mFilters[i].Executing(this);
                        mFilterIndex++;
                        if (!result)
                            return false;
                    }
                }
            }
            return true;
        }

        private void FilterExecuted()
        {
            if (mFilters.Count > 0)
            {
                int start = mFilterIndex - 1;
                for (int i = start; i >= 0; i--)
                {
                    using (CodeTrackFactory.Track(Handler.SourceUrl, CodeTrackLevel.Function, null,
                        "Filter", mFilters[i].GetType().Name, "Executed"))
                    {
                        mFilters[i].Executed(this);
                    }
                }
            }
        }

        private bool mIsDisposed = false;

        public void Dispose()
        {
            if (!mIsDisposed)
            {
                mIsDisposed = true;

                FilterDisposed();
                // ParametersDisposed();
            }
        }
    }
}
