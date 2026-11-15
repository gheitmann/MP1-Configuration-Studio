using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class PendingChange
    {
        public string Section;
        public string Entry;
        public string OldValue;
        public string NewValue;
        public bool Removed;
    }

    internal sealed class StudioConfiguration
    {
        private readonly XmlDocument document;
        private readonly Dictionary<string, PendingChange> pending = new Dictionary<string, PendingChange>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> original = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool changed;

        public string Path { get; private set; }
        public bool IsDirty { get { return changed; } }
        public IEnumerable<PendingChange> PendingChanges
        {
            get { return pending.Values.OrderBy(change => change.Section, StringComparer.CurrentCultureIgnoreCase).ThenBy(change => change.Entry, StringComparer.CurrentCultureIgnoreCase); }
        }
        public int PendingCount { get { return pending.Count; } }

        private StudioConfiguration(string path, XmlDocument document)
        {
            Path = path;
            this.document = document;
            CaptureOriginals();
        }

        private void CaptureOriginals()
        {
            original.Clear();
            foreach (XmlNode section in document.DocumentElement.ChildNodes)
            {
                if (section.Name != "section") continue;
                string sectionName = Attribute(section, "name");
                foreach (XmlNode entry in section.ChildNodes)
                {
                    if (entry.Name != "entry") continue;
                    original[sectionName + "/" + Attribute(entry, "name")] = entry.InnerText;
                }
            }
        }

        public string OriginalValue(string section, string entry)
        {
            string value;
            return original.TryGetValue((section ?? String.Empty) + "/" + (entry ?? String.Empty), out value) ? value : null;
        }

        public static string FindConfigPath(string runtime)
        {
            string dirs = System.IO.Path.Combine(runtime, "MediaPortalDirs.xml");
            if (File.Exists(dirs))
            {
                try
                {
                    var document = new XmlDocument { XmlResolver = null };
                    document.Load(dirs);
                    XmlNode node = document.SelectSingleNode("/Config/Dir[@id='Config']");
                    if (node != null)
                    {
                        XmlNode path = node.SelectSingleNode("Path");
                        if (path != null && !String.IsNullOrWhiteSpace(path.InnerText))
                        {
                            string folder = path.InnerText.Trim();
                            if (!System.IO.Path.IsPathRooted(folder)) folder = System.IO.Path.Combine(runtime, folder);
                            string config = System.IO.Path.Combine(folder, "MediaPortal.xml");
                            if (File.Exists(config)) return System.IO.Path.GetFullPath(config);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            foreach (string candidate in new[]
            {
                System.IO.Path.Combine(runtime, "config", "MediaPortal.xml"),
                System.IO.Path.Combine(runtime, "MediaPortal.xml"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal", "MediaPortal.xml"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Team MediaPortal", "MediaPortal", "MediaPortal.xml")
            })
            {
                if (File.Exists(candidate)) return System.IO.Path.GetFullPath(candidate);
            }
            throw new FileNotFoundException("MediaPortal.xml wurde im Laufzeitordner nicht gefunden.", runtime);
        }

        public static StudioConfiguration Load(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException("MediaPortal.xml wurde nicht gefunden.", path);
            }
            var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            document.Load(path);
            if (document.DocumentElement == null || document.DocumentElement.Name != "profile")
            {
                throw new InvalidDataException("Die Datei ist kein gültiges MediaPortal-Profil.");
            }
            return new StudioConfiguration(System.IO.Path.GetFullPath(path), document);
        }

        public string Get(string section, string entry, string fallback)
        {
            XmlNode node = FindEntry(section, entry);
            return node == null ? fallback : node.InnerText;
        }

        public bool GetBool(string section, string entry, bool fallback)
        {
            string value = Get(section, entry, fallback ? "yes" : "no");
            return String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        public int GetInt(string section, string entry, int fallback)
        {
            int value;
            return Int32.TryParse(Get(section, entry, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        public void Set(string section, string entry, string value)
        {
            XmlNode node = FindEntry(section, entry);
            string previousValue = node == null ? null : node.InnerText;
            if (node == null)
            {
                XmlNode sectionNode = FindSection(section);
                if (sectionNode == null)
                {
                    sectionNode = document.CreateElement("section");
                    XmlAttribute name = document.CreateAttribute("name");
                    name.Value = section;
                    sectionNode.Attributes.Append(name);
                    document.DocumentElement.AppendChild(sectionNode);
                }
                node = document.CreateElement("entry");
                XmlAttribute entryName = document.CreateAttribute("name");
                entryName.Value = entry;
                node.Attributes.Append(entryName);
                sectionNode.AppendChild(node);
            }
            if (node.InnerText == value) return;
            node.InnerText = value;
            TrackChange(section, entry, previousValue, value, false);
        }

        private void TrackChange(string section, string entry, string previousValue, string newValue, bool removed)
        {
            string key = section + "\u0001" + entry;
            PendingChange record;
            if (!pending.TryGetValue(key, out record))
            {
                record = new PendingChange { Section = section, Entry = entry, OldValue = previousValue };
                pending[key] = record;
            }
            if (removed && record.OldValue == null)
            {
                pending.Remove(key);
                changed = pending.Count > 0;
                return;
            }
            record.NewValue = newValue;
            record.Removed = removed;
            if (!removed && String.Equals(record.OldValue, newValue, StringComparison.Ordinal))
                pending.Remove(key);
            changed = pending.Count > 0;
        }

        public System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> Entries(string section)
        {
            XmlNode sectionNode = FindSection(section);
            if (sectionNode == null) yield break;
            foreach (XmlNode node in sectionNode.ChildNodes)
                if (node.Name == "entry") yield return new System.Collections.Generic.KeyValuePair<string, string>(Attribute(node, "name"), node.InnerText);
        }

        public System.Collections.Generic.IEnumerable<string> Sections()
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
                if (node.Name == "section") yield return Attribute(node, "name");
        }

        public void Remove(string section, string entry)
        {
            XmlNode node = FindEntry(section, entry);
            if (node == null) return;
            string previousValue = node.InnerText;
            node.ParentNode.RemoveChild(node);
            TrackChange(section, entry, previousValue, null, true);
        }

        public void SetBool(string section, string entry, bool value)
        {
            Set(section, entry, value ? "yes" : "no");
        }

        public void SetInt(string section, string entry, int value)
        {
            Set(section, entry, value.ToString(CultureInfo.InvariantCulture));
        }

        public string Save(string backupRoot = null)
        {
            if (!changed) return null;
            string root = String.IsNullOrWhiteSpace(backupRoot)
                ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "backups")
                : backupRoot;
            Directory.CreateDirectory(root);
            string backup = System.IO.Path.Combine(root, "native-save-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".zip");
            ConfigurationBackup.Create(Path, backup);

            string temporary = Path + ".studio-native.tmp";
            document.Save(temporary);
            try
            {
                if (File.Exists(Path)) File.Replace(temporary, Path, null);
                else File.Move(temporary, Path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
            changed = false;
            pending.Clear();
            CaptureOriginals();
            StudioLog.Info("Native configuration saved: " + Path + "; backup=" + backup);
            return backup;
        }

        private XmlNode FindSection(string name)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
                if (node.Name == "section" && String.Equals(Attribute(node, "name"), name, StringComparison.OrdinalIgnoreCase)) return node;
            return null;
        }

        private XmlNode FindEntry(string section, string entry)
        {
            XmlNode sectionNode = FindSection(section);
            if (sectionNode == null) return null;
            foreach (XmlNode node in sectionNode.ChildNodes)
                if (node.Name == "entry" && String.Equals(Attribute(node, "name"), entry, StringComparison.OrdinalIgnoreCase)) return node;
            return null;
        }

        private static string Attribute(XmlNode node, string name)
        {
            XmlAttribute attribute = node.Attributes == null ? null : node.Attributes[name];
            return attribute == null ? String.Empty : attribute.Value;
        }
    }
}
