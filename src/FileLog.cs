using BeetleX.Dispatchs;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BeetleX.FastHttpApi
{

    public class FileLogWriter
    {
        public FileLogWriter(string type)
        {
            Type = type;
            mLogPath = System.IO.Directory.GetCurrentDirectory() +
                System.IO.Path.DirectorySeparatorChar + "logs" + System.IO.Path.DirectorySeparatorChar;
            if (!System.IO.Directory.Exists(mLogPath))
            {
                System.IO.Directory.CreateDirectory(mLogPath);
            }
            mDispatcher = new SingleThreadDispatcher<LogItem>(OnWriteLog);
        }

        public string Type { get; private set; }

        private string mLogPath;

        private int FileIndex = 0;

        private SingleThreadDispatcher<LogItem> mDispatcher;

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
                    filename = mLogPath + Type + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + ++FileIndex + ".txt";
                } while (System.IO.File.Exists(filename));
                mWriter = new System.IO.StreamWriter(filename, false, Encoding.UTF8);

            }
            return mWriter;

        }

        private void OnWriteLog(LogItem e)
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

        public void Add(LogType type, string message)
        {
            Add(new LogItem(type, message));
        }

        public void Add(LogItem e)
        {
            mDispatcher.Enqueue(e);
        }

        public class LogItem
        {
            public LogItem(LogType type, string message)
            {
                Type = type;
                Message = message;
            }
            public LogType Type;
            public string Message;
        }

        public void Run()
        {

        }
    }

    public class LogRecord
    {
        public string Type { get; set; }

        public string Time { get; set; }

        public string Message { get; set; }
    }
}
