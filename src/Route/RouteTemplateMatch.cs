using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class RouteTemplateMatch
    {
        public RouteTemplateMatch(string value, int offset = 0)
        {
            mTemplate = value;
            mOffset = offset;
            Init();
        }

        private int mOffset;

        private List<MatchItem> mItems = new List<MatchItem>();

        private string mTemplate;

        private void Init()
        {
            MatchItem item = new MatchItem();
            bool first = true;
            bool inmatch = false;
            int offset = 0;
            for (int i = mOffset; i < mTemplate.Length; i++)
            {
                if (mTemplate[i] == '{')
                {
                    if (!first)
                        item = new MatchItem();
                    inmatch = true;
                    offset = i;
                }
                else if (mTemplate[i] == '}')
                {
                    if (first)
                    {
                        first = false;
                    }
                    item.Name = new string(mTemplate.AsSpan().Slice(offset + 1, i - offset - 1));
                    mItems.Add(item);
                    inmatch = false;
                }
                else
                {
                    if (mItems.Count > 0 && !inmatch)
                        item.Eof += mTemplate[i];
                    if (mItems.Count == 0 && !inmatch)
                        item.Start += mTemplate[i];
                }
            }
        }

        public bool Execut(string url, QueryString queryString)
        {
            if (mItems.Count == 0)
            {
                return url == mTemplate;
            }
            else
            {
                int offset = mOffset;
                for (int i = 0; i < mItems.Count; i++)
                {
                    MatchItem item = mItems[i];
                    string value;
                    var count = item.Match(url, offset, out value);
                    if (count <= 0)
                        return false;
                    queryString.Add(item.Name, value);
                    offset += count;
                }
                return true;
            }
        }

        class MatchItem
        {
            public string Start;

            public string Name;

            public string Eof;

            public int Match(string url, int offset, out string value)
            {
                int count = 0;
                value = "";
                int valueoffset = -1;
                int length = url.Length;
                if (Start != null)
                {
                    for (int k = 0; k < Start.Length; k++)
                    {
                        if (offset + k < length)
                        {
                            if (Start[k] != url[offset + k])
                                return -1;
                        }
                        else
                            return -1;
                    }
                    offset = offset + Start.Length;
                    count += Start.Length;
                }
                for (int i = offset; i < length; i++)
                {
                    if (Eof != null && url[i] == Eof[0])
                    {
                        value = url.Substring(valueoffset, i - valueoffset);
                        valueoffset = -1;
                        for (int k = 1; k < Eof.Length; k++)
                        {
                            if (url[i + k] != Eof[k])
                                return -1;
                        }
                        count += Eof.Length;
                        break;
                    }
                    else
                    {
                        if (valueoffset == -1)
                            valueoffset = i;
                        count++;
                    }
                }
                if (valueoffset != -1)
                    value = url.Substring(valueoffset, length - valueoffset);
                return count;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in mItems)
            {
                sb.Append(item.Start).Append(item.Name).Append(item.Eof);
            }
            return sb.ToString();
        }
    }
}
