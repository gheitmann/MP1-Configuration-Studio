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

                int tvChannels = Count(GetProperty(businessType, business, "Channels"));
                int cards = Count(GetProperty(businessType, business, "Cards"));
                int radioChannels = Count(Call(businessType, business, "GetAllRadioChannels"));
                int groups = CountStatic(databaseAssembly, "TvDatabase.ChannelGroup");
                int radioGroups = CountStatic(databaseAssembly, "TvDatabase.RadioChannelGroup");
                int recordings = CountStatic(databaseAssembly, "TvDatabase.Recording");
                int schedules = CountStatic(databaseAssembly, "TvDatabase.Schedule");

                var output = new StringBuilder();
                output.AppendLine("ok=1");
                output.AppendLine("tvChannels=" + tvChannels);
                output.AppendLine("radioChannels=" + radioChannels);
                output.AppendLine("groups=" + groups);
                output.AppendLine("radioGroups=" + radioGroups);
                output.AppendLine("cards=" + cards);
                output.AppendLine("recordings=" + recordings);
                output.AppendLine("schedules=" + schedules);
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

        private static int CountStatic(Assembly assembly, string typeName)
        {
            try
            {
                Type type = assembly.GetType(typeName);
                if (type == null) return 0;
                MethodInfo method = type.GetMethod("ListAll", Type.EmptyTypes);
                return method == null ? 0 : Count(method.Invoke(null, null));
            }
            catch { return 0; }
        }
    }
}
