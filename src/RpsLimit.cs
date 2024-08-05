using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class RpsLimit
    {
        public RpsLimit(int max)
        {
            mMax = max;
        }

        private int mMax;

        private long mRpsCount;

        private long mLastRpsTime;

        public void SetMaxRpx(int value)
        {
            this.mMax = value;
        }

        public bool Check(int max = 0)
        {
            if (max > 0)
                mMax = max;
            if (mMax <= 0)
                return false;
            else
            {
                mRpsCount = System.Threading.Interlocked.Increment(ref mRpsCount);
                long now = TimeWatch.GetElapsedMilliseconds();
                long time = now - mLastRpsTime;
                if (time >= 1000)
                {
                    System.Threading.Interlocked.Exchange(ref mRpsCount, 0);
                    System.Threading.Interlocked.Exchange(ref mLastRpsTime, now);
                }
                else
                {
                    if (mRpsCount > mMax)
                        return true;
                }
            }
            return false;
        }
    }
}
