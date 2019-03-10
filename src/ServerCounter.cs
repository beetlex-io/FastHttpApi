using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ServerCounter
    {
        public ServerCounter(HttpApiServer server)
        {
            mProcessorCount = Environment.ProcessorCount;
            mTotalMemory = Environment.WorkingSet;
            mServer = server;
            mLastTime = mServer.BaseServer.GetRunTime();
            mCpuMaxTime = mProcessorCount * 1000;
            mProcess = System.Diagnostics.Process.GetCurrentProcess();
            mLastTotalProcessorTime = mProcess.TotalProcessorTime.Milliseconds;


        }

        private System.Diagnostics.Process mProcess;

        private long mCpuMaxTime;

        private long mTotalMemory;

        private HttpApiServer mServer;

        private long mLastTime;

        private int mProcessorCount;

        private long mLastTotalRequest;

        private long mLastTotalConnections;

        private double mLastSendBytes;

        private double mLastReceiveBytes;

        private double mLastTotalProcessorTime;

        private ServerStatus mInfo = new ServerStatus();

        private long mLastNextTime;

        private int mNextStatu = 0;

        public ServerStatus Next()
        {
            if (mServer.BaseServer.GetRunTime() - mLastNextTime > 1000)
            {
                if (System.Threading.Interlocked.CompareExchange(ref mNextStatu, 1, 0) == 0)
                {
                    mLastNextTime = mServer.BaseServer.GetRunTime();
                    ServerStatus result = new ServerStatus();
                    result.BeetleXVersion = mServer.BaseServer.GetType().Assembly.GetName().Version.ToString();
                    result.WebApiVersion = mServer.GetType().Assembly.GetName().Version.ToString();
                    result.ServerName = mServer.Name;
                    result.Host = mServer.Options.Host;
                    result.Port = mServer.Options.Port;
                    TimeSpan ts = (DateTime.Now - mServer.StartTime);
                    result.RunTime = $"{(long)ts.Days}:{(long)ts.Hours}:{(long)ts.Minutes}:{(long)ts.Seconds}";

                    long time = mServer.BaseServer.GetRunTime();
                    double second = (double)(time - mLastTime) / 1000d;
                    mLastTime = time;
                    double cputime = mProcess.TotalProcessorTime.TotalMilliseconds;
                    long cpufulltime = (long)(second * mCpuMaxTime);
                    double usetime = cputime - mLastTotalProcessorTime;
                    mLastTotalProcessorTime = cputime;
                    result.Cpu = (int)(((double)usetime / (double)cpufulltime) * 10000) / 100d;

                    result.Memory = Environment.WorkingSet / 1024;
                    result.TotalMemory = Environment.WorkingSet / 1024;

                    result.CurrentConnectinos = mServer.BaseServer.Count;
                    result.CurrentHttpRequest = (long)mServer.CurrentHttpRequests;
                    result.CurrentRequest = result.CurrentHttpRequest;// + result.CurrentWSRequest;
                                                                      // result.CurrentWSRequest = (long)mServer.CurrentWebSocketRequests;

                    result.TotalRequest = mServer.TotalRequest;
                    result.RequestPer = (long)((result.TotalRequest - mLastTotalRequest) / second);
                    mLastTotalRequest = result.TotalRequest;

                    result.TotalConnections = mServer.TotalConnections;
                    result.ConnectionsPer = (long)((result.TotalConnections - mLastTotalConnections) / second);
                    mLastTotalConnections = result.TotalConnections;

                    result.TotalReceiveBytes = mServer.BaseServer.ReceivBytes;
                    result.ReceiveBytesPer = ((result.TotalReceiveBytes - mLastReceiveBytes) / second);
                    mLastReceiveBytes = result.TotalReceiveBytes;
                    result.TotalReceiveBytes = GetByteMB(result.TotalReceiveBytes);
                    result.ReceiveBytesPer = GetByteMB(result.ReceiveBytesPer);

                    result.TotalSendBytes = mServer.BaseServer.SendBytes;
                    result.SendBytesPer = ((result.TotalSendBytes - mLastSendBytes) / second);
                    mLastSendBytes = result.TotalSendBytes;
                    result.TotalSendBytes = GetByteMB(result.TotalSendBytes);
                    result.SendBytesPer = GetByteMB(result.SendBytesPer);

                    foreach (var item in mServer.ActionFactory.Handlers)
                    {
                        ActionStatus actionStatus = new ActionStatus();
                        actionStatus.ID = item.ID;
                        actionStatus.Path = item.Path;
                        actionStatus.Version = item.Version;
                        actionStatus.AssmblyName = item.AssmblyName;
                        actionStatus.MaxRPS = item.MaxRPS;
                        actionStatus.Url = item.SourceUrl;
                        actionStatus.Requests = item.Requests;
                        actionStatus.RequestsPer = (long)((item.Requests - item.LastRequests) / second);
                        actionStatus.Errors = item.Errors;
                        actionStatus.ErrorsPer = (long)((item.Errors - item.LastErrors) / second);
                        item.LastErrors = item.Errors;
                        item.LastRequests = item.Requests;
                        result.Actions.Add(actionStatus);
                    }
                    result.Actions.Sort((o, e) => o.Url.CompareTo(e.Url));
                    mInfo = result;
                    System.Threading.Interlocked.Exchange(ref mNextStatu, 0);
                }
            }
            return mInfo;
        }

        private double GetByteMB(double value)
        {
            return (long)((value / (double)(1024 * 1024)) * 10000) / 10000d;
        }

        public class ServerStatus
        {
            public ServerStatus()
            {
                Actions = new List<ActionStatus>();
            }

            public string BeetleXVersion { get; set; }

            public string WebApiVersion { get; set; }

            public string Host { get; set; }

            public int Port { get; set; }

            public string RunTime { get; set; }

            public string ServerName { get; set; }

            public long TotalRequest { get; set; }

            public long TotalMemory { get; set; }

            public long RequestPer { get; set; }

            public long ConnectionsPer { get; set; }

            public long TotalConnections { get; set; }

            public long CurrentConnectinos { get; set; }

            public long CurrentRequest { get; set; }

            //  public long CurrentWSRequest { get; set; }

            public long CurrentHttpRequest { get; set; }

            public double TotalSendBytes { get; set; }

            public double TotalReceiveBytes { get; set; }

            public double SendBytesPer { get; set; }

            public double ReceiveBytesPer { get; set; }

            public long Memory { get; set; }

            public double Cpu { get; set; }

            public List<ActionStatus> Actions { get; set; }

        }

        public class ActionStatus
        {

            public int ID { get; set; }

            public string Path { get; set; }

            public string Version { get; set; }

            public string AssmblyName { get; set; }

            public int MaxRPS { get; set; }

            public string Url { get; set; }

            public long Requests { get; set; }

            public long RequestsPer { get; set; }

            public long Errors { get; set; }

            public long ErrorsPer { get; set; }

        }
    }
}
