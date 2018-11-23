using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{

    class ObjectPoolGroup<T>
    {
        private List<ObjectPool<T>> objectPools = new List<ObjectPool<T>>();

        private long mIndex = 0;

        public ObjectPoolGroup(int maxItem = 5000)
        {
            for (int i = 0; i < Math.Min(Environment.ProcessorCount, 16); i++)
            {
                objectPools.Add(new ObjectPool<T>(maxItem));
            }
        }
        public bool Push(T data)
        {
            return objectPools[Math.Abs(data.GetHashCode()) % objectPools.Count].Push(data);
        }
        public bool TryPop(out T data)
        {
            return objectPools[(int)(++mIndex % objectPools.Count)].TryPop(out data);
        }

    }

    class ObjectPool<T>
    {

        public ObjectPool(int maxItems = 5000)
        {
            mMaxItems = maxItems;

        }

        private int mMaxItems;

        private System.Collections.Concurrent.ConcurrentStack<T> mQueues = new System.Collections.Concurrent.ConcurrentStack<T>();

        private int mCount;

        public bool Push(T data)
        {
            int value = System.Threading.Interlocked.Increment(ref mCount);
            if (value < mMaxItems)
            {
                mQueues.Push(data);
                return true;
            }
            else
            {
                System.Threading.Interlocked.Decrement(ref mCount);
                return false;
            }
        }

        public bool TryPop(out T data)
        {
            bool result;
            result = mQueues.TryPop(out data);
            if (result)
                System.Threading.Interlocked.Decrement(ref mCount);
            return result;
        }
    }
}
