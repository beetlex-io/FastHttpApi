using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{


    class ObjectPoolGroup<T>
    {
        private List<ObjectPool<T>> objectPools = new List<ObjectPool<T>>();

        private long mIndex = 0;

        public ObjectPoolGroup()
        {
            for (int i = 0; i < Math.Min(Environment.ProcessorCount, 16); i++)
            {
                objectPools.Add(new ObjectPool<T>());
            }
        }
        public void Push(T data)
        {
            objectPools[(data.GetHashCode() % objectPools.Count)].Push(data);
        }
        public bool TryPop(out T data)
        {
            return objectPools[(int)(++mIndex % objectPools.Count)].TryPop(out data);
        }

    }

    class ObjectPool<T>
    {
        private System.Collections.Concurrent.ConcurrentStack<T> mQueues = new System.Collections.Concurrent.ConcurrentStack<T>();

        public void Push(T data)
        {
            mQueues.Push(data);
        }

        public bool TryPop(out T data)
        {
            return mQueues.TryPop(out data);
        }
    }
}
