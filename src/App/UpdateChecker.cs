using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MP1.ConfigurationStudio
{
    internal static class UpdateChecker
    {
        internal sealed class Result
        {
            public string Latest;
            public string Url;
            public bool Newer;
            public string Error;
        }

        public static string CurrentVersion
        {
            get
            {
                var attribute = Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                string value = attribute == null ? null : attribute.InformationalVersion;
                if (String.IsNullOrWhiteSpace(value))
                {
                    Version version = Assembly.GetExecutingAssembly().GetName().Version;
                    value = version == null ? "0.0.0" : version.ToString(3);
                }
                int hash = value.IndexOf('+');
                return hash > 0 ? value.Substring(0, hash) : value;
            }
        }

        public static Result Check(string current)
        {
            var result = new Result { Latest = current };
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/gheitmann/MP1-Configuration-Studio/releases?per_page=10");
                request.UserAgent = "MP1-Configuration-Studio";
                request.Accept = "application/vnd.github+json";
                request.Timeout = 8000;
                request.ReadWriteTimeout = 8000;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    MatchCollection tags = Regex.Matches(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                    if (tags.Count == 0)
                    {
                        result.Error = "No release found.";
                        return result;
                    }
                    var urls = new List<string>();
                    foreach (Match match in Regex.Matches(json, "\"html_url\"\\s*:\\s*\"([^\"]+)\""))
                    {
                        string value = match.Groups[1].Value;
                        if (value.IndexOf("/releases/tag/", StringComparison.OrdinalIgnoreCase) >= 0) urls.Add(value);
                    }
                    string best = null;
                    string bestUrl = null;
                    for (int i = 0; i < tags.Count; i++)
                    {
                        string tag = tags[i].Groups[1].Value;
                        if (best == null || Compare(tag, best) > 0)
                        {
                            best = tag;
                            bestUrl = i < urls.Count ? urls[i] : null;
                        }
                    }
                    result.Latest = best;
                    result.Url = bestUrl ?? "https://github.com/gheitmann/MP1-Configuration-Studio/releases";
                    result.Newer = Compare(best, current) > 0;
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        public static int RunCli(string currentOverride = null)
        {
            string current = String.IsNullOrWhiteSpace(currentOverride) ? CurrentVersion : currentOverride;
            Result result = Check(current);
            Console.WriteLine("current=" + current);
            Console.WriteLine("latest=" + result.Latest);
            Console.WriteLine("url=" + result.Url);
            Console.WriteLine("newer=" + (result.Newer ? "yes" : "no"));
            if (!String.IsNullOrEmpty(result.Error)) Console.WriteLine("error=" + result.Error);
            Console.Out.Flush();
            return String.IsNullOrEmpty(result.Error) ? 0 : 1;
        }

        internal static int Compare(string left, string right)
        {
            int[] first = Parts(left);
            int[] second = Parts(right);
            for (int i = 0; i < Math.Max(first.Length, second.Length); i++)
            {
                int a = i < first.Length ? first[i] : 0;
                int b = i < second.Length ? second[i] : 0;
                if (a != b) return a.CompareTo(b);
            }
            return 0;
        }

        private static int[] Parts(string version)
        {
            string value = (version ?? string.Empty).Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase)) value = value.Substring(1);
            string pre = string.Empty;
            int dash = value.IndexOf('-');
            if (dash >= 0)
            {
                pre = value.Substring(dash + 1);
                value = value.Substring(0, dash);
            }
            var parts = new List<int>();
            foreach (string part in value.Split('.'))
            {
                int number;
                Int32.TryParse(part, out number);
                parts.Add(number);
            }
            while (parts.Count < 3) parts.Add(0);
            int beta = 0;
            Match match = Regex.Match(pre, "(?i)beta\\.?(\\d+)");
            if (match.Success) Int32.TryParse(match.Groups[1].Value, out beta);
            parts.Add(pre.Length == 0 ? Int32.MaxValue : beta);
            return parts.ToArray();
        }
    }
}
