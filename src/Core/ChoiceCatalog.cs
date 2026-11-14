using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class ChoiceOption
    {
        public string Value;
        public string Label;

        public ChoiceOption(string value, string label)
        {
            Value = value;
            Label = label;
        }
    }

    /// <summary>
    /// Central catalog of values that the classic MediaPortal Configuration exposes
    /// as fixed choices or discoverable lists instead of free text boxes.
    /// Used by the desktop Studio and the MediaPortal plugin.
    /// </summary>
    internal static class ChoiceCatalog
    {
        private static string configDirectory;
        private static string runtimeDirectory;

        public static void Configure(string configDir, string runtimeDir)
        {
            configDirectory = configDir;
            runtimeDirectory = runtimeDir;
        }

        public static bool Has(string section, string entry)
        {
            return Resolve(section, entry).Count > 0;
        }

        public static List<ChoiceOption> Resolve(string section, string entry)
        {
            string key = ((section ?? string.Empty) + "/" + (entry ?? string.Empty)).ToLowerInvariant();
            switch (key)
            {
                case "bdplayer/regioncode":
                    return Text(new[] { "A", "B", "C" });
                case "bdplayer/audiotype":
                    return Text(new[] { "AC3", "AC3+", "DTS", "DTS-HD", "DTS-HD Master", "LPCM", "TrueHD" });
                case "bdplayer/audiolanguage":
                case "bdplayer/subtitlelanguage":
                case "dvdplayer/audiolanguage":
                case "dvdplayer/subtitlelanguage":
                    return CultureNames();
                case "movieplayer/audiolanguage":
                case "subtitles/language":
                    return CultureCodes();
                case "dvdplayer/armode":
                    return Text(new[] { "Follow stream", "Crop", "Letterbox", "Stretch" });
                case "dvdplayer/displaymode":
                    return Text(new[] { "Default", "16:9", "4:3 Pan Scan", "4:3 Letterbox" });
                case "subtitles/engine":
                    return Text(new[] { "MPC-HC", "DirectVobSub", "FFDShow", "XySubFilter", "Disabled" });
                case "subtitles/fontface":
                    return Fonts();
                case "mytv/deinterlace":
                    return Indexed(new[] { "None", "Bob", "Weave", "Best" });
                case "mytv/showepisodeinfo":
                    return Indexed(new[] { "[None]", "Number", "Title", "Number + Title" });
                case "musicfiles/selectoption":
                    return new List<ChoiceOption> { new ChoiceOption("play", "Play"), new ChoiceOption("queue", "Queue") };
                case "musicfiles/dateadded":
                    return Indexed(new[] { "Current Date", "Creation Date", "Last Write Date" });
                case "audioplayer/playerid":
                    return Indexed(new[] { "BASS engine", "ASIO", "WASAPI", "Internal dshow player" });
                case "audioplayer/wasapispeakers":
                    return Indexed(new[] { "Mono (1 Speaker)", "Stereo (2 Speakers)", "Quadrophonic (4 Speakers)", "5.1 (6 Speakers)", "7.1 (8 Speakers)" });
                case "audioplayer/resumeselect":
                    return Text(new[] { "Genre", "Title", "Filename", "Album", "Artist", "Albumartist", "Composer", "Conductor" });
                case "audioplayer/upmixmono":
                    return Indexed(new[] { "None", "Stereo", "QuadraphonicPhonic", "5.1 Surround", "7.1 Surround" });
                case "audioplayer/upmixstereo":
                    return Indexed(new[] { "None", "QuadraphonicPhonic", "5.1 Surround", "7.1 Surround" });
                case "audioplayer/upmixquadro":
                    return Indexed(new[] { "None", "5.1 Surround", "7.1 Surround" });
                case "audioplayer/upmixfivedotone":
                    return Indexed(new[] { "None", "7.1 Surround" });
                case "moviedatabase/actorslistsize":
                    return Text(new[] { "Short", "Long" });
                case "general/dx9filteringmode":
                    return Text(new[] { "None", "Point Filtering", "Bilinear Filtering", "Anisotropic Filtering", "Pyrimidal Quad Filtering", "Gaussian Quad Filtering" });
                case "screenselector/screennumber":
                    return Screens();
                case "remote/hidprofile":
                    return Profiles();
                case "skin/name":
                    return Skins();
                case "gui/language":
                    return Languages();
            }
            return new List<ChoiceOption>();
        }

        private static List<ChoiceOption> Text(IEnumerable<string> values)
        {
            return values.Select(value => new ChoiceOption(value, value)).ToList();
        }

        private static List<ChoiceOption> Indexed(IEnumerable<string> labels)
        {
            var list = new List<ChoiceOption>();
            int index = 0;
            foreach (string label in labels) list.Add(new ChoiceOption(index++.ToString(CultureInfo.InvariantCulture), label));
            return list;
        }

        private static string ConfigDir
        {
            get
            {
                if (!String.IsNullOrEmpty(configDirectory)) return configDirectory;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal");
            }
        }

        private static string RuntimeDir
        {
            get { return String.IsNullOrEmpty(runtimeDirectory) ? AppDomain.CurrentDomain.BaseDirectory : runtimeDirectory; }
        }

        public static List<ChoiceOption> Skins()
        {
            var list = new List<ChoiceOption>();
            try
            {
                string root = Path.Combine(ConfigDir, "skin");
                if (Directory.Exists(root))
                {
                    foreach (string directory in Directory.GetDirectories(root))
                    {
                        string name = Path.GetFileName(directory);
                        if (String.IsNullOrEmpty(name) || name.StartsWith(".", StringComparison.Ordinal)) continue;
                        if (!File.Exists(Path.Combine(directory, "references.xml")) && !Directory.Exists(Path.Combine(directory, "Media"))) continue;
                        list.Add(new ChoiceOption(name, name));
                    }
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static List<ChoiceOption> Languages()
        {
            var list = new List<ChoiceOption>();
            try
            {
                string root = Path.Combine(ConfigDir, "language");
                if (Directory.Exists(root))
                {
                    foreach (string file in Directory.GetFiles(root, "strings_*.xml"))
                    {
                        try
                        {
                            var document = new System.Xml.XmlDocument { XmlResolver = null };
                            document.Load(file);
                            string name = document.DocumentElement == null ? null : document.DocumentElement.GetAttribute("name");
                            string code = Path.GetFileNameWithoutExtension(file);
                            if (code.StartsWith("strings_", StringComparison.OrdinalIgnoreCase)) code = code.Substring("strings_".Length);
                            if (String.IsNullOrEmpty(name)) name = code;
                            list.Add(new ChoiceOption(name, name + " (" + code + ")"));
                        }
                        catch { }
                    }
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static List<ChoiceOption> Fonts()
        {
            var list = new List<ChoiceOption>();
            try
            {
                using (var fonts = new InstalledFontCollection())
                {
                    foreach (FontFamily family in fonts.Families) list.Add(new ChoiceOption(family.Name, family.Name));
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static List<ChoiceOption> Profiles()
        {
            var list = new List<ChoiceOption>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string root in new[]
            {
                Path.Combine(RuntimeDir, "defaults", "InputDeviceMappings"),
                Path.Combine(RuntimeDir, "InputDeviceMappings"),
                Path.Combine(ConfigDir, "InputDeviceMappings")
            })
            {
                try
                {
                    if (!Directory.Exists(root)) continue;
                    foreach (string file in Directory.GetFiles(root, "*.xml"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        if (String.IsNullOrEmpty(name) || !seen.Add(name)) continue;
                        list.Add(new ChoiceOption(name, name));
                    }
                }
                catch { }
            }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static List<ChoiceOption> Screens()
        {
            var list = new List<ChoiceOption>();
            try
            {
                Screen[] screens = Screen.AllScreens;
                for (int i = 0; i < screens.Length; i++)
                {
                    var bounds = screens[i].Bounds;
                    list.Add(new ChoiceOption(i.ToString(CultureInfo.InvariantCulture), String.Format(CultureInfo.InvariantCulture, "{0} ({1}x{2}){3}", i + 1, bounds.Width, bounds.Height, screens[i].Primary ? " *" : "")));
                }
            }
            catch { }
            return list;
        }

        public static List<ChoiceOption> CultureNames()
        {
            var list = new List<ChoiceOption>();
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if (culture.Equals(CultureInfo.InvariantCulture)) continue;
                list.Add(new ChoiceOption(culture.EnglishName, culture.EnglishName));
            }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        public static List<ChoiceOption> CultureCodes()
        {
            var list = new List<ChoiceOption>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                string code = culture.TwoLetterISOLanguageName;
                if (String.IsNullOrEmpty(code) || code == "iv" || !seen.Add(code)) continue;
                list.Add(new ChoiceOption(code, code + " – " + culture.EnglishName));
            }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }
    }
}
