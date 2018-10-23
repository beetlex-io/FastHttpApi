using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace BeetleX.FastHttpApi.Route
{
    public class UrlRoute
    {

        public UrlRoute()
        {

        }

        internal void Init()
        {
            Valid = Regex.IsMatch(Url, @"(\{\d+\})");
            if (Valid)
            {
                Url = Regex.Replace(Url, @"(\{\d+\})", "([^/]+)");
            }
        }

        public string Ext { get; set; }

        public bool Valid { get; set; }

        public string Url { get; set; }

        public string Rewrite { get; set; }

        public bool Match(string url, out string result)
        {
            result = null;
            if (!Valid)
                return false;

            bool isMatch = false;
            var match = Regex.Match(url, Url, RegexOptions.IgnoreCase);
            if (match != null && match.Groups.Count > 1)
            {
                isMatch = true;
                int count = match.Groups.Count;
                string[] groups = new string[count - 1];
                for (int i = 1; i < count; i++)
                {
                    groups[i - 1] = match.Groups[i].Value;
                }
                result = string.Format(Rewrite, groups);
            }
            return isMatch;
        }
    }
}
