using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    class GMTDate
    {
        private List<byte[]> mWeekBuffers = new List<byte[]>();

        public List<byte[]> mYears = new List<byte[]>();

        public List<byte[]> mMoth = new List<byte[]>();

        public List<byte[]> mNumber = new List<byte[]>();

        private byte _1 = 58;

        private byte _s = 32;

        private byte _r = 13;

        private byte _n = 10;

        private byte[] GMT = new byte[] { 71, 77, 84 };

        private static GMTDate mDefault;

        public static GMTDate Default
        {
            get
            {
                if (mDefault == null)
                {
                    mDefault = new GMTDate();
                    mDefault.Init();
                }
                return mDefault;
            }
        }


        public GMTDate()
        {
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Sun"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Mon"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Tue"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Wed"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Thu"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Fri"));
            mWeekBuffers.Add(Encoding.ASCII.GetBytes("Sat"));
            for (int i = 1970; i < 1970 + 500; i++)
            {
                mYears.Add(Encoding.ASCII.GetBytes(i.ToString()));
            }
            for (int i = 0; i <= 100; i++)
            {
                mNumber.Add(Encoding.ASCII.GetBytes(i.ToString("00")));
            }
            mMoth.Add(Encoding.ASCII.GetBytes("Jan"));
            mMoth.Add(Encoding.ASCII.GetBytes("Feb"));
            mMoth.Add(Encoding.ASCII.GetBytes("Mar"));
            mMoth.Add(Encoding.ASCII.GetBytes("Apr"));
            mMoth.Add(Encoding.ASCII.GetBytes("May"));
            mMoth.Add(Encoding.ASCII.GetBytes("Jun"));
            mMoth.Add(Encoding.ASCII.GetBytes("Jul"));
            mMoth.Add(Encoding.ASCII.GetBytes("Aug"));
            mMoth.Add(Encoding.ASCII.GetBytes("Sep"));
            mMoth.Add(Encoding.ASCII.GetBytes("Oct"));
            mMoth.Add(Encoding.ASCII.GetBytes("Nov"));
            mMoth.Add(Encoding.ASCII.GetBytes("Dec"));
        }

        private System.Threading.Timer mUpdateTime;

        private void Init()
        {
            DATE = GetData(true);
            mUpdateTime = new System.Threading.Timer(o => {
                DATE = GetData(true);
            }, null, 1000, 1000);   
        }

        public ArraySegment<byte> DATE
        {
            get; set;
        }


        private ArraySegment<byte> GetData(bool inLine = false)
        {
            return GetData(DateTime.Now, inLine);
        }
        private ArraySegment<byte> GetData(DateTime date, bool inLine = false)
        {
            date = date.ToUniversalTime();
            int offset = 0;
            var GTM_BUFFER = new byte[50];
            Encoding.ASCII.GetBytes("Date: ", 0, 6, GTM_BUFFER, 0);
            offset = 6;
            var buffer = GTM_BUFFER;
            // week
            var sub = mWeekBuffers[(int)date.DayOfWeek];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = sub[2];
            offset++;
            buffer[offset] = 44;
            offset++;
            buffer[offset] = _s;
            offset++;
            //day
            sub = mNumber[date.Day];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = _s;
            offset++;
            //moth
            sub = mMoth[date.Month - 1];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = sub[2];
            offset++;
            buffer[offset] = _s;
            offset++;

            //year
            sub = mYears[date.Year - 1970];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = sub[2];
            offset++;
            buffer[offset] = sub[3];
            offset++;
            buffer[offset] = _s;
            offset++;

            //h
            sub = mNumber[date.Hour];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = _1;
            offset++;

            //m
            sub = mNumber[date.Minute];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = _1;
            offset++;

            //s
            sub = mNumber[date.Second];
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = _s;
            offset++;
            //GMT
            sub = GMT;
            buffer[offset] = sub[0];
            offset++;
            buffer[offset] = sub[1];
            offset++;
            buffer[offset] = sub[2];
            offset++;
            if (inLine)
            {
                buffer[offset] = _r;
                offset++;
                buffer[offset] = _n;
                offset++;
            }
            return new ArraySegment<byte>(GTM_BUFFER, 0, offset);
        }

    }
}
