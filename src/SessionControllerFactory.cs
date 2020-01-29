using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
namespace BeetleX.FastHttpApi
{
    class SessionControllerFactory : IDisposable
    {

        const string FACTORY_TAG = "__CONTROLLER_FACTORY_TAG";

        private ConcurrentDictionary<string, object> mControllers = new ConcurrentDictionary<string, object>();

        private IServer mServer;

        public static SessionControllerFactory GetFactory(ISession session)
        {
            SessionControllerFactory factory = (SessionControllerFactory)session[FACTORY_TAG];
            if (factory == null)
            {
                factory = new SessionControllerFactory();
                factory.mServer = session.Server;
                session[FACTORY_TAG] = factory;
            }
            return factory;
        }

        public object this[string name]
        {
            get
            {
                mControllers.TryGetValue(name, out object result);
                return result;
            }
            set
            {
                mControllers[name] = value;
            }
        }


        public static void DisposedFactory(ISession session)
        {
            SessionControllerFactory factory = (SessionControllerFactory)session[FACTORY_TAG];
            factory?.Dispose();
        }

        public void Dispose()
        {
            foreach (var item in mControllers.Values)
            {
                if (item is IDisposable disposable)
                {
                    try
                    {
                        disposable?.Dispose();
                    }
                    catch (Exception e_)
                    {
                        if (mServer.EnableLog(EventArgs.LogType.Error))
                        {
                            mServer.Log(EventArgs.LogType.Error, null,
                                $"HTTP {item} controller disposed error {e_.Message}@{e_.StackTrace}");
                        }
                    }
                }
            }
            mControllers.Clear();
        }
    }
}
