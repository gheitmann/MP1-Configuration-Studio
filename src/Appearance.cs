using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed class Appearance
    {
        public bool Dark = true;
        public bool ContextVisible = true;
        public string ThemeMode = "Dunkel";
        public int Width = 1360, Height = 900;
        public int TextScale=125;
        public readonly string PathName;
        public string LastError { get; private set; }
        public Appearance(string path) { PathName = path; Load(); }
        private void Load()
        {
            if (!File.Exists(PathName)) return;
            try
            {
                var doc = new XmlDocument { XmlResolver = null }; doc.Load(PathName);
                var root = doc.DocumentElement;
                if (root == null || root.Name != "appearance") return;
                ThemeMode = root.GetAttribute("theme") == "Hell" ? "Hell" : "Dunkel";
                Dark = ThemeMode == "Dunkel";
                ContextVisible = root.GetAttribute("context") != "false";
                int value;
                if(Int32.TryParse(root.GetAttribute("textScale"),out value))TextScale=Math.Max(100,Math.Min(175,value));
                if (Int32.TryParse(root.GetAttribute("width"), out value)) Width = Math.Max(1024, Math.Min(3840, value));
                if (Int32.TryParse(root.GetAttribute("height"), out value)) Height = Math.Max(650, Math.Min(2160, value));
            }
            catch (Exception ex) { if (!(ex is IOException) && !(ex is XmlException) && !(ex is UnauthorizedAccessException)) throw; LastError = ex.Message; }
        }
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(System.IO.Path.GetFullPath(PathName)));
                string temporary = PathName + ".tmp";
                using (var writer = XmlWriter.Create(temporary, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartElement("appearance");
                    writer.WriteAttributeString("theme", Dark ? "Dunkel" : "Hell");
                    writer.WriteAttributeString("context", ContextVisible ? "true" : "false");
                    writer.WriteAttributeString("width", Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("height", Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("textScale",TextScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }
                if (File.Exists(PathName)) File.Replace(temporary, PathName, null);
                else File.Move(temporary, PathName);
                LastError = null;
            }
            catch (Exception ex) { if (!(ex is IOException) && !(ex is UnauthorizedAccessException)) throw; LastError = ex.Message; }
        }
    }

    public sealed class Palette
    {
        public Color Background, Panel, Input, Border, Text, Muted, Accent, Selection, Hover, Primary;
        public static Palette For(bool dark)
        {
            return dark ? new Palette {
                Background = Color.FromArgb(21,26,35), Panel = Color.FromArgb(29,35,46), Input = Color.FromArgb(20,26,35),
                Border = Color.FromArgb(52,65,82), Text = Color.FromArgb(225,234,244), Muted = Color.FromArgb(149,168,189),
                Accent = Color.FromArgb(92,185,245), Selection = Color.FromArgb(32,57,77), Hover = Color.FromArgb(55,82,108), Primary = Color.FromArgb(30,113,170)
            } : new Palette {
                Background = Color.FromArgb(243,245,248), Panel = Color.White, Input = Color.FromArgb(247,249,252),
                Border = Color.FromArgb(189,201,215), Text = Color.FromArgb(32,48,68), Muted = Color.FromArgb(82,101,123),
                Accent = Color.FromArgb(8,100,159), Selection = Color.FromArgb(225,239,251), Hover = Color.FromArgb(205,225,244), Primary = Color.FromArgb(30,113,170)
            };
        }
    }
}
