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
