using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal enum CompatibilityTier
    {
        A,
        B,
        C
    }

    internal sealed class SectionPolicy
    {
        public string TypeName { get; private set; }
        public CompatibilityTier Tier { get; private set; }
        public bool Zoom { get; private set; }
        public bool Theme { get; private set; }
        public string ThemeMode { get; private set; }
        public bool Search { get; private set; }

        public SectionPolicy(string typeName, CompatibilityTier tier, bool zoom, string themeMode, bool search)
        {
            TypeName = typeName;
            Tier = tier;
            Zoom = zoom;
            ThemeMode = String.IsNullOrWhiteSpace(themeMode) ? "base" : themeMode;
            Theme = !String.Equals(ThemeMode, "native", StringComparison.OrdinalIgnoreCase);
            Search = search;
        }
    }

    internal static class CompatibilityPolicy
    {
        private const string ResourceName = "MP1.ConfigurationStudio.compatibility.xml";
        private static readonly Dictionary<string, SectionPolicy> Sections = new Dictionary<string, SectionPolicy>(StringComparer.OrdinalIgnoreCase);
        private static readonly SectionPolicy Unknown = new SectionPolicy("", CompatibilityTier.B, true, "base", true);

        public static int Count
        {
            get { return Sections.Count; }
        }

        public static void Initialize(string overridePath = null)
        {
            Sections.Clear();
            LoadEmbedded();
            if (!String.IsNullOrEmpty(overridePath) && File.Exists(overridePath))
            {
                LoadFile(overridePath);
            }
            StudioLog.Info("Compatibility policy loaded: " + Sections.Count + " sections");
        }

        public static SectionPolicy Resolve(string typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName))
            {
                return Unknown;
            }

            SectionPolicy policy;
            return Sections.TryGetValue(typeName, out policy) ? policy : Unknown;
        }

        public static IEnumerable<SectionPolicy> All()
        {
            return Sections.Values.OrderBy(policy => policy.TypeName, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static void LoadEmbedded()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(ResourceName))
                {
                    if (stream == null)
                    {
                        StudioLog.Warn("Compatibility resource missing: " + ResourceName);
                        return;
                    }
                    using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                    {
                        Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Compatibility policy embedded load failed");
            }
        }

        private static void LoadFile(string path)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                {
                    Load(reader);
                }
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Compatibility policy override load failed");
            }
        }

        private static void Load(XmlReader reader)
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(reader);
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "compatibility")
            {
                throw new XmlException("Das Compatibility-Dokument hat kein compatibility-Stammelement.");
            }

            XmlNodeList nodes = root.SelectNodes("section");
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                var element = node as XmlElement;
                if (element == null)
                {
                    continue;
                }

                string typeName = element.GetAttribute("type");
                if (String.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                CompatibilityTier tier = ParseTier(element.GetAttribute("tier"));
                bool zoom = ParseBool(element.GetAttribute("zoom"), true);
                bool search = ParseBool(element.GetAttribute("search"), true);
                string themeMode = element.GetAttribute("theme");
                Sections[typeName] = new SectionPolicy(typeName, tier, zoom, themeMode, search);
            }
        }

        private static CompatibilityTier ParseTier(string value)
        {
            if (String.Equals(value, "A", StringComparison.OrdinalIgnoreCase))
            {
                return CompatibilityTier.A;
            }
            if (String.Equals(value, "C", StringComparison.OrdinalIgnoreCase))
            {
                return CompatibilityTier.C;
            }
            return CompatibilityTier.B;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            bool result;
            return Boolean.TryParse(value, out result) ? result : fallback;
        }
    }
}
