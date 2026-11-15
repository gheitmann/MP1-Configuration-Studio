using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class PluginConfigHelper
    {
        private const string SetupInterface = "MediaPortal.GUI.Library.ISetupForm";

        public static int List(string mediaPortalDirectory)
        {
            try
            {
                Prepare(mediaPortalDirectory);
                string folder = Path.Combine(mediaPortalDirectory, "plugins", "Windows");
                if (!Directory.Exists(folder)) return Fail("plugins\\Windows not found");
                foreach (string file in Directory.GetFiles(folder, "*.dll").OrderBy(name => name))
                {
                    Assembly assembly;
                    try { assembly = Assembly.LoadFrom(file); }
                    catch { continue; }
                    Type[] types;
                    try { types = assembly.GetExportedTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(type => type != null).ToArray(); }
                    catch { continue; }
                    foreach (Type type in types)
                    {
                        if (type == null || type.IsAbstract || type.IsInterface) continue;
                        if (!type.GetInterfaces().Any(item => String.Equals(item.FullName, SetupInterface, StringComparison.Ordinal))) continue;
                        try
                        {
                            object instance = Activator.CreateInstance(type);
                            string name = Invoke(instance, "PluginName") as string;
                            bool hasSetup = !(Invoke(instance, "HasSetup") is bool) || (bool)Invoke(instance, "HasSetup");
                            if (!hasSetup) continue;
                            if (String.IsNullOrWhiteSpace(name)) name = type.Name;
                            Console.WriteLine("plugin=" + Clean(name) + "|" + Clean(file) + "|" + Clean(type.FullName));
                        }
                        catch { }
                    }
                }
                Console.Out.Flush();
                return 0;
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        /// <summary>Lists window, external-player and process plugins with their metadata and embedded icon for the overview grid.</summary>
        public static int Overview(string mediaPortalDirectory, bool refresh = false)
        {
            try
            {
                string cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "plugin-overview.cache");
                if (!refresh && TryReadCache(cacheFile, mediaPortalDirectory)) return 0;
                Prepare(mediaPortalDirectory);
                var lines = new List<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var folder in new[]
                {
                    new { Path = Path.Combine(mediaPortalDirectory, "plugins", "Windows"), Category = "Window" },
                    new { Path = Path.Combine(mediaPortalDirectory, "plugins", "ExternalPlayers"), Category = "External" },
                    new { Path = Path.Combine(mediaPortalDirectory, "plugins", "process"), Category = "Process" }
                })
                {
                    if (!Directory.Exists(folder.Path)) continue;
                    foreach (string file in Directory.GetFiles(folder.Path, "*.dll").OrderBy(name => name))
                    {
                        Assembly assembly;
                        try { assembly = Assembly.LoadFrom(file); }
                        catch { continue; }
                        Type[] types;
                        try { types = assembly.GetExportedTypes(); }
                        catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(type => type != null).ToArray(); }
                        catch { continue; }
                        foreach (Type type in types)
                        {
                            if (type == null || type.IsAbstract || type.IsInterface) continue;
                            if (!type.GetInterfaces().Any(item => String.Equals(item.FullName, SetupInterface, StringComparison.Ordinal))) continue;
                            if (!seen.Add(type.FullName ?? type.Name)) continue;
                            try
                            {
                                object instance = Activator.CreateInstance(type);
                                string name = Invoke(instance, "PluginName") as string;
                                if (String.IsNullOrWhiteSpace(name)) name = type.Name;
                                if (name.StartsWith("GUI", StringComparison.Ordinal) && name.Length > 3) name = name.Substring(3);
                                object hasSetupValue = Invoke(instance, "HasSetup");
                                object defaultEnabledValue = Invoke(instance, "DefaultEnabled");
                                bool hasSetup = !(hasSetupValue is bool) || (bool)hasSetupValue;
                                bool defaultEnabled = defaultEnabledValue is bool && (bool)defaultEnabledValue;
                                string icon = IconBase64(assembly, name);
                                lines.Add("plugin=" + Clean(name) + "|" + Clean(file) + "|" + Clean(type.FullName) + "|" + folder.Category + "|"
                                    + (hasSetup ? "1" : "0") + "|" + (defaultEnabled ? "1" : "0") + "|" + icon);
                            }
                            catch { }
                        }
                    }
                }
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
                    lock (typeof(PluginConfigHelper))
                    {
                        using (var writer = new StreamWriter(cacheFile, false, Encoding.UTF8))
                        {
                            writer.WriteLine("#MP1-PluginOverview2|" + mediaPortalDirectory);
                            foreach (string line in lines) writer.WriteLine(line);
                        }
                    }
                }
                catch { }
                foreach (string line in lines) Console.WriteLine(line);
                Console.Out.Flush();
                return 0;
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        private static bool TryReadCache(string cacheFile, string mediaPortalDirectory)
        {
            try
            {
                if (!File.Exists(cacheFile) || File.GetLastWriteTimeUtc(cacheFile) < DateTime.UtcNow.AddHours(-12)) return false;
                string[] lines = File.ReadAllLines(cacheFile, Encoding.UTF8);
                if (lines.Length == 0 || !String.Equals(lines[0], "#MP1-PluginOverview2|" + mediaPortalDirectory, StringComparison.OrdinalIgnoreCase)) return false;
                for (int index = 1; index < lines.Length; index++)
                    if (lines[index].StartsWith("plugin=", StringComparison.OrdinalIgnoreCase)) Console.WriteLine(lines[index]);
                Console.Out.Flush();
                return true;
            }
            catch { return false; }
        }

        private static string IconBase64(Assembly assembly, string pluginName)
        {
            try
            {
                string[] resources;
                try { resources = assembly.GetManifestResourceNames(); }
                catch { return String.Empty; }
                string best = null;
                int bestScore = Int32.MinValue;
                string normalized = new string((pluginName ?? String.Empty).Where(char.IsLetterOrDigit).ToArray());
                foreach (string resource in resources)
                {
                    if (!resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                    string value = resource.ToLowerInvariant();
                    int score = 0;
                    if (value.Contains("icon")) score += 60;
                    if (value.Contains("logo")) score += 40;
                    if (value.Contains("_enabled") || value.Contains("_normal") || value.EndsWith("icon.png")) score += 100;
                    if (value.Contains("disabled") || value.Contains("faded") || value.Contains("_off")) score -= 120;
                    if (normalized.Length > 0 && value.Contains(normalized.ToLowerInvariant())) score += 30;
                    if (score > bestScore) { bestScore = score; best = resource; }
                }
                if (best == null || bestScore < 0) return String.Empty;
                using (Stream stream = assembly.GetManifestResourceStream(best))
                {
                    if (stream == null) return String.Empty;
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        return Convert.ToBase64String(memory.ToArray());
                    }
                }
            }
            catch { return String.Empty; }
        }

        public static int Show(string mediaPortalDirectory, string dll, string typeName)
        {
            try
            {
                Prepare(mediaPortalDirectory);
                Assembly assembly = Assembly.LoadFrom(dll);
                Type type = assembly.GetType(typeName, false, true);
                if (type == null) return Fail("Plugin type not found: " + typeName);
                object instance = Activator.CreateInstance(type);
                object hasSetup = Invoke(instance, "HasSetup");
                if (hasSetup is bool && !(bool)hasSetup) return Fail("This plugin has no configuration window.");
                Invoke(instance, "ShowPlugin");
                return 0;
            }
            catch (Exception ex)
            {
                try { MessageBox.Show(ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                catch { }
                return Fail(ex.Message);
            }
        }

        private static void Prepare(string mediaPortalDirectory)
        {
            if (String.IsNullOrEmpty(mediaPortalDirectory) || !Directory.Exists(mediaPortalDirectory)) throw new DirectoryNotFoundException(mediaPortalDirectory);
            Environment.CurrentDirectory = mediaPortalDirectory;
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                string name = args.Name.Split(',')[0];
                string[] candidates =
                {
                    Path.Combine(mediaPortalDirectory, name + ".dll"),
                    Path.Combine(mediaPortalDirectory, "plugins", "Windows", name + ".dll"),
                    Path.Combine(mediaPortalDirectory, "plugins", "process", name + ".dll")
                };
                foreach (string candidate in candidates)
                    if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
                return null;
            };
        }

        private static object Invoke(object instance, string method)
        {
            try
            {
                MethodInfo info = instance.GetType().GetMethod(method, Type.EmptyTypes);
                return info == null ? null : info.Invoke(instance, null);
            }
            catch { return null; }
        }

        private static string Clean(string value)
        {
            return (value ?? string.Empty).Replace("|", " ").Replace("\r", " ").Replace("\n", " ");
        }

        private static int Fail(string message)
        {
            Console.WriteLine("ok=0");
            Console.WriteLine("error=" + Clean(message));
            Console.Out.Flush();
            return 1;
        }
    }
}
