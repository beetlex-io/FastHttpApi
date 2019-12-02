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


        public string ID { get; set; }

        public string Host { get; set; }

        public int GetPrefixCode()
        {
            if (Prefix == null)
                return 0;
            return (int)Prefix.Type;
        }

        public UrlPrefix Prefix { get; set; }

        public bool HasQueryString { get; set; } = false;

        public int PathLevel { get; set; }

        public bool UrlIgnoreCase { get; set; }

        private List<string> mItems = new List<string>();

        public string Path { get; private set; }

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
                {
                    index = i;
                    PathLevel++;
                }
                else if (Url[i] == '?')
                {
                    HasQueryString = true;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(Host))
            {
                this.Prefix = new UrlPrefix(Host);
            }
            // if (this.Prefix == null)
            Path = $"{Url.Substring(0, index + 1)}";
            ID = $"{Host}{Path}";
            //else
            //    Path = $"{this.Prefix.Value}{Url.Substring(0, index + 1)}";
            Valid = Regex.IsMatch(Url, parent);
            //TemplateMatch = new RouteTemplateMatch(Url, Path.Length);
            TemplateMatch = new RouteTemplateMatch(Url, 0);
            if (!string.IsNullOrEmpty(Rewrite))
            {
                ReExt = HttpParse.GetBaseUrlExt(Rewrite);
                HasRewriteParamters = Rewrite.IndexOf("{") >= 0;
            }
        }

        public bool HasRewriteParamters { get; set; } = false;

        public static long GetPathID(string path, long level)
        {

            return ((long)path.ToLower().GetHashCode() << 16 | (long)path.Length) << 8 | level;
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

    public class UrlPrefix
    {
        public UrlPrefix(string prefix)
        {
            var param = prefix.Split('=');
            if (param.Length == 1)
            {
                Name = param[0];
                Value = param[0];
                Type = PrefixType.Host;
            }
            else
            {
                Type = PrefixType.Param;
                Value = param[1];
                Name = param[0];

            }
        }

        public PrefixType Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string GetPrefix(HttpRequest request)
        {
            if (Type == PrefixType.Host)
                return request.GetHostBase();
            var value = request.Header[Name];
            if (value == null)
                value = request.Data[Name];
            return value;

        }

        public enum PrefixType:int
        {
            Host = 2,
            Param = 1
        }
    }
}
