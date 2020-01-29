using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;

namespace BeetleX.FastHttpApi
{
    public class IPLimit
    {
        public IPLimit(HttpApiServer server)
        {
            mHttpServer = server;
            mClearTimer = new System.Threading.Timer(OnClearLimit, null, 1000, 1000 * 600);
        }

        private System.Threading.Timer mClearTimer;

        private HttpApiServer mHttpServer;

        private ConcurrentDictionary<string, LimitItem> mIpLimitTable = new ConcurrentDictionary<string, LimitItem>();

        public LimitItem GetItem(string ip)
        {
            LimitItem result = null;
            if (mHttpServer.Options.IPRpsLimit > 0)
            {
                mIpLimitTable.TryGetValue(ip, out result);
            }
            return result;
        }

        private void OnClearLimit(object state)
        {
            try
            {
                var items = mIpLimitTable.Values.ToArray();
                foreach (var item in items)
                {
                    var now = TimeWatch.GetElapsedMilliseconds();
                    if (now - item.ActiveTime > 1000 * 600)
                        mIpLimitTable.TryRemove(item.IP,out LimitItem result);
                }
            }
            catch(Exception e_)
            {
                mHttpServer.GetLog(EventArgs.LogType.Error)
                    ?.Log(EventArgs.LogType.Error, $"IP limit clear error {e_.Message}@{e_.StackTrace}");
            }
            
        }

        private LimitItem GetOrCreateItem(HttpRequest request)
        {
            string ip = request.RemoteIPAddress;
            if (mIpLimitTable.TryGetValue(ip, out LimitItem result))
                return result;
            LimitItem item = new LimitItem(ip, mHttpServer);
            if (!mIpLimitTable.TryAdd(ip, item))
                mIpLimitTable.TryGetValue(ip, out item);
            return item;
        }

        public bool ValidateRPS(HttpRequest request)
        {
            if (mHttpServer.Options.IPRpsLimit == 0)
                return true;
            var item = GetOrCreateItem(request);
            return item.ValidateRPS();
        }

        public class LimitItem
        {
            #region rps limit

            public LimitItem(string ip, HttpApiServer server)
            {
                mServer = server;
                IP = ip;
            }

            private long mEnbaledTime = 0;

            private HttpApiServer mServer;

            private int mRPS;

            private long mLastTime;

            public string IP { get; set; }

            public long ActiveTime { get; set; }

            public bool Enabled { get { return mEnbaledTime < TimeWatch.GetElapsedMilliseconds(); } }

            public bool ValidateRPS()
            {
                lock (this)
                {
                    long now = TimeWatch.GetElapsedMilliseconds();
                    ActiveTime = now;
                    if (mEnbaledTime > now)
                        return false;
                    if (mServer.Options.IPRpsLimit == 0)
                        return true;
                    if (now - mLastTime >= 1000)
                    {
                        mLastTime = now;
                        mRPS = 0;
                        return true;
                    }
                    else
                    {
                        mRPS++;
                        bool result = mRPS < mServer.Options.IPRpsLimit;
                        if (!result)
                            mEnbaledTime = mServer.Options.IPRpsLimitDisableTime + now;
                        return result;
                    }
                }

            }

            #endregion
        }

    }
}
