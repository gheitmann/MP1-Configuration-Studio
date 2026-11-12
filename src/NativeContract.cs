using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeContractReport
    {
        public readonly List<string> Missing = new List<string>();
        public int SectionCount;

        public bool IsValid
        {
            get { return Missing.Count == 0 && SectionCount > 0; }
        }

        public string Summary()
        {
            if (IsValid)
            {
                return "Native contract valid; " + SectionCount + " sections.";
            }

            return "Native contract invalid; missing: " + String.Join(", ", Missing.ToArray()) + "; sections: " + SectionCount + ".";
        }
    }

    internal static class NativeContract
    {
        internal const string SettingsFormTypeName = "MediaPortal.Configuration.SettingsForm";

        internal const string SectionTreeField = "sectionTree";
        internal const string HolderPanelField = "holderPanel";
        internal const string OkButtonField = "okButton";
        internal const string CancelButtonField = "cancelButton";
        internal const string ApplyButtonField = "applyButton";
        internal const string ToolStripField = "toolStrip1";
        internal const string DonateLinkField = "linkLabel1";

        internal static object FindField(object target, string name)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field.GetValue(target);
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static T Require<T>(object target, string name) where T : class
        {
            T value = FindField(target, name) as T;
            if (value == null)
            {
                throw new NotSupportedException("Die native Oberfläche enthält nicht das erwartete Feld: " + name);
            }
            return value;
        }

        internal static List<Control> ReadSections(Form form)
        {
            string error;
            List<Control> sections = TryReadSections(form, out error);
            if (sections == null)
            {
                throw new NotSupportedException(error ?? "Die nativen Konfigurationsseiten konnten nicht ermittelt werden.");
            }
            return sections;
        }

        internal static List<Control> TryReadSections(Form form, out string error)
        {
            error = null;
            if (form == null)
            {
                error = "Kein natives Formular.";
                return null;
            }

            PropertyInfo property = form.GetType().GetProperty("SettingSections", BindingFlags.Static | BindingFlags.Public);
            if (property == null)
            {
                error = "Die native Eigenschaft SettingSections fehlt.";
                return null;
            }

            IDictionary values = property.GetValue(null, null) as IDictionary;
            if (values == null)
            {
                error = "SettingSections ist keine Dictionary-Instanz.";
                return null;
            }

            var pages = new List<Control>();
            foreach (DictionaryEntry entry in values)
            {
                if (entry.Value == null)
                {
                    continue;
                }

                PropertyInfo config = entry.Value.GetType().GetProperty("ConfigSection");
                if (config == null)
                {
                    error = "Ein Eintrag in SettingSections hat keine ConfigSection-Eigenschaft.";
                    return null;
                }

                Control page = config.GetValue(entry.Value, null) as Control;
                if (page != null)
                {
                    pages.Add(page);
                }
            }

            if (pages.Count == 0)
            {
                error = "Es wurden keine Originalseiten gefunden.";
                return null;
            }

            return pages;
        }

        internal static Control NodeSection(TreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            PropertyInfo property = node.GetType().GetProperty("Section");
            if (property != null)
            {
                return property.GetValue(node, null) as Control;
            }
            return node.Tag as Control;
        }

        internal static NativeContractReport Validate(Form form)
        {
            var report = new NativeContractReport();
            CheckField<TreeView>(form, SectionTreeField, report);
            CheckField<Panel>(form, HolderPanelField, report);
            CheckField<Button>(form, OkButtonField, report);
            CheckField<Button>(form, CancelButtonField, report);
            CheckField<ToolStrip>(form, ToolStripField, report);

            string error;
            List<Control> sections = TryReadSections(form, out error);
            if (sections == null)
            {
                report.Missing.Add("SettingSections/ConfigSection: " + error);
            }
            else
            {
                report.SectionCount = sections.Count;
            }

            return report;
        }

        private static void CheckField<T>(Form form, string name, NativeContractReport report) where T : class
        {
            if (form == null)
            {
                report.Missing.Add("Formular");
                return;
            }

            if (!(FindField(form, name) is T))
            {
                report.Missing.Add(name + " (" + typeof(T).Name + ")");
            }
        }

        internal static string ConfigFolder(string kind)
        {
            Type config = Assembly.Load("Utils").GetType("MediaPortal.Configuration.Config", true);
            Type dir = config.GetNestedType("Dir");
            if (dir == null)
            {
                throw new NotSupportedException("Der native Typ Config.Dir fehlt.");
            }
            object folder = config.GetMethod("GetFolder").Invoke(null, new object[] { Enum.Parse(dir, kind) });
            return Convert.ToString(folder);
        }

        internal static string DebugSettingsPath()
        {
            Type settings = Assembly.Load("Utils").GetType("MediaPortal.Configuration.DebugSettings", true);
            return Convert.ToString(settings.GetMethod("SettingPath").Invoke(null, new object[] { "DoNotAllowSlowMotionDuringZapping" }));
        }

        internal static void SetDebugSettingsPath(string path)
        {
            Type settings = Assembly.Load("Utils").GetType("MediaPortal.Configuration.DebugSettings", true);
            FieldInfo field = settings.GetField("SettingsPath", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new NotSupportedException("Das native Feld DebugSettings.SettingsPath fehlt.");
            }
            field.SetValue(null, path);
        }

        internal static string MPSettingsValue(string section, string key, string defaultValue)
        {
            Type settingsType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("MediaPortal.Profile.MPSettings", false))
                .FirstOrDefault(type => type != null);
            if (settingsType == null)
            {
                return defaultValue;
            }

            object settings = Activator.CreateInstance(settingsType);
            try
            {
                MethodInfo method = settingsType.GetMethod("GetValueAsString", new[] { typeof(string), typeof(string), typeof(string) });
                if (method == null)
                {
                    return defaultValue;
                }
                return Convert.ToString(method.Invoke(settings, new object[] { section, key, defaultValue }));
            }
            finally
            {
                IDisposable disposable = settings as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
