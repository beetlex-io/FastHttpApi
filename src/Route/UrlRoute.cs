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
                if (!string.IsNullOrEmpty(Rewrite))
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
            ID = GetPathID(Path);
            TemplateMatch = new RouteTemplateMatch(Url, Path.Length);
            if (!string.IsNullOrEmpty(Rewrite))
            {
                ReExt = HttpParse.GetBaseUrlExt(Rewrite);
                HasRewriteParamters = Rewrite.IndexOf("{") >= 0;
            }
        }

        public bool HasRewriteParamters { get; set; } = false;

        public static int GetPathID(string path)
        {
            return path.ToLower().GetHashCode() << 16 | path.Length;
        }


        public string GetRewriteUrl(Dictionary<string, string> parameters)
        {
            if (parameters.Count == 0 || !HasRewriteParamters)
                return Rewrite;
            else
            {
                var buffer = HttpParse.GetCharBuffer();
                int count = 0;
                int nameIndex = 0;
                ReadOnlySpan<char> str = Rewrite;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '{')
                    {
                        nameIndex = i + 1;
                    }
                    else if (str[i] == '}')
                    {

                        if (nameIndex > 0)
                        {
                            string name = new string(str.Slice(nameIndex, i - nameIndex));
                            if (parameters.TryGetValue(name, out string value))
                            {
                                for (int l = 0; l < value.Length; l++)
                                {
                                    buffer[count] = value[l];
                                    count++;
                                }
                            }
                        }
                        nameIndex = 0;
                    }
                    else
                    {
                        if (nameIndex == 0)
                        {
                            buffer[count] = str[i];
                            count++;
                        }
                    }
                }
                return new string(buffer, 0, count);
            }
        }

        public RouteTemplateMatch TemplateMatch { get; set; }

        public string Ext { get; set; }

        public string ReExt { get; set; }

        public bool Valid { get; set; }

        public string Url { get; set; }

        public string Rewrite { get; set; }

        public bool Match(string url, Dictionary<string, string> parameters)
        {
            return TemplateMatch.Execute(url, parameters);
        }
    }
}
