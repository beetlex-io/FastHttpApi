using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public interface IEventWork : IDisposable
    {
        Task Execute();
    }

    public class NextQueue : IDisposable
    {
        public NextQueue()
        {
            mQueue = new System.Collections.Concurrent.ConcurrentQueue<IEventWork>();
            ID = System.Threading.Interlocked.Increment(ref mID);
        }

        public long ID { get; set; }

        private static long mID;

        private readonly object _workSync = new object();

        private bool _doingWork;

        private int mCount;

        private System.Collections.Concurrent.ConcurrentQueue<IEventWork> mQueue;

        public int Count => mCount;

        public void Enqueue(IEventWork item)
        {
            mQueue.Enqueue(item);
            System.Threading.Interlocked.Increment(ref mCount);
            lock (_workSync)
            {
                if (!_doingWork)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(OnStart);
                    _doingWork = true;
                }
            }
        }

        private void OnError(Exception e, IEventWork work)
        {
            try
            {
                Error?.Invoke(e, work);
            }
            catch
            {

            }
        }

        public static Action<Exception, IEventWork> Error { get; set; }

        private async void OnStart(object state)
        {
            while (true)
            {
                while (mQueue.TryDequeue(out IEventWork item))
                {
                    System.Threading.Interlocked.Decrement(ref mCount);
                    using (item)
                    {
                        try
                        {
                            await item.Execute();
                        }
                        catch (Exception e_)
                        {
                            OnError(e_, item);
                        }
                    }
                }
                lock (_workSync)
                {
                    if (mQueue.IsEmpty)
                    {
                        try
                        {
                            Unused?.Invoke();
                        }
                        catch { }
                        _doingWork = false;
                        return;
                    }
                }
            }

        }

        public Action Unused { get; set; }

        public void Dispose()
        {
            while (mQueue.TryDequeue(out IEventWork work))
            {
                try
                {
                    work.Dispose();
                }
                catch
                {

                }

            }
        }
    }

    public class NextQueueGroup
    {

        private List<NextQueue> mQueues = new List<NextQueue>();

        public IList<NextQueue> Queues => mQueues;

        public NextQueueGroup(int count = 0)
        {
            if (count == 0)
            {
                count = Math.Min(Environment.ProcessorCount, 16);
            }

            for (int i = 0; i < count; i++)
                mQueues.Add(new NextQueue());
        }

        private long mIndex = 0;

        public void Enqueue(IEventWork item, int waitLength = 5)
        {
            for (int i = 0; i < mQueues.Count; i++)
            {
                if (mQueues[i].Count < waitLength)
                {
                    mQueues[i].Enqueue(item);
                    return;
                }
            }
            Next().Enqueue(item);
        }

        public NextQueue Next(int waitLength)
        {
            for (int i = 0; i < mQueues.Count; i++)
            {
                if (mQueues[i].Count < waitLength)
                {
                    return mQueues[i];

                }
            }
            return Next();
        }

        public NextQueue Next()
        {
            var index = System.Threading.Interlocked.Increment(ref mIndex);
            return mQueues[(int)(index % mQueues.Count)];
        }
        public NextQueue Has(int code)
        {
            return mQueues[Math.Abs(code) % mQueues.Count];
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ThreadQueueAttribute : Attribute
    {

        public ThreadQueueAttribute(ThreadQueueType type, int queues = 1)
        {
            Type = type;
            QueueGroup = new NextQueueGroup(queues);
            Count = queues;
        }

        public int Count { get; private set; }

        private static NextQueueGroup mUniqueQueueGroup = new NextQueueGroup(25 * Environment.ProcessorCount);

        public ThreadQueueAttribute(string uniqueName)
        {
            Type = ThreadQueueType.DataUnique;
            UniqueName = uniqueName;
        }

        public int Limit { get; set; } = 100;

        public string UniqueName
        {
            get; set;
        }
        public bool Enabled(NextQueue queue)
        {
            return queue.Count < Limit;
        }
        public NextQueueGroup QueueGroup { get; private set; }

        public ThreadQueueType Type { get; private set; }

        public NextQueue GetQueue(IHttpContext context)
        {
            if (Type == ThreadQueueType.Single)
                return QueueGroup.Queues[0];
            else if (Type == ThreadQueueType.Multiple)
                return QueueGroup.Next();
            else if (Type == ThreadQueueType.DataUnique)
            {
                string value = null;
                if (UniqueName != null)
                {
                    if (string.Compare(UniqueName, "$path", true) == 0)
                    {
                        value = context.Request.GetSourcePath();
                    }
                    else if(UniqueName.IndexOf("__")==0)
                    {
                        return mUniqueQueueGroup.Has(UniqueName.GetHashCode());
                    }
                    else
                    {
                        value = context.Request.Header[UniqueName];
                        if (value == null)
                            context.Data.TryGetString(UniqueName, out value);
                    }
                }
                if (value == null)
                    value = context.Request.GetSourceUrl();
                return mUniqueQueueGroup.Has(value.GetHashCode());
            }
            return QueueGroup.Next();
        }
    }

    public enum ThreadQueueType
    {
        None,
        Single,
        Multiple,
        DataUnique,
    }


}
