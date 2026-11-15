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
        private static readonly Dictionary<NativeFilterKind, Dictionary<string, object>> devices = new Dictionary<NativeFilterKind, Dictionary<string, object>>();
        private static string runtimePath = AppDomain.CurrentDomain.BaseDirectory;

        public static void Configure(string runtime)
        {
            if (!String.IsNullOrEmpty(runtime)) runtimePath = runtime;
            cache.Clear();
            devices.Clear();
        }

        public static object Device(NativeFilterKind kind, string name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            Names(kind);
            Dictionary<string, object> map;
            object device;
            return devices.TryGetValue(kind, out map) && map.TryGetValue(name, out device) ? device : null;
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

        private static List<string> CoreNames(NativeFilterKind kind)
        {
            try
            {
                string corePath = Path.Combine(runtimePath, "Core.dll");
                if (!File.Exists(corePath)) return null;
                Assembly core = Assembly.LoadFrom(corePath);
                Type helper = core.GetType("DShowNET.Helper.FilterHelper");
                if (helper == null) return null;
                string method = kind == NativeFilterKind.Video ? "GetVideoCodec"
                    : kind == NativeFilterKind.Audio ? "GetAudioCodec"
                    : kind == NativeFilterKind.AudioRenderer ? "GetAudioRenderers"
                    : null;
                if (method == null) return null;
                MethodInfo invoke = helper.GetMethod(method, Type.EmptyTypes);
                if (invoke == null) return null;
                var values = invoke.Invoke(null, null) as System.Collections.IEnumerable;
                if (values == null) return null;
                var names = new List<string>();
                foreach (object item in values)
                    if (item != null && !String.IsNullOrWhiteSpace(item.ToString())) names.Add(item.ToString());
                return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch (Exception ex)
            {
                StudioLog.Warn("Filter catalog core lookup failed for " + kind + ": " + ex.Message);
                return null;
            }
        }

        private static List<string> Load(NativeFilterKind kind)
        {
            var result = new List<string>();
            var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            devices[kind] = map;
            try
            {
                string library = Path.Combine(runtimePath, "DirectShowLib.dll");
                StudioLog.Info("Filter catalog: path=" + library + " exists=" + File.Exists(library));
                if (!File.Exists(library)) return result;
                Assembly assembly = Assembly.LoadFrom(library);
                Type devices = assembly.GetType("DirectShowLib.DsDevice");
                Type categories = assembly.GetType("DirectShowLib.FilterCategory");
                StudioLog.Info("Filter catalog types: devices=" + (devices != null) + " categories=" + (categories != null));
                if (devices == null || categories == null) return result;
                string field = kind == NativeFilterKind.AudioRenderer ? "AudioRendererCategory" : "LegacyAmFilterCategory";
                FieldInfo categoryField = categories.GetField(field, BindingFlags.Public | BindingFlags.Static);
                MethodInfo enumerate = devices.GetMethod("GetDevicesOfCat", new[] { typeof(Guid) });
                StudioLog.Info("Filter catalog field=" + (categoryField != null) + " method=" + (enumerate != null));
                if (categoryField == null || enumerate == null) return result;
                Array found = enumerate.Invoke(null, new object[] { (Guid)categoryField.GetValue(null) }) as Array;
                StudioLog.Info("Filter catalog devices=" + (found == null ? 0 : found.Length));
                if (found == null) return result;
                foreach (object device in found)
                {
                    if (device == null) continue;
                    PropertyInfo name = device.GetType().GetProperty("Name");
                    string value = name == null ? null : name.GetValue(device, null) as string;
                    if (String.IsNullOrWhiteSpace(value)) continue;
                    if (!map.ContainsKey(value)) map[value] = device;
                }

                List<string> compatible = CoreNames(kind);
                if (compatible != null)
                {
                    compatible = compatible.Where(name => IsCodec(kind, name)).ToList();
                    StudioLog.Info("Filter catalog core=" + kind + " names=" + compatible.Count);
                    result.AddRange(compatible);
                    if (kind == NativeFilterKind.AudioRenderer)
                    {
                        var portal = map.Where(pair => pair.Key.IndexOf("MediaPortal", StringComparison.OrdinalIgnoreCase) >= 0).Select(pair => pair.Value).FirstOrDefault();
                        if (portal != null)
                        {
                            map["MediaPortal - Audio Renderer"] = portal;
                            map["MediaPortal Audio Renderer"] = portal;
                        }
                    }
                }
                else
                {
                    foreach (string value in map.Keys)
                    {
                        if (!IsCodec(kind, value)) continue;
                        result.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                StudioLog.Warn("Filter catalog failed for " + kind + ": " + ex.Message);
                return new List<string>();
            }
            return result.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>Keeps decoder/codec filters and removes encoders, muxers, parsers and other DirectShow utilities from codec lists.</summary>
        private static bool IsCodec(NativeFilterKind kind, string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return false;
            string value = name.Trim();
            if (kind == NativeFilterKind.AudioRenderer) return true;
            if (kind == NativeFilterKind.Legacy)
                return value.IndexOf("encoder", StringComparison.OrdinalIgnoreCase) < 0
                    && value.IndexOf("compressor", StringComparison.OrdinalIgnoreCase) < 0
                    && value.IndexOf("muxer", StringComparison.OrdinalIgnoreCase) < 0;
            string[] blocked = kind == NativeFilterKind.Video
                ? new[] { "encoder", "compressor", "muxer", "multiplexer", "parser", "splitter", "source", "renderer", "overlay", "allocator", "tee", "color space", "avi draw", "video port", "pbda", "vbi surface", "ditherer" }
                : new[] { "encoder", "compressor", "muxer", "multiplexer", "parser", "splitter", "source", "renderer", "overlay", "allocator", "tee", "color space", "avi draw", "video port", "pbda", "acm wrapper", "audioswitcher", "stereo input", "audio input", "audio output", "audio mic", "audio line", "hd audio", "hdmi out", "spdif out" };
            foreach (string word in blocked)
                if (value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return true;
        }
    }
}
