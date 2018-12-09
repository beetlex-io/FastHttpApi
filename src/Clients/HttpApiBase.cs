using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.Clients
{

    public class HttpApiProxy : System.Reflection.DispatchProxy
    {
        public HttpApiProxy()
        {
            TimeOut = 10000;
        }

        public HttpHost Host { get; set; }

        public int TimeOut { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            ClientActionHanler handler = ClientActionFactory.GetHandler((MethodInfo)targetMethod);
            var rinfo = handler.GetRequestInfo(args);
            var request = rinfo.GetRequest(Host);
            var task = request.Execute();
            if (!handler.Async)
            {
                task.Wait(TimeOut);
                if (!task.Wait(TimeOut))
                {
                    throw new HttpClientException(request, Host.Uri, $"{rinfo.Method} {rinfo.Url} request time out!");
                }
                if (task.Result.Exception != null)
                    throw task.Result.Exception;
                return task.Result.Body;
            }
            else
            {
                if (handler.MethodType == typeof(ValueTask))
                {
                    AnyCompletionSource<object> source = new AnyCompletionSource<object>();
                    source.WaitResponse(task);
                    return new ValueTask(source.Task);
                }
                else
                {
                    Type gtype = typeof(AnyCompletionSource<>);
                    Type type = gtype.MakeGenericType(handler.ReturnType);
                    IAnyCompletionSource source = (IAnyCompletionSource)Activator.CreateInstance(type);
                    source.WaitResponse(task);
                    return source.GetTask();
                }
            }
        }
    }

    interface IAnyCompletionSource
    {
        void Success(object data);
        void Error(Exception error);
        void WaitResponse(Task<Response> task);
        Task GetTask();
    }

    class AnyCompletionSource<T> : TaskCompletionSource<T>, IAnyCompletionSource
    {
        public void Success(object data)
        {
            TrySetResult((T)data);
        }

        public void Error(Exception error)
        {
            TrySetException(error);
        }

        public async void WaitResponse(Task<Response> task)
        {
            var response = await task;
            if (response.Exception != null)
                Error(response.Exception);
            else
                Success(response.Body);
        }

        public Task GetTask()
        {
            return this.Task;
        }
    }

    public class HttpApiClient
    {
        public HttpApiClient(string host)
        {
            Host = new HttpHost(host);
        }

        public HttpHost Host { get; set; }

        protected async Task<T> OnExecute<T>(MethodBase targetMethod, params object[] args)
        {
            var rinfo = ClientActionFactory.GetHandler((MethodInfo)targetMethod).GetRequestInfo(args);
            var request = rinfo.GetRequest(Host);
            var respnse = await request.Execute();
            if (respnse.Exception != null)
                throw respnse.Exception;
            return (T)respnse.Body;
        }

        private System.Collections.Concurrent.ConcurrentDictionary<Type, object> mAPI = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();

        public T Create<T>()
        {
            Type type = typeof(T);
            object result;
            if (!mAPI.TryGetValue(type, out result))
            {
                result = DispatchProxy.Create<T, HttpApiProxy>();
                mAPI[type] = result;
                ((HttpApiProxy)result).Host = Host;
            }
            return (T)result;
        }
    }
}
