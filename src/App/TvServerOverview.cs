using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace MP1.ConfigurationStudio
{
    internal static class TvServerOverview
    {
        public static int Run(string tvServerDirectory, string configurationDirectory)
        {
            try
            {
                if (String.IsNullOrEmpty(tvServerDirectory) || !Directory.Exists(tvServerDirectory)) return Fail("TV Server folder not found");
                if (!String.IsNullOrEmpty(configurationDirectory) && Directory.Exists(configurationDirectory)) Environment.CurrentDirectory = configurationDirectory;
                AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
                {
                    string name = args.Name.Split(',')[0];
                    string candidate = Path.Combine(tvServerDirectory, name + ".dll");
                    return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
                };
                Assembly businessAssembly = Assembly.LoadFrom(Path.Combine(tvServerDirectory, "TvBusinessLayer.dll"));
                Assembly databaseAssembly = Assembly.LoadFrom(Path.Combine(tvServerDirectory, "TVDatabase.dll"));
                Type businessType = businessAssembly.GetType("TvDatabase.TvBusinessLayer");
                if (businessType == null) return Fail("TvBusinessLayer not found");
                object business = Activator.CreateInstance(businessType);

                object tvChannelList = GetProperty(businessType, business, "Channels");
                int tvChannels = CountWhere(tvChannelList, "IsTv");
                int cards = Count(GetProperty(businessType, business, "Cards"));
                object radioChannelList = Call(businessType, business, "GetAllRadioChannels");
                int radioChannels = Count(radioChannelList);
                object groupList = StaticList(databaseAssembly, "TvDatabase.ChannelGroup");
                object radioGroupList = StaticList(databaseAssembly, "TvDatabase.RadioChannelGroup");
                object recordingList = StaticList(databaseAssembly, "TvDatabase.Recording");
                object scheduleList = StaticList(databaseAssembly, "TvDatabase.Schedule");
                int groups = Count(groupList);
                int radioGroups = Count(radioGroupList);
                int recordings = Count(recordingList);
                int schedules = Count(scheduleList);

                var output = new StringBuilder();
                output.AppendLine("ok=1");
                output.AppendLine("tvChannels=" + tvChannels);
                output.AppendLine("radioChannels=" + radioChannels);
                output.AppendLine("groups=" + groups);
                output.AppendLine("radioGroups=" + radioGroups);
                output.AppendLine("cards=" + cards);
                output.AppendLine("recordings=" + recordings);
                output.AppendLine("schedules=" + schedules);
                AppendNames(output, "tvChannel", tvChannelList, "DisplayName", "IsTv");
                AppendNames(output, "radioChannel", radioChannelList, "DisplayName");
                AppendNames(output, "group", groupList, "GroupName");
                AppendNames(output, "radioGroup", radioGroupList, "GroupName");
                AppendNames(output, "recording", recordingList, "Title");
                AppendNames(output, "schedule", scheduleList, "ProgramName");
                Console.Write(output.ToString());
                Console.Out.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        private static int Fail(string message)
        {
            Console.WriteLine("ok=0");
            Console.WriteLine("error=" + (message ?? string.Empty).Replace("\r", " ").Replace("\n", " "));
            Console.Out.Flush();
            return 1;
        }

        private static object GetProperty(Type type, object instance, string name)
        {
            try
            {
                PropertyInfo property = type.GetProperty(name);
                return property == null ? null : property.GetValue(instance, null);
            }
            catch { return null; }
        }

        private static object Call(Type type, object instance, string name)
        {
            try
            {
                MethodInfo method = type.GetMethod(name, Type.EmptyTypes);
                return method == null ? null : method.Invoke(instance, null);
            }
            catch { return null; }
        }

        private static int Count(object value)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null) return 0;
            int count = 0;
            foreach (object item in enumerable) count++;
            return count;
        }

        private static int CountWhere(object value, string property)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null) return 0;
            int count = 0;
            foreach (object item in enumerable)
            {
                try
                {
                    PropertyInfo info = item.GetType().GetProperty(property);
                    if (info != null && info.GetValue(item, null) is bool && (bool)info.GetValue(item, null)) count++;
                }
                catch { }
            }
            return count;
        }

        private static void AppendNames(StringBuilder output, string prefix, object value, string property, string filterProperty = null)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null) return;
            foreach (object item in enumerable)
            {
                if (item == null) continue;
                string text = null;
                try
                {
                    if (filterProperty != null)
                    {
                        PropertyInfo filter = item.GetType().GetProperty(filterProperty);
                        if (filter == null || !(filter.GetValue(item, null) is bool) || !(bool)filter.GetValue(item, null)) continue;
                    }
                    PropertyInfo info = item.GetType().GetProperty(property);
                    if (info != null) text = info.GetValue(item, null) as string;
                }
                catch { }
                if (text == null) text = item.ToString();
                text = (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
                output.AppendLine(prefix + "=" + text);
            }
        }

        private static int CountStatic(Assembly assembly, string typeName)
        {
            return Count(StaticList(assembly, typeName));
        }

        private static object StaticList(Assembly assembly, string typeName)
        {
            try
            {
                Type type = assembly.GetType(typeName);
                if (type == null) return null;
                MethodInfo method = type.GetMethod("ListAll", Type.EmptyTypes);
                return method == null ? null : method.Invoke(null, null);
            }
            catch { return null; }
        }

        private static void AppendNames(StringBuilder output, string prefix, object value, string property)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null) return;
            foreach (object item in enumerable)
            {
                if (item == null) continue;
                string text = null;
                try
                {
                    PropertyInfo info = item.GetType().GetProperty(property);
                    if (info != null) text = info.GetValue(item, null) as string;
                }
                catch { }
                if (text == null) text = item.ToString();
                text = (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
                output.AppendLine(prefix + "=" + text);
            }
        }
    }
}
