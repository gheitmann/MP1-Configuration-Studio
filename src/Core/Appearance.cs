using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed class Appearance
    {
        public const int CurrentSchema = 2;

        public bool Dark = true;
        public bool ContextVisible = true;
        public bool OnboardingSeen = false;
        public string Language = "de";
        public string ThemeMode = "Dunkel";
        public int Width = 1360;
        public int Height = 900;
        public int TextScale = 125;

        public readonly string PathName;
        public string LastError { get; private set; }
        public int SchemaVersion { get; private set; }

        public Appearance(string path)
        {
            PathName = path;
            Reset();
            Load();
        }

        public static int DefaultTextScaleForDpi(int dpi)
        {
            return dpi >= 120 ? 100 : 125;
        }

        private void Reset()
        {
            Dark = true;
            ContextVisible = true;
            OnboardingSeen = false;
            Language = Localization.DefaultCode;
            ThemeMode = "Dunkel";
            Width = 1360;
            Height = 900;
            TextScale = 125;
            SchemaVersion = CurrentSchema;
        }

        private void Load()
        {
            if (!File.Exists(PathName))
            {
                return;
            }

            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.Load(PathName);
                XmlElement root = document.DocumentElement;
                if (root == null || root.Name != "appearance")
                {
                    LastError = "Unbekanntes Format der Darstellungsdatei.";
                    return;
                }

                int schema;
                SchemaVersion = Int32.TryParse(root.GetAttribute("schema"), out schema) && schema > 0 ? schema : 1;
                ThemeMode = root.GetAttribute("theme") == "Hell" ? "Hell" : "Dunkel";
                Dark = ThemeMode == "Dunkel";
                ContextVisible = root.GetAttribute("context") != "false";
                OnboardingSeen = root.GetAttribute("onboarding") == "true";
                string language = root.GetAttribute("language");
                Language = String.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ? "en"
                    : String.Equals(language, "de", StringComparison.OrdinalIgnoreCase) ? "de"
                    : Localization.DefaultCode;

                int value;
                if (Int32.TryParse(root.GetAttribute("textScale"), out value))
                {
                    TextScale = Math.Max(100, Math.Min(175, value));
                }
                if (Int32.TryParse(root.GetAttribute("width"), out value))
                {
                    Width = Math.Max(1024, Math.Min(3840, value));
                }
                if (Int32.TryParse(root.GetAttribute("height"), out value))
                {
                    Height = Math.Max(650, Math.Min(2160, value));
                }
            }
            catch (Exception ex)
            {
                if (!(ex is IOException) && !(ex is XmlException) && !(ex is UnauthorizedAccessException))
                {
                    throw;
                }
                Reset();
                LastError = ex.Message;
            }
        }

        public void Save()
        {
            if (SchemaVersion > CurrentSchema)
            {
                LastError = "Die Darstellungsdatei stammt aus einer neueren Version und wird nicht überschrieben.";
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(System.IO.Path.GetFullPath(PathName)));
                string temporary = PathName + ".tmp";
                using (var writer = XmlWriter.Create(temporary, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartElement("appearance");
                    writer.WriteAttributeString("schema", CurrentSchema.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("theme", Dark ? "Dunkel" : "Hell");
                    writer.WriteAttributeString("context", ContextVisible ? "true" : "false");
                    writer.WriteAttributeString("onboarding", OnboardingSeen ? "true" : "false");
                    writer.WriteAttributeString("language", Language);
                    writer.WriteAttributeString("width", Width.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("height", Height.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("textScale", TextScale.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }
                if (File.Exists(PathName))
                {
                    File.Replace(temporary, PathName, null);
                }
                else
                {
                    File.Move(temporary, PathName);
                }
                SchemaVersion = CurrentSchema;
                LastError = null;
            }
            catch (Exception ex)
            {
                if (!(ex is IOException) && !(ex is UnauthorizedAccessException))
                {
                    throw;
                }
                LastError = ex.Message;
            }
        }
    }

    public sealed class Palette
    {
        public Color Background, Panel, Input, Border, Text, Muted, Accent, Selection, Hover, Primary, SelectionText, PrimaryText;
        internal static bool ForceHighContrast {get;set;}

        internal static bool HighContrastActive
        {
            get { return ForceHighContrast || SystemInformation.HighContrast; }
        }

        public static Palette For(bool dark)
        {
            if (HighContrastActive)
            {
                return new Palette
                {
                    Background = SystemColors.Window,
                    Panel = SystemColors.Window,
                    Input = SystemColors.Window,
                    Border = SystemColors.WindowFrame,
                    Text = SystemColors.WindowText,
                    Muted = SystemColors.GrayText,
                    Accent = SystemColors.Highlight,
                    Selection = SystemColors.Highlight,
                    Hover = SystemColors.Highlight,
                    Primary = SystemColors.Highlight,
                    SelectionText = SystemColors.HighlightText,
                    PrimaryText = SystemColors.HighlightText
                };
            }

            return dark ? new Palette
            {
                Background = Color.FromArgb(21, 26, 35),
                Panel = Color.FromArgb(29, 35, 46),
                Input = Color.FromArgb(20, 26, 35),
                Border = Color.FromArgb(52, 65, 82),
                Text = Color.FromArgb(225, 234, 244),
                Muted = Color.FromArgb(149, 168, 189),
                Accent = Color.FromArgb(92, 185, 245),
                Selection = Color.FromArgb(32, 57, 77),
                Hover = Color.FromArgb(55, 82, 108),
                Primary = Color.FromArgb(30, 113, 170),
                SelectionText = Color.FromArgb(92, 185, 245),
                PrimaryText = Color.White
            } : new Palette
            {
                Background = Color.FromArgb(243, 245, 248),
                Panel = Color.White,
                Input = Color.FromArgb(247, 249, 252),
                Border = Color.FromArgb(189, 201, 215),
                Text = Color.FromArgb(32, 48, 68),
                Muted = Color.FromArgb(82, 101, 123),
                Accent = Color.FromArgb(8, 100, 159),
                Selection = Color.FromArgb(225, 239, 251),
                Hover = Color.FromArgb(205, 225, 244),
                Primary = Color.FromArgb(30, 113, 170),
                SelectionText = Color.FromArgb(8, 100, 159),
                PrimaryText = Color.White
            };
        }
    }
}
