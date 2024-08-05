using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;
using BeetleX.EventArgs;

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

        private ConcurrentDictionary<string, LimitItem> mCustomIpLimitTable = new ConcurrentDictionary<string, LimitItem>();

        public IPLimitConfig Config { get; set; }


        public void Load()
        {
            Config = IPLimitConfig.Instance;
            Config.Save();
            foreach (var item in Config.Items)
            {
                AddIPAddress(item.IP, item.MaxRPS);
            }
        }

        public void Save()
        {
            Config.Items = (from a in mCustomIpLimitTable.Values select new LimitRecord { IP = a.IP, MaxRPS = a.MaxRPS }).ToList();
            Config.Save();
        }

        public LimitItem GetItem(string ip)
        {
            LimitItem result = null;
            if (mHttpServer.Options.IPRpsLimit > 0)
            {
                mIpLimitTable.TryGetValue(ip, out result);
            }
            return result;
        }

        public void RemoveIPAddress(string ip)
        {
            mCustomIpLimitTable.TryRemove(ip, out LimitItem result);
        }

        public void AddIPAddress(string ip, int maxrps)
        {
            var item = new LimitItem(ip, mHttpServer);
            item.MaxRPS = maxrps;
            mCustomIpLimitTable[ip] = item;
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
                        mIpLimitTable.TryRemove(item.IP, out LimitItem result);
                }
            }
            catch (Exception e_)
            {
                mHttpServer.GetLog(EventArgs.LogType.Error)
                    ?.Log(EventArgs.LogType.Error, null, $"IP limit clear error {e_.Message}@{e_.StackTrace}");
            }

        }

        private LimitItem GetOrCreateItem(HttpRequest request)
        {
            string ip = request.RemoteIPAddress;
            if (mCustomIpLimitTable.TryGetValue(ip, out LimitItem result))
                return result;
            if (mIpLimitTable.TryGetValue(ip, out result))
                return result;
            LimitItem item = new LimitItem(ip, mHttpServer);
            if (!mIpLimitTable.TryAdd(ip, item))
                mIpLimitTable.TryGetValue(ip, out item);
            return item;
        }

        public bool ValidateRPS(HttpRequest request)
        {
            if (mHttpServer.Options.IPRpsLimit == 0 && mCustomIpLimitTable.Count == 0)
                return true;
            var item = GetOrCreateItem(request);
            return item.ValidateRPS();
        }


        public class LimitRecord
        {

            public string IP { get; set; }

            public int MaxRPS { get; set; }
        }


        public class IPLimitConfig : ConfigBase<IPLimitConfig>
        {
            public List<LimitRecord> Items { get; set; } = new List<LimitRecord>();
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

            public int MaxRPS { get; set; } = 0;

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
                    if (mServer.Options.IPRpsLimit == 0 && MaxRPS == 0)
                        return true;
                    if (now - mLastTime >= 1000)
                    {
                        mLastTime = now;
                        mRPS = 0;
                        return true;
                    }
                    else
                    {
                        var max = System.Math.Min(mServer.Options.IPRpsLimit, MaxRPS);
                        if (max <= 0)
                            max = System.Math.Max(mServer.Options.IPRpsLimit, MaxRPS);
                        mRPS++;
                        bool result = mRPS < max;
                        if (!result && mServer.Options.IPRpsLimitDisableTime > 0)
                            mEnbaledTime = mServer.Options.IPRpsLimitDisableTime + now;
                        return result;
                    }
                }

            }

            #endregion
        }

    }
    public class UrlLimitRecord
    {

        public string Url { get; set; }

        public int MaxRPS { get; set; }
    }

    public interface IUrlLimitConfig<T>
    {
        List<UrlLimitRecord> Items { get; set; }

        void Save();

        T GetInstance();
    }
    public class UrlLimitConfig : ConfigBase<UrlLimitConfig>, IUrlLimitConfig<UrlLimitConfig>
    {
        public List<UrlLimitRecord> Items { get; set; } = new List<UrlLimitRecord>();


    }

    public class DomainLimitConfig : ConfigBase<DomainLimitConfig>, IUrlLimitConfig<DomainLimitConfig>
    {
        public List<UrlLimitRecord> Items { get; set; } = new List<UrlLimitRecord>();


    }

    class UrlLimitAgent
    {
        public UrlLimitRecord Config { get; set; }

        public RpsLimit RpsLimit { get; set; }

        public long Version { get; set; }
        public bool ValidateRPS()
        {
            if (Config == null)
                return true;
            return !RpsLimit.Check(Config.MaxRPS);
        }
    }
    public class UrlLimit<CONFIG>
        where CONFIG : IUrlLimitConfig<CONFIG>, new()
    {

        private long mVersion = 0;

        private ConcurrentDictionary<string, UrlLimitRecord> mLimitTable =
            new ConcurrentDictionary<string, UrlLimitRecord>(StringComparer.InvariantCultureIgnoreCase);

        private ConcurrentDictionary<string, UrlLimitAgent> mCachedLimitTable =
            new ConcurrentDictionary<string, UrlLimitAgent>(StringComparer.InvariantCultureIgnoreCase);

        private UrlLimitRecord[] mUrlTables = new UrlLimitRecord[0];

        public int CacheSize { get; set; } = 1024 * 200;

        public long Version
        {
            get
            {
                return mVersion;
            }
        }

        private void OnRefreshTable()
        {
            lock (this)
            {

                var items = (from a in mLimitTable.Values
                             orderby a.Url.Length descending
                             select a).ToArray();
                if (items == null)
                {
                    items = new UrlLimitRecord[0];
                }
                mUrlTables = items;
                System.Threading.Interlocked.Increment(ref mVersion);
            }
        }

        public int Count => mUrlTables.Length;

        public void AddUrl(string url, int maxrps)
        {
            mLimitTable[url] = new UrlLimitRecord { Url = url, MaxRPS = maxrps };
            OnRefreshTable();
        }

        public void RemoveUrl(string url)
        {
            mLimitTable.TryRemove(url, out UrlLimitRecord result);
            OnRefreshTable();
        }

        public CONFIG Config { get; set; }

        internal UrlLimitAgent Match(string url,HttpRequest request)
        {
            if (mCachedLimitTable.TryGetValue(url, out UrlLimitAgent result))
            {
                if (result.Version == Version)
                    return result;
            }
            var items = mUrlTables;
            result = new UrlLimitAgent();
            foreach (var item in items)
            {
                if (url.IndexOf(item.Url, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    result.RpsLimit = new RpsLimit(item.MaxRPS);
                    result.Config = item;
                }
            }

            result.Version = System.Threading.Interlocked.Add(ref mVersion, 0);
            if (mCachedLimitTable.Count < this.CacheSize)
            {
                mCachedLimitTable[url] = result;
            }
            else
            {
                request.Server.GetLog(LogType.Warring)?.Log(LogType.Warring, null, $"Http url limit cached out of {this.CacheSize} size!");
            }

            return result;
        }
        public void Load()
        {

            Config = new CONFIG().GetInstance();
            Config.Save();
            foreach (var item in Config.Items)
            {
                AddUrl(item.Url, item.MaxRPS);
            }
        }

        public void Save()
        {
            Config.Items = mLimitTable.Values.ToList();
            Config.Save();
        }


    }
}
