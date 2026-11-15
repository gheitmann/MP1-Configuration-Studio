using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Classic-style icon overview for window, external and process plugins.</summary>
    internal sealed class NativePluginOverviewPanel : UserControl
    {
        private sealed class PluginEntry
        {
            public string Name;
            public string Dll;
            public string Type;
            public string Category;
            public bool HasSetup;
            public bool DefaultEnabled;
            public Image Icon;
            public bool Enabled;
            public bool Home;
            public bool Plugins;
            public StudioPluginTile Tile;
        }

        private readonly StudioConfiguration configuration;
        private readonly string runtime;
        private readonly float scale;
        private readonly Action classic;
        private readonly Action changed;
        private readonly Palette palette = Palette.For(true);
        private readonly FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Panel grid = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Palette.For(true).Background };
        private readonly Label status = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        private readonly List<PluginEntry> entries = new List<PluginEntry>();
        private readonly List<Label> headers = new List<Label>();
        private readonly Button configure, toggle, home, myPlugins, browse;
        private PluginEntry selected;
        private bool loading;

        public NativePluginOverviewPanel(StudioConfiguration configuration, string runtime, float scale, Action classic, Action changed)
        {
            this.configuration = configuration;
            this.runtime = runtime;
            this.scale = scale;
            this.classic = classic;
            this.changed = changed;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = palette.Background, Margin = new Padding(0), Padding = new Padding(0) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(30)));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(44)));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            status.Text = Localization.IsEnglish ? "Reading plugins …" : "Plugins werden gelesen …";
            status.Margin = new Padding(0, 0, 0, Px(4));
            root.Controls.Add(status, 0, 0);

            grid.Controls.Add(flow);
            grid.Resize += delegate { SizeHeaders(); };
            root.Controls.Add(grid, 0, 2);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Margin = new Padding(0) };
            configure = ActionButton(Localization.IsEnglish ? "Configure" : "Konfigurieren", delegate { ConfigureSelected(); }, body);
            toggle = ActionButton(Localization.IsEnglish ? "Enable / disable" : "Aktivieren / deaktivieren", delegate { ToggleEnabled(); }, body);
            home = ActionButton(Localization.IsEnglish ? "In Home" : "Im Home-Menü", delegate { ToggleHome(); }, body);
            myPlugins = ActionButton(Localization.IsEnglish ? "In Plugins" : "In My Plugins", delegate { TogglePlugins(); }, body);
            browse = ActionButton(Localization.IsEnglish ? "Browse and install plugins…" : "Plugins suchen und installieren…", delegate { Open("https://www.team-mediaportal.com/extensions"); }, body);
            actions.Controls.Add(configure);
            actions.Controls.Add(toggle);
            actions.Controls.Add(home);
            actions.Controls.Add(myPlugins);
            actions.Controls.Add(ActionButton(Localization.IsEnglish ? "Refresh" : "Aktualisieren", delegate { LoadPlugins(true); }, body));
            actions.Controls.Add(browse);
            actions.Controls.Add(ActionButton(Localization.IsEnglish ? "Classic configuration" : "Klassische Konfiguration", delegate { if (classic != null) classic(); }, body));
            root.Controls.Add(actions, 0, 1);

            Controls.Add(root);
            UpdateActions();
            LoadPlugins();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Button ActionButton(string text, EventHandler click, Font font)
        {
            var button = new StudioButton
            {
                Text = text,
                Font = font,
                Height = Px(30),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(Px(8), 0, Px(8), 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = palette.Panel,
                ForeColor = palette.Text,
                Margin = new Padding(0, 0, Px(6), Px(4))
            };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LoadPlugins(bool refresh = false)
        {
            if (loading) return;
            loading = true;
            selected = null;
            foreach (PluginEntry entry in entries) if (entry.Icon != null) entry.Icon.Dispose();
            entries.Clear();
            flow.Controls.Clear();
            headers.Clear();
            status.Text = Localization.IsEnglish ? "Reading plugins …" : "Plugins werden gelesen …";
            status.ForeColor = palette.Muted;
            UpdateActions();
            ThreadPool.QueueUserWorkItem(delegate
            {
                List<PluginEntry> found = ReadPlugins(refresh);
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        BeginInvoke(new Action(delegate { Populate(found); }));
                }
                catch { loading = false; }
            });
        }

        private List<PluginEntry> ReadPlugins(bool refresh)
        {
            var result = new List<PluginEntry>();
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-overview \"" + runtime + "\"" + (refresh ? " --refresh" : String.Empty),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = runtime
                };
                using (Process process = Process.Start(info))
                {
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(60000))
                    {
                        try { process.Kill(); } catch { }
                        return result;
                    }
                    string output = outputTask.Result;
                    errorTask.Result.ToString();
                    foreach (string line in (output ?? String.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!line.StartsWith("plugin=", StringComparison.OrdinalIgnoreCase)) continue;
                        string[] parts = line.Substring(7).Split('|');
                        if (parts.Length < 7) continue;
                        string name = parts[0].Trim();
                        string dll = parts[1].Trim();
                        string type = parts[2].Trim();
                        if (name.Length == 0 || dll.Length == 0 || type.Length == 0) continue;
                        var entry = new PluginEntry
                        {
                            Name = name,
                            Dll = dll,
                            Type = type,
                            Category = parts[3].Trim(),
                            HasSetup = parts[4] == "1",
                            DefaultEnabled = parts[5] == "1",
                            Icon = Decode(parts[6])
                        };
                        entry.Enabled = configuration.GetBool("plugins", entry.Name, entry.DefaultEnabled);
                        entry.Home = configuration.GetBool("home", entry.Name, false);
                        entry.Plugins = configuration.GetBool("myplugins", entry.Name, false);
                        if (entry.Category == "Window" && entry.Enabled && !entry.Home && !entry.Plugins) entry.Home = true;
                        result.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(delegate { status.Text = ex.Message; status.ForeColor = palette.Muted; }));
                }
                catch { }
            }
            return result.OrderBy(entry => entry.Category == "Window" ? 0 : entry.Category == "External" ? 1 : 2)
                .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static Image Decode(string base64)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(base64)) return null;
                using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
                using (var image = Image.FromStream(stream)) return new Bitmap(image);
            }
            catch { return null; }
        }

        private void Populate(List<PluginEntry> found)
        {
            loading = false;
            using (var suspended = new SuspendLayoutScope(flow))
            {
                entries.Clear();
                entries.AddRange(found);
                flow.Controls.Clear();
                headers.Clear();
                string category = null;
                foreach (PluginEntry entry in entries)
                {
                    if (!String.Equals(category, entry.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        category = entry.Category;
                        var header = new Label
                        {
                            Text = CategoryTitle(category),
                            Font = Typography.Create(10f * scale, FontStyle.Bold),
                            ForeColor = palette.Text,
                            BackColor = palette.Background,
                            AutoSize = false,
                            Height = Px(28),
                            Margin = new Padding(0, Px(8), 0, Px(4)),
                            Padding = new Padding(Px(4), Px(6), 0, 0)
                        };
                        flow.Controls.Add(header);
                        headers.Add(header);
                        flow.SetFlowBreak(header, true);
                    }
                    var tile = new StudioPluginTile(scale)
                    {
                        Entry = entry,
                        Width = Px(120),
                        Height = Px(122),
                        Margin = new Padding(Px(4)),
                        Colors = () => palette,
                        AccessibleName = entry.Name
                    };
                    tile.Click += delegate { Select(entry); };
                    tile.DoubleClick += delegate { Select(entry); ConfigureSelected(); };
                    entry.Tile = tile;
                    flow.Controls.Add(tile);
                }
                SizeHeaders();
            }
            status.Text = entries.Count == 0
                ? (Localization.IsEnglish ? "No plugins found." : "Keine Plugins gefunden.")
                : entries.Count + (Localization.IsEnglish ? " plugins · " : " Plugins · ") + entries.Count(entry => entry.Enabled) + (Localization.IsEnglish ? " enabled" : " aktiviert");
            status.ForeColor = entries.Count == 0 ? palette.Muted : palette.Text;
            UpdateActions();
        }

        private void SizeHeaders()
        {
            int width = Math.Max(Px(240), grid.ClientSize.Width - Px(24));
            foreach (Label header in headers) header.Width = width;
            flow.Width = Math.Max(Px(240), grid.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(4));
            flow.SuspendLayout();
            flow.ResumeLayout(true);
        }

        private string CategoryTitle(string category)
        {
            switch ((category ?? String.Empty).ToLowerInvariant())
            {
                case "window": return Localization.IsEnglish ? "Window plugins" : "Fenster-Plugins";
                case "external": return Localization.IsEnglish ? "External players" : "Externe Player";
                case "process": return Localization.IsEnglish ? "Process plugins" : "Prozess-Plugins";
                default: return Localization.IsEnglish ? "Other plugins" : "Weitere Plugins";
            }
        }

        private void Select(PluginEntry entry)
        {
            selected = entry;
            foreach (PluginEntry item in entries)
                if (item.Tile != null) item.Tile.Selected = Object.ReferenceEquals(item, entry);
            UpdateActions();
        }

        private void UpdateActions()
        {
            bool has = selected != null;
            configure.Enabled = has && selected.HasSetup;
            toggle.Enabled = has;
            home.Enabled = has && selected.Category == "Window";
            myPlugins.Enabled = has && selected.Category == "Window";
            if (!has)
            {
                toggle.Text = Localization.IsEnglish ? "Enable / disable" : "Aktivieren / deaktivieren";
                return;
            }
            toggle.Text = selected.Enabled
                ? (Localization.IsEnglish ? "Disable" : "Deaktivieren")
                : (Localization.IsEnglish ? "Enable" : "Aktivieren");
            home.Text = selected.Home ? (Localization.IsEnglish ? "Remove from Home" : "Aus Home entfernen") : (Localization.IsEnglish ? "In Home" : "Im Home-Menü");
            myPlugins.Text = selected.Plugins ? (Localization.IsEnglish ? "Remove from Plugins" : "Aus My Plugins entfernen") : (Localization.IsEnglish ? "In Plugins" : "In My Plugins");
            status.Text = selected.Name + "  ·  " + CategoryTitle(selected.Category) + (selected.HasSetup ? "  ·  " + (Localization.IsEnglish ? "configurable" : "konfigurierbar") : String.Empty);
            status.ForeColor = palette.Text;
        }

        private void ToggleEnabled()
        {
            if (selected == null) return;
            selected.Enabled = !selected.Enabled;
            WriteState(selected);
        }

        private void ToggleHome()
        {
            if (selected == null || selected.Category != "Window") return;
            selected.Home = !selected.Home;
            if (selected.Home) selected.Plugins = false;
            if (!selected.Home && !selected.Plugins) selected.Plugins = true;
            WriteState(selected);
        }

        private void TogglePlugins()
        {
            if (selected == null || selected.Category != "Window") return;
            selected.Plugins = !selected.Plugins;
            if (selected.Plugins) selected.Home = false;
            if (!selected.Home && !selected.Plugins) selected.Home = true;
            WriteState(selected);
        }

        private void WriteState(PluginEntry entry)
        {
            configuration.SetBool("plugins", entry.Name, entry.Enabled);
            configuration.SetBool("pluginsdlls", Path.GetFileName(entry.Dll), entry.Enabled);
            if (entry.Category == "Window")
            {
                configuration.SetBool("pluginswindows", entry.Type, entry.Enabled);
                configuration.SetBool("home", entry.Name, entry.Home);
                configuration.SetBool("myplugins", entry.Name, entry.Plugins);
            }
            if (changed != null) changed();
            if (entry.Tile != null) entry.Tile.Invalidate();
            UpdateActions();
        }

        private void ConfigureSelected()
        {
            if (selected == null || !selected.HasSetup) return;
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-config \"" + runtime + "\" \"" + selected.Dll + "\" \"" + selected.Type + "\"",
                    UseShellExecute = true,
                    WorkingDirectory = runtime
                };
                Process.Start(info);
                status.Text = (Localization.IsEnglish ? "Opened: " : "Geöffnet: ") + selected.Name;
                status.ForeColor = palette.Text;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                status.ForeColor = palette.Muted;
            }
        }

        private static void Open(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (PluginEntry entry in entries) if (entry.Icon != null) entry.Icon.Dispose();
                entries.Clear();
            }
            base.Dispose(disposing);
        }

        private sealed class SuspendLayoutScope : IDisposable
        {
            private readonly Control control;
            public SuspendLayoutScope(Control control) { this.control = control; control.SuspendLayout(); }
            public void Dispose() { control.ResumeLayout(true); }
        }
    }

    internal sealed class StudioPluginTile : Control
    {
        private readonly float scale;
        public object Entry;
        public bool Selected;
        public Func<Palette> Colors;

        public StudioPluginTile(float scale)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            object entry = Entry;
            string name = Get(entry, "Name");
            bool enabled = GetBool(entry, "Enabled");
            string category = Get(entry, "Category");
            Image icon = GetImage(entry);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), S(7)))
            {
                using (var brush = new SolidBrush(Selected ? palette.Selection : palette.Panel)) e.Graphics.FillPath(brush, path);
                using (var pen = new Pen(Selected ? palette.Accent : palette.Border)) e.Graphics.DrawPath(pen, path);
            }
            int iconSize = S(52);
            var iconBounds = new Rectangle((Width - iconSize) / 2, S(12), iconSize, iconSize);
            if (icon != null)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(icon, iconBounds);
            }
            else
            {
                StudioIcons.Draw(e.Graphics, GlyphFor(name, category), iconBounds, palette.Muted);
            }
            using (var brush = new SolidBrush(enabled ? Color.FromArgb(76, 175, 80) : palette.Muted))
                e.Graphics.FillEllipse(brush, Width - S(18), S(8), S(8), S(8));
            Color ink = enabled ? palette.Text : palette.Muted;
            var textBounds = new Rectangle(S(7), S(68), Width - S(14), Height - S(74));
            TextRenderer.DrawText(e.Graphics, name, Font, textBounds, ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static string GlyphFor(string name, string category)
        {
            string value = (name ?? String.Empty).ToLowerInvariant();
            if (value.Contains("home")) return "home";
            if (value.Contains("music") || value.Contains("lyrics") || value.Contains("lastfm") || value.Contains("scrobbl")) return "music";
            if (value.Contains("picture")) return "picture";
            if (value.Contains("video") || value.Contains("movie") || value.Contains("film") || value.Contains("series") || value.Contains("mvcentral")) return "video";
            if (value.Contains("radio")) return "radio";
            if (value.Contains("weather")) return "weather";
            if (value.Contains("rss") || value.Contains("news")) return "rss";
            if (value.Contains("sudoku") || value.Contains("tetris") || value.Contains("game")) return "game";
            if (value.Contains("setting") || value.Contains("skin")) return "settings";
            if (value.Contains("search")) return "search";
            if (String.Equals(category, "Process", StringComparison.OrdinalIgnoreCase)) return "system";
            if (String.Equals(category, "External", StringComparison.OrdinalIgnoreCase)) return "play";
            if (value.Contains("tv")) return "tv";
            return "plugin";
        }

        protected override void OnMouseEnter(EventArgs e) { Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { Invalidate(); base.OnMouseLeave(e); }

        private static string Get(object entry, string property)
        {
            var info = entry == null ? null : entry.GetType().GetField(property);
            return info == null ? String.Empty : Convert.ToString(info.GetValue(entry), CultureInfo.InvariantCulture);
        }

        private static bool GetBool(object entry, string property)
        {
            var info = entry == null ? null : entry.GetType().GetField(property);
            return info != null && info.GetValue(entry) is bool && (bool)info.GetValue(entry);
        }

        private static Image GetImage(object entry)
        {
            var info = entry == null ? null : entry.GetType().GetField("Icon");
            return info == null ? null : info.GetValue(entry) as Image;
        }
    }
}
