using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MP1.ConfigurationStudio
{
    internal static class NativeFilterCatalog
    {
        private static readonly Dictionary<NativeFilterKind, List<string>> cache = new Dictionary<NativeFilterKind, List<string>>();
        private static string runtimePath = AppDomain.CurrentDomain.BaseDirectory;

        public static void Configure(string runtime)
        {
            if (!String.IsNullOrEmpty(runtime)) runtimePath = runtime;
            cache.Clear();
        }

        public static IList<string> Names(NativeFilterKind kind)
        {
            lock (cache)
            {
                List<string> names;
                if (cache.TryGetValue(kind, out names)) return names;
                names = Load(kind);
                cache[kind] = names;
                return names;
            }
        }

        private static List<string> Load(NativeFilterKind kind)
        {
            var result = new List<string>();
            try
            {
                string library = Path.Combine(runtimePath, "DirectShowLib.dll");
                if (!File.Exists(library)) return result;
                Assembly assembly = Assembly.LoadFrom(library);
                Type devices = assembly.GetType("DirectShowLib.DsDevice");
                Type categories = assembly.GetType("DirectShowLib.FilterCategory");
                if (devices == null || categories == null) return result;
                string field = kind == NativeFilterKind.Video ? "VideoCompressorCategory"
                    : kind == NativeFilterKind.Audio ? "AudioCompressorCategory"
                    : kind == NativeFilterKind.AudioRenderer ? "AudioRendererCategory"
                    : "LegacyAmFilterCategory";
                FieldInfo categoryField = categories.GetField(field, BindingFlags.Public | BindingFlags.Static);
                MethodInfo enumerate = devices.GetMethod("GetDevicesOfCat", new[] { typeof(Guid) });
                if (categoryField == null || enumerate == null) return result;
                Array found = enumerate.Invoke(null, new object[] { (Guid)categoryField.GetValue(null) }) as Array;
                if (found == null) return result;
                foreach (object device in found)
                {
                    if (device == null) continue;
                    PropertyInfo name = device.GetType().GetProperty("Name");
                    string value = name == null ? null : name.GetValue(device, null) as string;
                    if (String.IsNullOrWhiteSpace(value)) continue;
                    if ((kind == NativeFilterKind.Video || kind == NativeFilterKind.Audio)
                        && (value.IndexOf("Encoder", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("Compressor", StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;
                    result.Add(value);
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }
            return result.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase).ToList();
        }
    }
}
