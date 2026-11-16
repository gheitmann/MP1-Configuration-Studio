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
            public bool CanEnable;
            public string Description;
            public string Author;
            public bool LoadedEnabled;
            public bool LoadedHome;
            public bool LoadedPlugins;
            public StudioPluginTile Tile;
        }

        private readonly StudioConfiguration configuration;
        private readonly string runtime;
        private readonly float scale;
        private readonly Action classic;
        private readonly Action changed;
        private readonly Action<int> heightChanged;
        private readonly FlowLayoutPanel actions;
        private StudioTextInput filter;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, string lParam);

        private static void SetCue(TextBox box, string text)
        {
            if (box == null || box.IsDisposed) return;
            try { SendMessage(box.Handle, 0x1501, new IntPtr(1), text); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }
        private readonly Palette palette = Palette.For(true);
        private readonly FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Panel grid = new Panel { Dock = DockStyle.Fill, AutoScroll = false, BackColor = Palette.For(true).Background };
        private readonly Label status = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        private readonly List<PluginEntry> entries = new List<PluginEntry>();
        private readonly List<Label> headers = new List<Label>();
        private readonly ToolTip tips = new ToolTip();
        private ContextMenuStrip contextMenu;
        private readonly Button configure, toggle, home, myPlugins, browse;
        private PluginEntry selected;
        private bool loading;
        private string loadError;
        private int lastHeight;

        public NativePluginOverviewPanel(StudioConfiguration configuration, string runtime, float scale, Action classic, Action changed, Action<int> heightChanged)
        {
            this.configuration = configuration;
            this.runtime = runtime;
            this.scale = scale;
            this.classic = classic;
            this.changed = changed;
            this.heightChanged = heightChanged;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = palette.Background, Margin = new Padding(0), Padding = new Padding(0) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(30)));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(38)));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            status.Text = Localization.IsEnglish ? "Reading plugins …" : "Plugins werden gelesen …";
            status.Margin = new Padding(0, 0, 0, Px(4));
            root.Controls.Add(status, 0, 0);

            grid.Controls.Add(flow);
            grid.Resize += delegate { SizeHeaders(); };
            root.Controls.Add(grid, 0, 3);

            filter = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, AccessibleName = Localization.IsEnglish ? "Filter plugins" : "Plugins filtern" };
            filter.Box.TextChanged += delegate { ApplyFilter(); };
            filter.Box.HandleCreated += delegate { SetCue(filter.Box, Localization.IsEnglish ? "Filter plugins…" : "Plugins filtern…"); };
            root.Controls.Add(filter, 0, 2);

            actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Margin = new Padding(0) };
            actions.SizeChanged += delegate { UpdateHeight(); };
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
            loadError = null;
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
                    Arguments = "--plugin-overview " + PluginConfigHelper.QuoteArgument(runtime) + (refresh ? " --refresh" : String.Empty),
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
                    if (!process.WaitForExit(180000))
                    {
                        try { process.Kill(); } catch { }
                        loadError = Localization.IsEnglish
                            ? "The plugin scan timed out. Click Refresh to try again."
                            : "Die Plugin-Erfassung hat zu lange gedauert. Klicke auf Aktualisieren.";
                        StudioLog.Warn("Plugin overview timed out for " + runtime);
                        return result;
                    }
                    string output = outputTask.Result;
                    string error = errorTask.Result;
                    StudioLog.Info("Plugin overview: exit=" + process.ExitCode + " lines=" + ((output ?? String.Empty).Split('\n').Count(line => line.StartsWith("plugin=", StringComparison.OrdinalIgnoreCase))) + " runtime=" + runtime);
                    if (process.ExitCode != 0)
                    {
                        loadError = (Localization.IsEnglish ? "Plugin scan failed: " : "Plugin-Erfassung fehlgeschlagen: ") + FirstLine(error);
                        return result;
                    }
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
                            CanEnable = parts.Length <= 6 || parts[6] == "1",
                            Description = parts.Length > 7 ? parts[7] : String.Empty,
                            Author = parts.Length > 8 ? parts[8] : String.Empty,
                            Icon = Decode(parts.Length > 9 ? parts[9] : String.Empty)
                        };
                        entry.Enabled = configuration.GetBool("plugins", entry.Name, entry.DefaultEnabled);
                        entry.Home = configuration.GetBool("home", entry.Name, false);
                        entry.Plugins = configuration.GetBool("myplugins", entry.Name, false);
                        if (entry.Category == "Window" && entry.Enabled && !entry.Home && !entry.Plugins) entry.Home = true;
                        entry.LoadedEnabled = entry.Enabled;
                        entry.LoadedHome = entry.Home;
                        entry.LoadedPlugins = entry.Plugins;
                        result.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                loadError = ex.Message;
                StudioLog.Warn("Plugin overview failed for " + runtime + ": " + ex.Message);
            }
            return result.OrderBy(entry => entry.Category == "Window" ? 0 : entry.Category == "External" ? 1 : 2)
                .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static string FirstLine(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            string line = value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return String.IsNullOrWhiteSpace(line) ? String.Empty : line.Trim();
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
                        header.Tag = category;
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
                    tile.MouseDown += delegate(object sender, MouseEventArgs e)
                    {
                        if (e.Button != MouseButtons.Right) return;
                        Select(entry);
                        ShowContextMenu(entry);
                    };
                    tips.SetToolTip(tile, BuildTileTip(entry));
                    entry.Tile = tile;
                    flow.Controls.Add(tile);
                }
                SizeHeaders();
                ApplyFilter();
            }
            if (entries.Count == 0)
            {
                status.Text = String.IsNullOrWhiteSpace(loadError)
                    ? (Localization.IsEnglish ? "No plugins found in " : "Keine Plugins gefunden in ") + Path.Combine(runtime, "plugins")
                    : loadError;
            }
            else
            {
                status.Text = entries.Count + (Localization.IsEnglish ? " plugins · " : " Plugins · ") + entries.Count(entry => entry.Enabled)
                    + (Localization.IsEnglish ? " enabled · right-click a tile for quick actions" : " aktiviert · Rechtsklick auf ein Plugin für Schnellaktionen");
            }
            status.ForeColor = entries.Count == 0 ? palette.Muted : palette.Text;
            UpdateActions();
        }

        private string BuildTileTip(PluginEntry entry)
        {
            string tip = (entry.Description ?? String.Empty).Trim();
            if (tip.Length == 0)
                return Localization.IsEnglish ? "Right-click for quick actions" : "Rechtsklick für Schnellaktionen";
            string author = (entry.Author ?? String.Empty).Trim();
            if (author.Length > 0) tip += "\r\n" + (Localization.IsEnglish ? "Author: " : "Autor: ") + author;
            tip += "\r\n" + (Localization.IsEnglish ? "Right-click for quick actions" : "Rechtsklick für Schnellaktionen");
            return tip;
        }

        private void ApplyFilter()
        {
            if (flow == null || entries.Count == 0) return;
            string query = filter == null ? String.Empty : filter.Text.Trim();
            foreach (PluginEntry entry in entries)
            {
                if (entry.Tile == null) continue;
                bool visible = query.Length == 0
                    || entry.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (entry.Description ?? String.Empty).IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || entry.Category.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
                entry.Tile.Visible = visible;
            }
            foreach (Label header in headers)
            {
                string category = Convert.ToString(header.Tag, CultureInfo.InvariantCulture);
                header.Visible = entries.Any(entry => String.Equals(entry.Category, category, StringComparison.OrdinalIgnoreCase)
                    && entry.Tile != null && entry.Tile.Visible);
            }
        }

        private void SizeHeaders()
        {
            int width = Math.Max(Px(240), grid.ClientSize.Width - Px(24));
            foreach (Label header in headers) header.Width = width;
            flow.Width = Math.Max(Px(240), grid.ClientSize.Width - Px(4));
            flow.SuspendLayout();
            flow.ResumeLayout(true);
            UpdateHeight();
        }

        private void UpdateHeight()
        {
            if (heightChanged == null) return;
            int actionHeight = actions == null ? Px(44) : Math.Max(Px(40), actions.PreferredSize.Height);
            int height = Px(30) + actionHeight + Math.Max(Px(120), flow.PreferredSize.Height) + Px(10);
            if (Math.Abs(height - lastHeight) < Px(4)) return;
            lastHeight = height;
            heightChanged(height);
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

        private void ShowContextMenu(PluginEntry entry)
        {
            if (contextMenu == null)
            {
                contextMenu = new ContextMenuStrip();
            }
            ToolStripItem[] oldItems = contextMenu.Items.Cast<ToolStripItem>().ToArray();
            contextMenu.Items.Clear();
            foreach (ToolStripItem old in oldItems) old.Dispose();
            contextMenu.BackColor = palette.Panel;
            contextMenu.ForeColor = palette.Text;
            contextMenu.ShowImageMargin = false;
            contextMenu.ShowCheckMargin = true;
            contextMenu.Font = Typography.Create(9.5f * scale, FontStyle.Regular);
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new StudioMenuColorTable(palette));
            contextMenu.Items.Add(MenuItem(Localization.IsEnglish ? "Configure" : "Konfigurieren", entry.HasSetup, null, delegate { Select(entry); ConfigureSelected(); }));
            contextMenu.Items.Add(MenuItem(entry.Enabled
                ? (Localization.IsEnglish ? "Disable" : "Deaktivieren")
                : (Localization.IsEnglish ? "Enable" : "Aktivieren"), entry.CanEnable, null, delegate { Select(entry); ToggleEnabled(); }));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(MenuItem(Localization.IsEnglish ? "In Home" : "Im Home-Menü", entry.Category == "Window", entry.Category == "Window" ? (bool?)entry.Home : null, delegate { Select(entry); ToggleHome(); }));
            contextMenu.Items.Add(MenuItem(Localization.IsEnglish ? "In Plugins" : "In My Plugins", entry.Category == "Window", entry.Category == "Window" ? (bool?)entry.Plugins : null, delegate { Select(entry); TogglePlugins(); }));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(MenuItem(Localization.IsEnglish ? "Browse and install plugins…" : "Plugins suchen und installieren…", true, null, delegate { Open("https://www.team-mediaportal.com/extensions"); }));
            contextMenu.Items.Add(MenuItem(Localization.IsEnglish ? "Classic configuration" : "Klassische Konfiguration", true, null, delegate { if (classic != null) classic(); }));
            contextMenu.Show(Cursor.Position);
        }

        private ToolStripMenuItem MenuItem(string text, bool enabled, bool? check, EventHandler click)
        {
            var item = new ToolStripMenuItem(text)
            {
                Enabled = enabled,
                ForeColor = palette.Text,
                BackColor = palette.Panel,
                Checked = check.HasValue && check.Value,
                CheckOnClick = false,
                Padding = new Padding(Px(4), Px(3), Px(18), Px(3))
            };
            item.Click += click;
            return item;
        }

        private void Select(PluginEntry entry)
        {
            PluginEntry previous = selected;
            selected = entry;
            if (previous != null && previous.Tile != null)
            {
                previous.Tile.Selected = false;
                previous.Tile.Invalidate();
            }
            if (entry != null && entry.Tile != null)
            {
                entry.Tile.Selected = true;
                entry.Tile.Invalidate();
            }
            UpdateActions();
        }

        private void UpdateActions()
        {
            bool has = selected != null;
            if (actions != null) actions.SuspendLayout();
            try
            {
                configure.Enabled = has && selected.HasSetup;
                toggle.Enabled = has && selected.CanEnable;
                home.Enabled = has && selected.Category == "Window";
                myPlugins.Enabled = has && selected.Category == "Window";
                if (!has)
                {
                    SetText(toggle, Localization.IsEnglish ? "Enable / disable" : "Aktivieren / deaktivieren");
                    return;
                }
                SetText(toggle, selected.Enabled
                    ? (Localization.IsEnglish ? "Disable" : "Deaktivieren")
                    : (Localization.IsEnglish ? "Enable" : "Aktivieren"));
                SetText(home, selected.Home ? (Localization.IsEnglish ? "Remove from Home" : "Aus Home entfernen") : (Localization.IsEnglish ? "In Home" : "Im Home-Menü"));
                SetText(myPlugins, selected.Plugins ? (Localization.IsEnglish ? "Remove from Plugins" : "Aus My Plugins entfernen") : (Localization.IsEnglish ? "In Plugins" : "In My Plugins"));
                status.Text = selected.Name + "  ·  " + CategoryTitle(selected.Category) + (selected.HasSetup ? "  ·  " + (Localization.IsEnglish ? "configurable" : "konfigurierbar") : String.Empty)
                    + (selected.CanEnable ? String.Empty : "  ·  " + (Localization.IsEnglish ? "cannot be disabled" : "nicht deaktivierbar"));
                status.ForeColor = palette.Text;
            }
            finally
            {
                if (actions != null) actions.ResumeLayout(true);
            }
        }

        private static void SetText(Control control, string text)
        {
            if (control != null && !String.Equals(control.Text, text, StringComparison.Ordinal)) control.Text = text;
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
            // Writes only deltas against the original MediaPortal.xml state so enabling a plugin back
            // does not leave unexpected pending entries for keys that were absent originally.
            SyncBool("plugins", entry.Name, entry.Enabled, entry.DefaultEnabled);
            SyncBool("pluginsdlls", Path.GetFileName(entry.Dll), entry.Enabled, entry.DefaultEnabled);
            if (entry.Category == "Window")
            {
                SyncBool("pluginswindows", entry.Type, entry.Enabled, true);
                SyncBool("home", entry.Name, entry.Home, entry.LoadedHome);
                SyncBool("myplugins", entry.Name, entry.Plugins, entry.LoadedPlugins);
            }
            if (changed != null) changed();
            if (entry.Tile != null) entry.Tile.Invalidate();
            UpdateActions();
        }

        private void SyncBool(string section, string key, bool desired, bool defaultWhenAbsent)
        {
            string original = configuration.OriginalValue(section, key);
            bool originalEnabled = original == null ? defaultWhenAbsent : IsYes(original);
            if (desired == originalEnabled)
            {
                if (original == null) configuration.Remove(section, key); // no-op, or clears a pending entry we added earlier
                else configuration.Set(section, key, original);           // no-op, or reverts an earlier pending change
                return;
            }
            configuration.SetBool(section, key, desired);
        }

        private static bool IsYes(string value)
        {
            return String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private void ConfigureSelected()
        {
            if (selected == null || !selected.HasSetup) return;
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-config " + PluginConfigHelper.QuoteArgument(runtime) + " " + PluginConfigHelper.QuoteArgument(selected.Dll) + " " + PluginConfigHelper.QuoteArgument(selected.Type),
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
                tips.Dispose();
                if (contextMenu != null) contextMenu.Dispose();
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

    internal sealed class StudioMenuColorTable : ProfessionalColorTable
    {
        private readonly Palette palette;
        public StudioMenuColorTable(Palette palette) { this.palette = palette; }
        public override Color ToolStripDropDownBackground { get { return palette.Panel; } }
        public override Color ImageMarginGradientBegin { get { return palette.Panel; } }
        public override Color ImageMarginGradientMiddle { get { return palette.Panel; } }
        public override Color ImageMarginGradientEnd { get { return palette.Panel; } }
        public override Color MenuBorder { get { return palette.Border; } }
        public override Color MenuItemBorder { get { return palette.Accent; } }
        public override Color MenuItemSelected { get { return palette.Selection; } }
        public override Color MenuItemSelectedGradientBegin { get { return palette.Selection; } }
        public override Color MenuItemSelectedGradientEnd { get { return palette.Selection; } }
        public override Color MenuItemPressedGradientBegin { get { return palette.Selection; } }
        public override Color MenuItemPressedGradientEnd { get { return palette.Selection; } }
        public override Color SeparatorDark { get { return palette.Border; } }
        public override Color SeparatorLight { get { return palette.Panel; } }
        public override Color CheckBackground { get { return palette.Selection; } }
        public override Color CheckSelectedBackground { get { return palette.Accent; } }
        public override Color CheckPressedBackground { get { return palette.Accent; } }
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
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            TabStop = false;
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
