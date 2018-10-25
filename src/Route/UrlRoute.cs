using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace BeetleX.FastHttpApi
{
    public class UrlRoute
    {

        public UrlRoute()
        {

        }

        private int mStartIndex;

        private List<string> mItems = new List<string>();

        private string mUrLower;

        public string Path { get; set; }

        public void Init()
        {
            string parent = @"(\{[A-Za-z0-9]+\})";
            int index = 0;
            for (int i = 0; i < Url.Length; i++)
            {
                if (Url[i] == '/')
                    index = i;
                else if (Url[i] == '{')
                    break;
            }
            Path = Url.Substring(0, index + 1).ToLower();

            Valid = Regex.IsMatch(Url, parent);
            if (Valid)
            {
                var items = Regex.Matches(Url, parent);
                foreach (Match item in items)
                {
                    mItems.Add(item.Value.Replace("{", "").Replace("}", ""));
                }
                mUrLower = Url.ToLower();
                mStartIndex = Url.IndexOf("{") - 1;
                Url = "^" + Regex.Replace(Url, parent, @"([^/\.]+)");

            }

            RewriteLower = Rewrite.ToLower();
        }

        public string Ext { get; set; }

        public bool Valid { get; set; }

        public string Url { get; set; }

        public string Rewrite { get; set; }

        public string RewriteLower { get; set; }

        public bool Match(string url, out string rewrite, out string rewriteLower, QueryString queryString)
        {
            rewrite = Rewrite;
            rewriteLower = RewriteLower;
            if (!Valid || mStartIndex < 2 || url.Length < mStartIndex)
                return false;
            if ((url[0] != Url[0] && url[0] != mUrLower[0])
                ||
                (url[mStartIndex] != Url[mStartIndex] && url[mStartIndex] != mUrLower[mStartIndex])
                )
            {
                return false;
            }
            bool isMatch = false;
            var match = Regex.Match(url, Url, RegexOptions.IgnoreCase);
            if (match != null && match.Groups.Count == mItems.Count + 1)
            {
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    queryString.Add(mItems[i - 1], match.Groups[i].Value);
                }
                isMatch = true;
            }
            return isMatch;
        }
    }
}
