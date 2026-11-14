using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;
using MediaPortal.Profile;

namespace MP1.ConfigurationStudio.Plugin
{
    /// <summary>
    /// Reads and writes MediaPortal.xml directly. The plugin must not create or
    /// replace MediaPortal's shared settings cache: doing so can persist defaults
    /// for unrelated entries when MediaPortal shuts down.
    /// </summary>
    internal sealed class PluginSettingsStore
    {
        private readonly string path;
        private readonly XmlDocument document;

        public PluginSettingsStore()
        {
            path = MPSettings.ConfigPathName;
            if (String.IsNullOrEmpty(path) || !File.Exists(path)) throw new FileNotFoundException("MediaPortal.xml not found", path);
            document = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
            document.Load(path);
        }

        public string Get(string section, string entry, string fallback)
        {
            XmlElement node = Find(section, entry);
            return node == null ? fallback : node.InnerText;
        }

        public bool GetBool(string section, string entry, bool fallback)
        {
            string value = Get(section, entry, null);
            if (String.IsNullOrEmpty(value)) return fallback;
            return String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || value == "1";
        }

        public int GetInt(string section, string entry, int fallback)
        {
            int value;
            return Int32.TryParse(Get(section, entry, null), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
        }

        public void Set(string section, string entry, string value)
        {
            XmlElement sectionNode = FindSection(section);
            if (sectionNode == null)
            {
                sectionNode = document.CreateElement("section");
                XmlAttribute name = document.CreateAttribute("name");
                name.Value = section;
                sectionNode.Attributes.Append(name);
                document.DocumentElement.AppendChild(sectionNode);
            }
            XmlElement node = FindEntry(sectionNode, entry);
            if (node == null)
            {
                node = document.CreateElement("entry");
                XmlAttribute name = document.CreateAttribute("name");
                name.Value = entry;
                node.Attributes.Append(name);
                sectionNode.AppendChild(node);
            }
            node.InnerText = value ?? string.Empty;
        }

        public void SetBool(string section, string entry, bool value)
        {
            Set(section, entry, value ? "yes" : "no");
        }

        public string Save()
        {
            string backup = CreateBackup();
            string temporary = path + ".plugin.tmp";
            document.Save(temporary);
            try
            {
                File.Replace(temporary, path, null);
            }
            catch
            {
                File.Copy(temporary, path, true);
                File.Delete(temporary);
            }
            return backup;
        }

        private string CreateBackup()
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "backups");
            Directory.CreateDirectory(root);
            string backup = Path.Combine(root, "mp-plugin-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".zip");
            using (ZipArchive archive = ZipFile.Open(backup, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry("MediaPortal.xml", CompressionLevel.Optimal);
                using (Stream output = entry.Open())
                using (Stream input = File.OpenRead(path))
                    input.CopyTo(output);
            }
            return backup;
        }

        private XmlElement FindSection(string section)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, "section", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(element.GetAttribute("name"), section, StringComparison.OrdinalIgnoreCase)) return element;
            }
            return null;
        }

        private XmlElement Find(string section, string entry)
        {
            XmlElement sectionNode = FindSection(section);
            return sectionNode == null ? null : FindEntry(sectionNode, entry);
        }

        private static XmlElement FindEntry(XmlElement section, string entry)
        {
            foreach (XmlNode node in section.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, "entry", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(element.GetAttribute("name"), entry, StringComparison.OrdinalIgnoreCase)) return element;
            }
            return null;
        }
    }
}
