using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BeetleX.FastHttpApi
{
    //1111 1111     1111 1111 1111 1111 1111 1111 1111 1111 1111    1111 1111 1111 1111 1111
    //[组 256 ]     [2010-1-1到当前时间秒         max:68719476735]    [自增值     最大:1000000]
    //              [可用年数:2179]
    public class IDGenerator
    {

        private byte mGroup = 1;

        private long mSeconds;

        private ulong mID = 1;

        private long mLastTime;

        private System.Diagnostics.Stopwatch mWatch = new System.Diagnostics.Stopwatch();

        public IDGenerator()
        {
            LoadIPAddress();
            Init();
        }

        public static IDGenerator Default { get; set; } = new IDGenerator();

        public IDGenerator(byte group)
        {
            mGroup = group;
            Init();
        }
        private void LoadIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var value = ip.GetAddressBytes();
                    if (value[0] == 10)
                    {
                        mGroup = value[3];
                        break;

                    }
                    if (value[0] == 172)
                    {
                        if (value[1] >= 16 && value[1] <= 31)
                        {
                            mGroup = value[3];
                            break;
                        }
                    }
                    if (value[0] == 192 && value[1] == 168)
                    {
                        mGroup = value[3];
                        break;
                    }
                }
            }
            if (mGroup == 0)
            {
                mGroup = 1;
            }
        }

        private void Init()
        {
            var ts = DateTime.Now - DateTime.Parse("2010-1-1");
            mSeconds = (long)ts.TotalSeconds;
            mWatch.Restart();
            mLastTime = (long)Math.Floor(mWatch.Elapsed.TotalSeconds);
        }

        public ulong Next()
        {
            lock (this)
            {
                ulong result = 0;
                result |= (ulong)mGroup << 56;
                mID++;
            START:
                var now = (long)Math.Floor(mWatch.Elapsed.TotalSeconds);
                if (now - mLastTime > 1)
                {
                    mID = 1;
                    mLastTime = now;
                }
                if (mID > 1000000)
                {
                    System.Threading.Thread.Sleep(50);
                    goto START;
                }
                result |= (ulong)(mSeconds + mLastTime) << 20;
                result |= mID;
                return result;
            }
        }
    }
}
