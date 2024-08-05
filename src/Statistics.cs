using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class Statistics
    {

        const int COUNT = 701;

        public Statistics(string url)
        {

            All = new CodeStatistics(0, "All");
            Url = url;
        }

        private ConcurrentDictionary<int, CodeStatistics> mCodeStatistics = new ConcurrentDictionary<int, CodeStatistics>();

        public CodeStatistics GetOrAddCodeStatistics(int code)
        {
            CodeStatistics result = new CodeStatistics(code);
            if (!mCodeStatistics.TryAdd(code, result))
            {
                mCodeStatistics.TryGetValue(code, out result);
            }
            return result;
        }
        public CodeStatistics GetCodeStatistics(int code)
        {
            mCodeStatistics.TryGetValue(code, out CodeStatistics result);
            return result;
        }

        public string Url { get; set; }

        public CodeStatistics OtherStatus { get; private set; } = new CodeStatistics(0, "Other");

        public CodeStatistics Status_1xx { get; private set; } = new CodeStatistics(0, "1xx");

        public CodeStatistics Status_2xx { get; private set; } = new CodeStatistics(0, "2xx");

        public CodeStatistics Status_3xx { get; private set; } = new CodeStatistics(0, "3xx");

        public CodeStatistics Status_4xx { get; private set; } = new CodeStatistics(0, "4xx");

        public CodeStatistics Status_5xx { get; private set; } = new CodeStatistics(0, "5xx");

        public CodeStatistics All { get; private set; }

        public int ItemID { get; set; }

        public void Add(int code, long time)
        {
            All.Add(time);
            if (code >= 100 && code < 200)
                Status_1xx.Add(time);
            else if (code >= 200 && code < 300)
                Status_2xx.Add(time);
            else if (code >= 300 && code < 400)
                Status_3xx.Add(time);
            else if (code >= 400 && code < 500)
                Status_4xx.Add(time);
            else if (code >= 500 && code < 600)
                Status_5xx.Add(time);
            else
            {
                OtherStatus.Add(time);
            }
            if (code >= COUNT)
            {
                GetOrAddCodeStatistics(COUNT).Add(time);
            }
            else
            {
                GetOrAddCodeStatistics(code).Add(time);
            }
        }

        public StatisticsData ListStatisticsData(int code)
        {
            return GetOrAddCodeStatistics(code)?.GetData();
        }
        public StatisticsData[] ListStatisticsData()
        {
            return (from a in this.mCodeStatistics.Values where a.Count > 0 orderby a.Count descending select a.GetData()).ToArray();
        }
        public StatisticsData[] ListStatisticsData(params int[] codes)
        {
            List<StatisticsData> result = new List<StatisticsData>();
            foreach (var i in codes)
            {
                if (i < COUNT)
                {
                    var stat = GetCodeStatistics(i);
                    if (stat != null)
                        result.Add(stat.GetData());
                }
            }
            return result.ToArray();
        }

        public StatisticsData[] ListStatisticsData(int start, int end)
        {
            List<StatisticsData> result = new List<StatisticsData>();
            for (int i = start; i < end; i++)
            {
                var stat = GetCodeStatistics(i);
                if (stat != null)
                    result.Add(stat.GetData());
            }
            return result.ToArray();
        }

        public object ListStatisticsData(int start, int end, Func<StatisticsData, object> selectObj)
        {
            var result = (from item in this.mCodeStatistics.Values
                          where item.Count > 0 && item.Code >= start && item.Code < end
                          select selectObj(item.GetData())).ToArray();
            return result;
        }

        public CodeStatistics[] List(Func<CodeStatistics, bool> filters = null)
        {
            if (filters == null)
                return (from a in this.mCodeStatistics.Values where a.Count > 0 orderby a.Count descending select a).ToArray();
            else
                return (from a in this.mCodeStatistics.Values where a.Count > 0 && filters(a) orderby a.Count descending select a).ToArray();
        }

        public StatisticsGroup GetData()
        {
            StatisticsGroup result = new StatisticsGroup(this);
            result.Url = Url;
            result.Other = OtherStatus.GetData().Copy();
            result._1xx = Status_1xx.GetData().Copy();
            result._2xx = Status_2xx.GetData().Copy();
            result._3xx = Status_3xx.GetData().Copy();
            result._4xx = Status_4xx.GetData().Copy();
            result._5xx = Status_5xx.GetData().Copy();
            result.All = All.GetData().Copy();
            return result;
        }
    }

    public class CodeStatistics
    {
        public CodeStatistics(int code, string name = null)
        {
            Code = code;
            if (name == null)
                name = code.ToString();
            mLastTime = BeetleX.TimeWatch.GetTotalSeconds();
            Name = name;
            mDelayStats.Add(new TimeDelayData(0, 0, 10, 0, 0));
            mDelayStats.Add(new TimeDelayData(1, 10, 20, 0, 0));
            mDelayStats.Add(new TimeDelayData(2, 20, 50, 0, 0));
            mDelayStats.Add(new TimeDelayData(3, 50, 100, 0, 0));
            mDelayStats.Add(new TimeDelayData(4, 100, 200, 0, 0));
            mDelayStats.Add(new TimeDelayData(5, 200, 500, 0, 0));
            mDelayStats.Add(new TimeDelayData(6, 500, 1000, 0, 0));
            mDelayStats.Add(new TimeDelayData(7, 1000, 2000, 0, 0));
            mDelayStats.Add(new TimeDelayData(8, 2000, 5000, 0, 0));
            mDelayStats.Add(new TimeDelayData(9, 5000, 10000, 0, 0));
            mDelayStats.Add(new TimeDelayData(10, 10000, 0, 0, 0));

        }

        private List<TimeDelayData> mDelayStats = new List<TimeDelayData>();

        public int Code { get; private set; }

        public string Name { get; set; }

        private long mCount;

        public long Count => mCount;

        private double mLastTime;

        private long mLastCount;

        public int Rps
        {
            get
            {
                double time = TimeWatch.GetTotalSeconds() - mLastTime;
                int value = (int)((double)(mCount - mLastCount) / time);
                mLastTime = TimeWatch.GetTotalSeconds();
                mLastCount = mCount;
                return value;
            }
        }

        public void Add(long time)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            if (time <= 10)
                System.Threading.Interlocked.Increment(ref ms10);
            else if (time <= 20)
                System.Threading.Interlocked.Increment(ref ms20);
            else if (time <= 50)
                System.Threading.Interlocked.Increment(ref ms50);
            else if (time <= 100)
                System.Threading.Interlocked.Increment(ref ms100);
            else if (time <= 200)
                System.Threading.Interlocked.Increment(ref ms200);
            else if (time <= 500)
                System.Threading.Interlocked.Increment(ref ms500);
            else if (time <= 1000)
                System.Threading.Interlocked.Increment(ref ms1000);
            else if (time <= 2000)
                System.Threading.Interlocked.Increment(ref ms2000);
            else if (time <= 5000)
                System.Threading.Interlocked.Increment(ref ms5000);
            else if (time <= 10000)
                System.Threading.Interlocked.Increment(ref ms10000);
            else
                System.Threading.Interlocked.Increment(ref msOther);
        }

        public override string ToString()
        {
            return mCount.ToString();
        }

        private long ms10;

        private long ms10LastCount;

        public long Time10ms => ms10;

        private long ms20;

        private long ms20LastCount;

        public long Time20ms => ms20;

        private long ms50;

        private long ms50LastCount;

        public long Time50ms => ms50;

        private long ms100;

        private long ms100LastCount;

        public long Time100ms => ms100;

        private long ms200;

        private long ms200LastCount;

        public long Time200ms => ms200;

        private long ms500;

        private long ms500LastCount;

        public long Time500ms => ms500;

        private long ms1000;

        private long ms1000LastCount;

        public long Time1000ms => ms1000;

        private long ms2000;

        private long ms2000LastCount;

        public long Time2000ms => ms2000;

        private long ms5000;

        private long ms5000LastCount;

        public long Time5000ms => ms5000;

        private long ms10000;

        private long ms10000LastCount;

        public long Time10000ms => ms10000;

        private long msOther;

        private long msOtherLastCount;

        public long TimeOtherms => msOther;

        private double mLastRpsTime = 0;

        private int mGetStatus = 0;

        private int mGetDelayStatus = 0;

        public List<TimeDelayData> GetDelay()
        {
            if (TimeWatch.GetTotalSeconds() - mLastRpsTime >= 1)
            {
                if (System.Threading.Interlocked.CompareExchange(ref mGetDelayStatus, 1, 0) == 0)
                {
                    mDelayStats[0].Count = Time10ms;
                    mDelayStats[1].Count = Time20ms;
                    mDelayStats[2].Count = Time50ms;
                    mDelayStats[3].Count = Time100ms;
                    mDelayStats[4].Count = Time200ms;
                    mDelayStats[5].Count = Time500ms;
                    mDelayStats[6].Count = Time1000ms;
                    mDelayStats[7].Count = Time2000ms;
                    mDelayStats[8].Count = Time5000ms;
                    mDelayStats[9].Count = Time10000ms;
                    mDelayStats[10].Count = TimeOtherms;
                    double now = TimeWatch.GetTotalSeconds();
                    double time = now - mLastRpsTime;

                    mDelayStats[0].Rps = (int)((double)(ms10 - ms10LastCount) / time);
                    ms10LastCount = ms10;

                    mDelayStats[1].Rps = (int)((double)(ms20 - ms20LastCount) / time);
                    ms20LastCount = ms20;

                    mDelayStats[2].Rps = (int)((double)(ms50 - ms50LastCount) / time);
                    ms50LastCount = ms50;

                    mDelayStats[3].Rps = (int)((double)(ms100 - ms100LastCount) / time);
                    ms100LastCount = ms100;

                    mDelayStats[4].Rps = (int)((double)(ms200 - ms200LastCount) / time);
                    ms200LastCount = ms200;

                    mDelayStats[5].Rps = (int)((double)(ms500 - ms500LastCount) / time);
                    ms500LastCount = ms500;

                    mDelayStats[6].Rps = (int)((double)(ms1000 - ms1000LastCount) / time);
                    ms1000LastCount = ms1000;

                    mDelayStats[7].Rps = (int)((double)(ms2000 - ms2000LastCount) / time);
                    ms2000LastCount = ms2000;

                    mDelayStats[8].Rps = (int)((double)(ms5000 - ms5000LastCount) / time);
                    ms5000LastCount = ms5000;

                    mDelayStats[9].Rps = (int)((double)(ms10000 - ms10000LastCount) / time);
                    ms10000LastCount = ms10000;

                    mDelayStats[10].Rps = (int)((double)(msOther - msOtherLastCount) / time);
                    msOtherLastCount = msOther;

                    mLastRpsTime = now;
                    mGetDelayStatus = 0;
                }
            }
            return mDelayStats;
        }

        private StatisticsData mStatisticsData = new StatisticsData();

        public StatisticsData GetData()
        {
            if (TimeWatch.GetTotalSeconds() - mStatisticsData.CreateTime >= 1)
                if (System.Threading.Interlocked.CompareExchange(ref mGetStatus, 1, 0) == 0)
                {
                    StatisticsData result = mStatisticsData;
                    result.CreateTime = TimeWatch.GetTotalSeconds();
                    result.Count = Count;
                    result.Rps = Rps;
                    result.Name = Name;
                    result.TimeDelays = GetDelay();
                    mGetStatus = 0;
                }
            return mStatisticsData;
        }

    }

    public class StatisticsGroup
    {
        public StatisticsGroup(Statistics statistics)
        {
            Statistics = statistics;
        }

        public Statistics Statistics { get; set; }

        public String Url { get; set; }

        public string Server { get; set; }

        public StatisticsData All { get; set; }

        public StatisticsData Other { get; set; }

        public StatisticsData _1xx { get; set; }
        public StatisticsData _2xx { get; set; }
        public StatisticsData _3xx { get; set; }
        public StatisticsData _4xx { get; set; }
        public StatisticsData _5xx { get; set; }


    }

    public class StatisticsData
    {
        public StatisticsData()
        {

        }

        public string Name { get; set; }

        public long Count { get; set; }

        public long Rps { get; set; }

        public List<TimeDelayData> TimeDelays { get; set; } = new List<TimeDelayData>();

        internal double CreateTime { get; set; }

        public StatisticsData Copy()
        {
            return new StatisticsData { Rps = Rps, Count = Count, Name = Name };
        }

    }

    public class TimeDelayData
    {
        public TimeDelayData(int index, int start, int end, long count, long rps)
        {
            StartTime = start;
            EndTime = end;
            Count = count;
            Rps = rps;
            Index = index;
        }

        public int Index { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public long Count { get; set; }

        public long Rps { get; set; }

        public string Name
        {
            get
            {
                string name;
                if (StartTime > 0 && EndTime > 0)
                {
                    if (StartTime >= 1000)
                        name = $"{StartTime / 1000}s";
                    else
                        name = $"{StartTime}ms";

                    if (EndTime >= 1000)
                        name += $"-{EndTime / 1000}s";
                    else
                        name += $"-{EndTime}ms";

                }
                else if (StartTime > 0)
                {
                    if (StartTime >= 1000)
                        name = $">{StartTime / 1000}s";
                    else
                        name = $">{StartTime}ms";
                }
                else
                {
                    name = $"<{EndTime}ms";
                }
                return name;
            }
        }
    }
}
