using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BeetleX.FastHttpApi
{

    class SingleThreadDispatcher<T> : IDisposable
    {
        public SingleThreadDispatcher(Action<T> process)
        {
            Process = process;
            mQueue = new System.Collections.Concurrent.ConcurrentQueue<T>();

        }

        private long mCount = 0;

        private int mRunStatus = 0;

        private Action<T> Process;

        private System.Collections.Concurrent.ConcurrentQueue<T> mQueue;

        public Action<T, Exception> ProcessError { get; set; }

        public void Enqueue(T item)
        {
            mQueue.Enqueue(item);
            System.Threading.Interlocked.Increment(ref mCount);
            InvokeStart();
        }

        public long Count => mCount;

        private T Dequeue()
        {
            T item;
            if (mQueue.TryDequeue(out item))
            {
                System.Threading.Interlocked.Decrement(ref mCount);
            }
            return item;
        }

        private void InvokeStart()
        {
            if (System.Threading.Interlocked.CompareExchange(ref mRunStatus, 1, 0) == 0)
            {
                if (mCount > 0)
                {
                    ThreadPool.QueueUserWorkItem(OnStart);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref mRunStatus, 0);
                }
            }
        }

        private void OnStart(object state)
        {
            while (true)
            {
                T item = Dequeue();
                if (item != null)
                {

                    try
                    {
                        Process(item);
                    }
                    catch (Exception e_)
                    {
                        try
                        {
                            if (ProcessError != null)
                                ProcessError(item, e_);
                        }
                        catch { }
                    }
                }
                else
                {
                    break;
                }
            }
            System.Threading.Interlocked.Exchange(ref mRunStatus, 0);
            InvokeStart();
        }

        public void Start()
        {
            InvokeStart();
        }

        public void Dispose()
        {
            mQueue.Clear();
        }
    }

    class FileLog
    {
        public FileLog()
        {
            mLogPath = System.IO.Directory.GetCurrentDirectory() +
                System.IO.Path.DirectorySeparatorChar + "logs" + System.IO.Path.DirectorySeparatorChar;
            if (!System.IO.Directory.Exists(mLogPath))
            {
                System.IO.Directory.CreateDirectory(mLogPath);
            }
            mDispatcher = new SingleThreadDispatcher<ServerLogEventArgs>(OnWriteLog);
        }

        private string mLogPath;

        private int FileIndex = 0;

        private SingleThreadDispatcher<ServerLogEventArgs> mDispatcher;

        private System.IO.StreamWriter mWriter;

        private int mWriteCount;

        protected System.IO.StreamWriter GetWriter()
        {
            if (mWriter == null || mWriter.BaseStream.Length > 1024 * 1024 * 20)
            {
                if (mWriter != null)
                {
                    mWriter.Flush();
                    mWriter.Close();
                }
                string filename;
                do
                {
                    filename = mLogPath + DateTime.Now.ToString("yyyyMMdd") + "_" + ++FileIndex + ".txt";
                } while (System.IO.File.Exists(filename));
                mWriter = new System.IO.StreamWriter(filename, false, Encoding.UTF8);

            }
            return mWriter;

        }



        private void OnWriteLog(ServerLogEventArgs e)
        {
            mWriteCount++;
            System.IO.StreamWriter writer = GetWriter();
            writer.Write("[");
            writer.Write(DateTime.Now);
            writer.Write("] [");
            writer.Write(e.Type.ToString());
            writer.Write("] ");
            writer.WriteLine(e.Message);
            if (mWriteCount > 200 || mDispatcher.Count == 0)
            {
                writer.Flush();
                mWriteCount = 0;
            }
        }

        public void Add(ServerLogEventArgs e)
        {
            mDispatcher.Enqueue(e);
        }

        public void Run()
        {

        }
    }
}
