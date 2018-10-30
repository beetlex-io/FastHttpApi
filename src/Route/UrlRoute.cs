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

        public int ID { get; set; }

        public bool UrlIgnoreCase { get; set; }

        private List<string> mItems = new List<string>();

        public string Path { get; set; }

        public void Init()
        {
            string parent = @"(\{[A-Za-z0-9]+\})";
            int index = 0;
            if (UrlIgnoreCase)
            {
                Url = Url.ToLower();
                Rewrite = Rewrite.ToLower();
            }
            for (int i = 0; i < Url.Length; i++)
            {
                if (Url[i] == '/')
                    index = i;
                else if (Url[i] == '{')
                    break;
            }
            Path = Url.Substring(0, index + 1);
            Valid = Regex.IsMatch(Url, parent);
            ID = Path.GetHashCode();
            ReExt = HttpParse.GetBaseUrlExt(Rewrite);
            TemplateMatch = new RouteTemplateMatch(Url, Path.Length);
        }

        public RouteTemplateMatch TemplateMatch { get; set; }

        public string Ext { get; set; }

        public string ReExt { get; set; }

        public bool Valid { get; set; }

        public string Url { get; set; }

        public string Rewrite { get; set; }

        public bool Match(string url, QueryString queryString)
        {
            return TemplateMatch.Execut(url, queryString);
        }
    }
}
