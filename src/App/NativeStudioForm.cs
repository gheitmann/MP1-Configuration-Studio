using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeStudioForm : Form, IMessageFilter
    {
        private readonly string runtime;
        private readonly Appearance appearance;
        private readonly StudioConfiguration configuration;
        private readonly Palette palette;
        private readonly List<NativeSection> sections = NativeSettingsSchema.Sections();
        private readonly Panel scroll = new StudioBufferedPanel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new StudioBufferedTable { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        private readonly GlassFlowPanel navigation = new GlassFlowPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        private readonly List<Label> navigationHeaders = new List<Label>();
        private GlassPanel header, footer, sidebar, glassBody, toolbar;
        private TableLayoutPanel topArea, toolbarRow;
        private StudioScrollHost contentScroll, sidebarScroll;
        private StudioAction backButton, forwardButton, helpButton, toolsButton;
        private StudioChoice modeChoice, themeChoice;
        private readonly List<NativeSection> history = new List<NativeSection>();
        private int historyIndex = -1;
        private bool historyNavigation;
        private System.Windows.Forms.Timer statusTimer;
        private Color statusRestoreColor;
        private string statusRestoreText;
        public bool RestartNativeRequested { get; private set; }
        public string RestartSection { get; private set; }
        private StudioLogo logo;
        private FlowLayoutPanel footerActions;
        private Button saveButton, closeButton, backupButton, updateButton;
        private readonly Label title = new Label { AutoSize = true };
        private readonly Label subtitle = new Label { AutoSize = true };
        private readonly Label status = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        private readonly Label pageTitle = new Label { AutoSize = true };
        private readonly Label pageLead = new Label { AutoSize = true };
        private StudioChoice sizeChoice;
        private StudioTextInput searchBox;
        private StudioTabStrip tabStrip;
        private int tabRowIndex = -1;
        private int contentRowIndex = -1;
        private readonly Dictionary<string, int> tabSelection = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private NativeSection current;
        private float scale;
        private Font body, heading, small, smallUnderline;
        private bool dirty;
        private bool suppressClosePrompt;
        public bool StartStudioRequested { get; private set; }
        public string PreferencesPath { get { return appearance.PathName; } }

        public NativeStudioForm(string runtime, string preferences) : this(runtime, preferences, null) { }

        public NativeStudioForm(string runtime, string preferences, string initialSection)
        {
            this.runtime = runtime;
            Text = "MP1 Configuration Studio · Neue Oberfläche";
            Icon = StudioIcon.Value;
            FormBorderStyle = FormBorderStyle.Sizable;
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1180, 780);
            MinimumSize = new Size(940, 600);
            BackColor = Color.FromArgb(21, 26, 35);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            appearance = new Appearance(preferences ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "appearance.xml"));
            appearance.NativeOriginal = true;
            Localization.Set(appearance.Language);
            Palette.UseLight = String.Equals(appearance.ThemeMode, "Hell", StringComparison.OrdinalIgnoreCase);
            Text = Localization.T("MP1 Configuration Studio · Neue Oberfläche");
            scale = appearance.TextScale / 100f;
            body = Typography.Create(10f * scale, FontStyle.Regular);
            heading = Typography.Create(18f * scale, FontStyle.Regular);
            small = Typography.Create(9f * scale, FontStyle.Regular);
            smallUnderline = new Font(small, small.Style | FontStyle.Underline);
            Font = body;
            palette = Palette.For(true);
            BackColor = palette.Background;
            ForeColor = palette.Text;
            string configPath = StudioConfiguration.FindConfigPath(runtime);
            configuration = StudioConfiguration.Load(configPath);
            NativeFilterCatalog.Configure(runtime);
            ChoiceCatalog.Configure(Path.GetDirectoryName(configuration.Path), runtime);
            BuildChrome();
            ShowSection(FindSection(initialSection) ?? sections[0]);
            ApplySearch();
            Shown += delegate { scroll.AutoScrollPosition = Point.Empty; };
            FormClosing += OnClosing;
            Application.AddMessageFilter(this);
        }

        private NativeSection FindSection(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return null;
            foreach (NativeSection section in sections)
            {
                if (String.Equals(section.Id, id, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(section.TitleDe, id, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(section.TitleEn, id, StringComparison.OrdinalIgnoreCase))
                    return section;
            }
            return null;
        }

        private void BuildChrome()
        {
            header = new GlassPanel { Dock = DockStyle.Fill, Height = Px(82), BackColor = palette.Background };
            logo = new StudioLogo { Colors = () => palette, Bounds = new Rectangle(Px(22), Px(20), Px(40), Px(40)), Font = Typography.Create(13f * scale, FontStyle.Bold) };
            header.Controls.Add(logo);
            title.Text = "MP1 / CONFIGURATION";
            title.Font = Typography.Create(16f * Math.Min(scale, 1.25f), FontStyle.Regular);
            title.ForeColor = palette.Text;
            title.BackColor = Color.Transparent;
            title.Location = new Point(Px(76), Px(14));
            header.Controls.Add(title);
            subtitle.Text = Localization.T("Neue Oberfläche · MediaPortal 1.39");
            subtitle.Font = Typography.Create(9f * Math.Min(scale, 1.4f), FontStyle.Regular);
            subtitle.ForeColor = palette.Muted;
            subtitle.BackColor = Color.Transparent;
            subtitle.Location = new Point(Px(78), Px(44));
            header.Controls.Add(subtitle);
            sizeChoice = new StudioChoice(scale) { Font = body, Colors = () => palette, Height = Px(30), Dock = DockStyle.Fill, AccessibleName = Localization.T("Schriftgröße") };
            sizeChoice.SetOptions(new[]
            {
                new ChoiceOption("100", "100 %"),
                new ChoiceOption("115", "115 %"),
                new ChoiceOption("125", "125 %"),
                new ChoiceOption("150", "150 %"),
                new ChoiceOption("175", "175 %")
            }, appearance.TextScale.ToString());
            sizeChoice.SelectedIndexChanged += delegate { ChangeScale(); };

            backButton = new StudioAction { Colors = () => palette, IconName = "back", Navigation = false, Secondary = true, Font = body, Height = Px(30), Width = Px(34), Margin = new Padding(0, Px(1), Px(4), Px(1)), BackColor = palette.Panel, AccessibleName = Localization.T("Zurück") };
            backButton.Click += delegate { NavigateHistory(-1); };
            forwardButton = new StudioAction { Colors = () => palette, IconName = "forward", Navigation = false, Secondary = true, Font = body, Height = Px(30), Width = Px(34), Margin = new Padding(0, Px(1), Px(8), Px(1)), BackColor = palette.Panel, AccessibleName = Localization.T("Vorwärts") };
            forwardButton.Click += delegate { NavigateHistory(1); };

            searchBox = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, AccessibleName = Localization.T("Einstellungen durchsuchen") };
            searchBox.ConfigureIcon("search", "Ctrl+F");
            searchBox.Box.TextChanged += delegate { ApplySearch(); };
            searchBox.Box.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) { searchBox.Clear(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter)
                {
                    StudioAction first = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Visible && item.Tag is NativeSection);
                    if (first != null) ShowSection((NativeSection)first.Tag, first);
                    e.SuppressKeyPress = true;
                }
            };

            modeChoice = new StudioChoice(scale) { Font = body, Colors = () => palette, Dock = DockStyle.Fill, AccessibleName = Localization.T("Ansichtsmodus") };
            modeChoice.SetOptions(new[] { new ChoiceOption("standard", "Standard"), new ChoiceOption("expert", "Expert") }, appearance.ExpertMode ? "expert" : "standard");
            modeChoice.SelectedIndexChanged += delegate
            {
                appearance.ExpertMode = String.Equals(modeChoice.SelectedValue, "expert", StringComparison.OrdinalIgnoreCase);
                appearance.Save();
                ApplySearch();
                if (!appearance.ExpertMode && current != null && IsAdvancedSection(current))
                {
                    StudioAction first = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Visible && item.Tag is NativeSection);
                    if (first != null) ShowSection((NativeSection)first.Tag, first);
                }
            };

            themeChoice = new StudioChoice(scale) { Font = body, Colors = () => palette, Dock = DockStyle.Fill, AccessibleName = Localization.T("Farbschema") };
            themeChoice.SetOptions(new[]
            {
                new ChoiceOption("Dunkel", Localization.T("Dunkel")),
                new ChoiceOption("Hell", Localization.T("Hell"))
            }, appearance.ThemeMode);
            themeChoice.SelectedIndexChanged += delegate { ChangeTheme(themeChoice.SelectedValue); };

            helpButton = new StudioAction { Colors = () => palette, IconName = "keyboard", Navigation = false, Secondary = true, Font = body, Height = Px(30), Width = Px(34), Margin = new Padding(Px(4), Px(1), Px(4), Px(1)), BackColor = palette.Panel, AccessibleName = Localization.T("Tastenkürzel") };
            helpButton.Click += delegate { using (var dialog = new KeyboardHelpDialog()) dialog.ShowDialog(this); };
            toolsButton = new StudioAction { Colors = () => palette, IconName = "system", Text = Localization.T("Werkzeuge"), Navigation = false, Font = body, Height = Px(30), Width = Px(124), Margin = new Padding(0, Px(1), 0, Px(1)), BackColor = palette.Panel, AccessibleName = Localization.T("Werkzeuge") };
            toolsButton.Click += delegate { ShowToolsMenu(); };

            toolbar = new GlassPanel { Dock = DockStyle.Fill, BackColor = palette.Background, Padding = new Padding(Px(24), Px(6), Px(20), Px(6)) };
            toolbarRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 9, RowCount = 1, BackColor = palette.Background, Margin = Padding.Empty, Padding = Padding.Empty };
            toolbarRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            int[] toolbarBaseWidths = { 34, 34, 8, 0, 150, 130, 120, 34, 124 };
            for (int index = 0; index < toolbarBaseWidths.Length; index++)
                toolbarRow.ColumnStyles.Add(new ColumnStyle(toolbarBaseWidths[index] == 0 ? SizeType.Percent : SizeType.Absolute, toolbarBaseWidths[index] == 0 ? 100 : ToolbarWidth(toolbarBaseWidths[index])));
            toolbarRow.Controls.Add(backButton, 0, 0);
            toolbarRow.Controls.Add(forwardButton, 1, 0);
            toolbarRow.Controls.Add(searchBox, 2, 0);
            toolbarRow.SetColumnSpan(searchBox, 2);
            toolbarRow.Controls.Add(modeChoice, 4, 0);
            toolbarRow.Controls.Add(sizeChoice, 5, 0);
            toolbarRow.Controls.Add(themeChoice, 6, 0);
            toolbarRow.Controls.Add(helpButton, 7, 0);
            toolbarRow.Controls.Add(toolsButton, 8, 0);
            toolbar.Controls.Add(toolbarRow);

            glassBody = new GlassPanel { Dock = DockStyle.Fill, BackColor = palette.Background };
            sidebar = new GlassPanel { Dock = DockStyle.Left, Width = Px(230), BackColor = palette.Panel, Padding = new Padding(Px(10), Px(14), Px(8), Px(10)) };
            navigation.BackColor = palette.Panel;
            navigation.Padding = new Padding(0);
            sidebar.Controls.Add(navigation);
            var groupCounts = sections.GroupBy(section => NavigationGroup(section)).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
            string previousGroup = null;
            foreach (NativeSection section in sections)
            {
                string group = NavigationGroup(section);
                bool header = groupCounts[group] > 1 && !String.Equals(group, previousGroup, StringComparison.OrdinalIgnoreCase);
                if (header)
                {
                    var caption = new Label
                    {
                        Text = NavigationGroupTitle(group).ToUpperInvariant(),
                        Font = Typography.Create(8.5f * Math.Min(scale, 1.3f), FontStyle.Bold),
                        ForeColor = palette.Muted,
                        BackColor = palette.Panel,
                        AutoSize = false,
                        Height = Px(26),
                        Width = Px(198),
                        Padding = new Padding(Px(8), Px(8), 0, 0),
                        Margin = new Padding(0, previousGroup == null ? 0 : Px(10), 0, Px(2)),
                        Tag = group
                    };
                    navigation.Controls.Add(caption);
                    navigationHeaders.Add(caption);
                }
                var item = new StudioAction { Text = section.NavTitle, IconName = section.Icon, Colors = () => palette, Navigation = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(header ? Px(12) : 0, 2, 0, 2), BackColor = palette.Panel, AccessibleName = section.TitleDe };
                item.Click += delegate { ShowSection(section, item); };
                item.Tag = section;
                string navTip = section.Subtitle;
                if (!String.IsNullOrEmpty(navTip)) FieldTips().SetToolTip(item, navTip);
                navigation.Controls.Add(item);
                previousGroup = group;
            }
            var classic = new StudioAction { Text = Localization.T("Klassisch / Plugins"), IconName = "plugin", Colors = () => palette, Navigation = true, Secondary = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(0, 18, 0, 2), BackColor = palette.Panel };
            classic.Click += delegate { StartClassic(); };
            navigation.Controls.Add(classic);
            var legacy = new StudioAction { Text = Localization.T("Studio 1.9-Oberfläche"), IconName = "surface", Colors = () => palette, Navigation = true, Secondary = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(0, 4, 0, 2), BackColor = palette.Panel };
            legacy.Click += delegate { StartStudioShell(); };
            navigation.Controls.Add(legacy);
            navigation.MouseLeave += delegate { ClearNavigationHover(); };

            scroll.BackColor = palette.Background;
            scroll.Padding = new Padding(Px(28), Px(18), Px(28), Px(18));
            table.BackColor = palette.Background;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(300)));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.Padding = new Padding(0);
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            contentScroll = new StudioScrollHost(scroll, delegate { return new Size(scroll.Padding.Horizontal + table.Width, scroll.Padding.Vertical + table.Height); }, () => palette, false, (Color?)palette.Background);
            sidebarScroll = new StudioScrollHost(navigation, delegate
            {
                int offset = -navigation.AutoScrollPosition.Y;
                int bottom = navigation.Padding.Top;
                foreach (Control item in navigation.Controls)
                {
                    if (!item.Visible) continue;
                    bottom = Math.Max(bottom, item.Top + offset + item.Height + item.Margin.Bottom);
                }
                bottom += navigation.Padding.Bottom;
                return new Size(navigation.ClientSize.Width, Math.Max(navigation.ClientSize.Height, bottom));
            }, () => palette, false, (Color?)palette.Panel);
            sidebar.Controls.Add(sidebarScroll);
            glassBody.Controls.Add(contentScroll);
            glassBody.Controls.Add(sidebar);
            Controls.Add(glassBody);

            footer = new GlassPanel { Dock = DockStyle.Bottom, Height = Px(62), BackColor = palette.Background, Padding = new Padding(Px(24), Px(12), Px(20), Px(10)) };
            footer.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (var pen = new Pen(palette.Border)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };
            status.Text = Localization.T("Bereit");
            status.Font = small;
            status.ForeColor = palette.Muted;
            status.BackColor = Color.Transparent;
            status.Cursor = Cursors.Hand;
            status.Click += delegate { ShowPendingChanges(); };
            status.MouseEnter += delegate
            {
                if (!dirty) return;
                status.Font = smallUnderline;
                status.ForeColor = palette.Primary;
            };
            status.MouseLeave += delegate { if (dirty) RestoreStatus(); };
            footer.Controls.Add(status);
            footerActions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = Px(520) };
            closeButton = new GlassButton { Colors = () => palette, Text = Localization.T("Schließen"), Width = Px(110), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            closeButton.FlatAppearance.BorderColor = palette.Border;
            closeButton.Click += delegate { Close(); };
            footerActions.Controls.Add(closeButton);
            saveButton = new GlassButton { Primary = true, Colors = () => palette, Text = Localization.T("Speichern"), Width = Px(120), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Primary, ForeColor = palette.PrimaryText, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += delegate { SaveConfiguration(); };
            footerActions.Controls.Add(saveButton);
            backupButton = new GlassButton { Colors = () => palette, Text = Localization.T("Sicherung"), Width = Px(110), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            backupButton.FlatAppearance.BorderColor = palette.Border;
            backupButton.Click += delegate { CreateBackup(); };
            footerActions.Controls.Add(backupButton);
            updateButton = new GlassButton { Colors = () => palette, Text = "Update", Width = Px(84), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            updateButton.FlatAppearance.BorderColor = palette.Border;
            updateButton.Click += delegate { CheckForUpdates(); };
            footerActions.Controls.Add(updateButton);
            footer.Controls.Add(footerActions);
            topArea = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, BackColor = palette.Background, Margin = Padding.Empty, Padding = Padding.Empty };
            topArea.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(82)));
            topArea.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(50)));
            topArea.Controls.Add(header, 0, 0);
            topArea.Controls.Add(toolbar, 0, 1);
            Controls.Add(footer);
            Controls.Add(topArea);
            ApplyMetrics();
            ApplyChromeStyle();
        }

        private void ApplyMetrics()
        {
            header.Height = Px(82);
            logo.Bounds = new Rectangle(Px(22), Px(20), Px(40), Px(40));
            logo.Font = Typography.Create(13f * scale, FontStyle.Bold);
            title.Font = Typography.Create(16f * Math.Min(scale, 1.25f), FontStyle.Regular);
            title.Location = new Point(Px(76), Px(14));
            subtitle.Font = Typography.Create(9f * Math.Min(scale, 1.4f), FontStyle.Regular);
            subtitle.Location = new Point(Px(78), Px(44));
            if (topArea != null && topArea.RowStyles.Count >= 2)
            {
                topArea.RowStyles[0].Height = Px(82);
                topArea.RowStyles[1].Height = Px(50);
            }
            if (toolbarRow != null && toolbarRow.ColumnStyles.Count >= 9)
            {
                int[] widths = { 34, 34, 8, 0, 150, 130, 120, 34, 124 };
                for (int index = 0; index < widths.Length && index < toolbarRow.ColumnStyles.Count; index++)
                    if (widths[index] > 0) toolbarRow.ColumnStyles[index].Width = ToolbarWidth(widths[index]);
            }
            if (toolbar != null) toolbar.Padding = new Padding(Px(24), Px(6), Px(20), Px(6));
            if (topArea != null) topArea.Height = Px(82) + Px(50);
            foreach (StudioAction button in new[] { backButton, forwardButton, helpButton })
            {
                if (button == null) continue;
                button.Font = body;
                button.Height = Px(30);
                button.Width = Px(34);
                button.Margin = new Padding(0, Px(1), Px(4), Px(1));
            }
            if (toolsButton != null)
            {
                toolsButton.Font = body;
                toolsButton.Height = Px(30);
                toolsButton.Width = Px(124);
                toolsButton.Margin = new Padding(0, Px(1), 0, Px(1));
            }
            foreach (StudioChoice choice in new[] { modeChoice, sizeChoice, themeChoice })
            {
                if (choice == null) continue;
                choice.Font = body;
                choice.Height = Px(30);
                choice.Margin = new Padding(0, Px(1), Px(6), Px(1));
            }
            if (searchBox != null)
            {
                searchBox.Font = body;
                searchBox.Margin = new Padding(0, Px(1), Px(8), Px(1));
            }
            sidebar.Width = Px(230);
            sidebar.Padding = new Padding(Px(10), Px(14), Px(8), Px(10));
            int navigationWidth = Math.Max(Px(120), sidebar.ClientSize.Width - sidebar.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
            int secondaryIndex = 0;
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>())
            {
                bool secondary = item.Secondary;
                item.Font = body;
                item.Height = Px(38);
                item.Width = navigationWidth;
                int topMargin = secondary ? (secondaryIndex++ == 0 ? Px(18) : Px(4)) : Px(2);
                item.Margin = new Padding(0, topMargin, 0, Px(2));
            }
            footer.Height = Px(62);
            footer.Padding = new Padding(Px(24), Px(12), Px(20), Px(10));
            footerActions.Width = Px(520);
            status.Font = small;
            closeButton.Font = body;
            closeButton.Width = Px(110);
            closeButton.Height = Px(34);
            closeButton.Margin = new Padding(Px(8), 0, 0, 0);
            saveButton.Font = body;
            saveButton.Width = Px(120);
            saveButton.Height = Px(34);
            backupButton.Font = body;
            backupButton.Width = Px(110);
            backupButton.Height = Px(34);
            backupButton.Margin = new Padding(Px(8), 0, 0, 0);
            scroll.Padding = new Padding(Px(28), Px(18), Px(28), Px(18));
            table.ColumnStyles[0].Width = Px(300);
            LayoutTable();
        }

        private void LayoutTable()
        {
            int available = contentScroll != null ? contentScroll.Width - SystemInformation.VerticalScrollBarWidth : scroll.ClientSize.Width;
            table.Width = Math.Max(Px(300), available - scroll.Padding.Horizontal - Px(4));
            float labelWidth = Math.Max(Px(220), Math.Min(Px(320), table.Width * 0.4f));
            if (table.ColumnStyles.Count > 0) table.ColumnStyles[0].Width = (int)Math.Round(labelWidth);
            table.PerformLayout();
            if (tabStrip != null && tabStrip.Parent == table)
            {
                tabStrip.Width = table.Width;
                int height = tabStrip.Reflow();
                if (tabRowIndex >= 0 && tabRowIndex < table.RowStyles.Count) table.RowStyles[tabRowIndex].Height = Math.Max(Px(36), height);
            }
            if (scroll.HorizontalScroll.Visible) scroll.AutoScrollPosition = Point.Empty;
        }

        private sealed class SectionTab
        {
            public string TitleDe;
            public string TitleEn;
            public NativeGroup Group;
            public List<KeyValuePair<string, List<KeyValuePair<string, string>>>> Remaining;
            public string Title { get { return Group != null ? Group.Title : Localization.T("Weitere Einstellungen"); } }
        }

        private void ShowSection(NativeSection section, StudioAction selected = null)
        {
            ShowSection(section, selected, -1);
        }

        private void ShowSection(NativeSection section, StudioAction selected, int requestedTab)
        {
            if (current != null && !Object.ReferenceEquals(current, section) && !ConfirmDiscardEditors())
            {
                StudioAction currentItem = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => Object.ReferenceEquals(item.Tag, current));
                SelectNavigationItem(currentItem);
                return;
            }
            if (Object.ReferenceEquals(current, section) && requestedTab >= 0)
            {
                if (!ConfirmDiscardEditors()) return;
                ShowSubTab(section, requestedTab);
                return;
            }

            current = section;
            RestartSection = section.Id;
            if (!historyNavigation)
            {
                if (historyIndex < history.Count - 1) history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
                if (historyIndex < 0 || !Object.ReferenceEquals(history[historyIndex], section))
                {
                    history.Add(section);
                    historyIndex = history.Count - 1;
                }
                UpdateHistoryButtons();
            }
            List<SectionTab> tabs = BuildTabs(section);
            int tabIndex = requestedTab;
            if (tabIndex < 0)
            {
                bool remembered = tabSelection.TryGetValue(section.Id, out tabIndex);
                string query = searchBox == null ? String.Empty : searchBox.Text.Trim();
                if (query.Length > 0)
                {
                    int match = tabs.FindIndex(tab => TabMatches(tab, query));
                    if (match >= 0) tabIndex = match;
                }
                else if (!remembered)
                {
                    tabIndex = 0;
                }
            }
            if (tabIndex < 0 || tabIndex >= tabs.Count) tabIndex = 0;
            tabSelection[section.Id] = tabIndex;

            table.SuspendLayout();
            try
            {
                table.Controls.Clear();
                table.RowStyles.Clear();
                table.RowCount = 0;
                tabStrip = null;
                tabRowIndex = -1;
                contentRowIndex = -1;
                pageTitle.Text = section.Title;
                pageTitle.Font = heading;
                pageTitle.ForeColor = palette.Text;
                pageTitle.Margin = new Padding(0, 0, 0, Px(2));
                table.Controls.Add(pageTitle, 0, table.RowCount);
                table.SetColumnSpan(pageTitle, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                pageLead.Text = section.Subtitle;
                pageLead.Font = small;
                pageLead.ForeColor = palette.Muted;
                pageLead.Margin = new Padding(0, 0, 0, tabs.Count > 1 ? Px(10) : Px(18));
                table.Controls.Add(pageLead, 0, table.RowCount);
                table.SetColumnSpan(pageLead, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                if (tabs.Count > 1)
                {
                    AddTabStrip(tabs, tabIndex, selected);
                    AddTabSpacer();
                }
                contentRowIndex = table.RowCount;
                if (tabs.Count > 0)
                {
                    SectionTab tab = tabs[tabIndex];
                    if (tab.Group != null) AddGroup(tab.Group, tabs.Count == 1 && !String.Equals(section.Id, "project", StringComparison.OrdinalIgnoreCase));
                    else AddRemaining(tab.Remaining, tabs.Count == 1);
                }
            }
            finally { table.ResumeLayout(true); }
            StudioAction targetItem = selected ?? navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => Object.ReferenceEquals(item.Tag, section));
            SelectNavigationItem(targetItem);
            if (targetItem != null && (targetItem.Bounds.Top < 0 || targetItem.Bounds.Bottom > navigation.ClientSize.Height))
                navigation.AutoScrollPosition = new Point(0, Math.Max(0, targetItem.Bounds.Top));
            LayoutTable();
            scroll.AutoScrollPosition = Point.Empty;
            if (IsHandleCreated) BeginInvoke(new Action(delegate { scroll.AutoScrollPosition = Point.Empty; }));
        }

        /// <summary>Swaps only the content below the subsection strip so the page header, sidebar selection and scroll state stay stable.</summary>
        private void ShowSubTab(NativeSection section, int requestedTab)
        {
            List<SectionTab> tabs = BuildTabs(section);
            if (requestedTab < 0 || requestedTab >= tabs.Count) requestedTab = 0;
            if (contentRowIndex < 0 || tabStrip == null || tabStrip.Parent != table)
            {
                tabSelection[section.Id] = requestedTab;
                ShowSection(section, null, -1);
                return;
            }
            tabSelection[section.Id] = requestedTab;
            int index = 0;
            foreach (StudioSubTab item in tabStrip.Controls.OfType<StudioSubTab>())
            {
                item.Selected = index == requestedTab;
                item.Invalidate();
                index++;
            }
            int scrollY = Math.Max(0, -scroll.AutoScrollPosition.Y);
            table.SuspendLayout();
            try
            {
                for (int i = table.Controls.Count - 1; i >= 0; i--)
                {
                    Control control = table.Controls[i];
                    if (table.GetRow(control) >= contentRowIndex)
                    {
                        table.Controls.RemoveAt(i);
                        control.Dispose();
                    }
                }
                while (table.RowStyles.Count > contentRowIndex) table.RowStyles.RemoveAt(table.RowStyles.Count - 1);
                table.RowCount = contentRowIndex;
                SectionTab tab = tabs[requestedTab];
                if (tab.Group != null) AddGroup(tab.Group, false);
                else AddRemaining(tab.Remaining, false);
            }
            finally { table.ResumeLayout(true); }
            LayoutTable();
            scroll.AutoScrollPosition = new Point(0, scrollY);
            if (contentScroll != null) contentScroll.SyncNow();
        }

        private bool ConfirmDiscardEditors()
        {
            IStudioFileEditor editor = table.Controls.OfType<IStudioFileEditor>().FirstOrDefault();
            return editor == null || !editor.HasUnsavedChanges || editor.ConfirmDiscardChanges();
        }

        private List<SectionTab> BuildTabs(NativeSection section)
        {
            var tabs = new List<SectionTab>();
            foreach (NativeGroup group in section.Groups)
                tabs.Add(new SectionTab { TitleDe = group.TitleDe, TitleEn = group.TitleEn, Group = group });
            List<KeyValuePair<string, List<KeyValuePair<string, string>>>> remaining = ComputeRemaining(section);
            if (appearance.ExpertMode && remaining.Count > 0)
                tabs.Add(new SectionTab { TitleDe = "Weitere Einstellungen", TitleEn = "More settings", Remaining = remaining });
            return tabs;
        }

        private static bool TabMatches(SectionTab tab, string query)
        {
            if (tab.Group == null) return false;
            if (Contains(tab.TitleDe, query) || Contains(tab.TitleEn, query)) return true;
            foreach (NativeField field in tab.Group.Fields)
                if (Contains(field.LabelDe, query) || Contains(field.LabelEn, query) || Contains(field.HelpDe, query) || Contains(field.HelpEn, query)) return true;
            return false;
        }

        private void AddTabStrip(List<SectionTab> tabs, int active, StudioAction navigationItem)
        {
            var strip = new StudioTabStrip(Px(9))
            {
                Dock = DockStyle.Top,
                Height = Px(36),
                BackColor = palette.Background,
                AccessibleName = Localization.T("Unterbereiche"),
                Margin = new Padding(0)
            };
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                string caption = tabs[i].Title;
                var tab = new StudioSubTab(scale)
                {
                    Text = caption,
                    Font = body,
                    BackColor = palette.Background,
                    Colors = () => palette,
                    Selected = i == active,
                    AccessibleName = caption,
                    Name = "studioSubTab",
                    Margin = new Padding(0)
                };
                tab.Width = Math.Min(Px(340), Math.Max(Px(64), TextRenderer.MeasureText(caption, body).Width + Px(30)));
                tab.Click += delegate { if (current != null) ShowSection(current, navigationItem, index); };
                strip.Add(tab);
            }
            table.Controls.Add(strip, 0, table.RowCount);
            table.SetColumnSpan(strip, 2);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(36)));
            tabStrip = strip;
            tabRowIndex = table.RowCount;
            table.RowCount++;
            strip.SizeChanged += delegate
            {
                if (tabRowIndex < 0 || tabRowIndex >= table.RowStyles.Count) return;
                if (table.RowStyles[tabRowIndex].Height != strip.Height)
                {
                    table.RowStyles[tabRowIndex].Height = strip.Height;
                    table.PerformLayout();
                }
            };
        }

        private void AddTabSpacer()
        {
            int height = Px(26);
            var spacer = new Panel { Height = height, Dock = DockStyle.Fill, BackColor = palette.Background, Margin = new Padding(0) };
            table.Controls.Add(spacer, 0, table.RowCount);
            table.SetColumnSpan(spacer, 2);
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            table.RowCount++;
        }

        private void AddGroup(NativeGroup group)
        {
            AddGroup(group, true);
        }

        private void AddGroup(NativeGroup group, bool showTitle)
        {
            if (showTitle)
            {
                var groupTitle = new Label { Text = group.Title, Font = Typography.Create(12f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, UseMnemonic = false, Padding = new Padding(Px(12), 0, 0, 0), Margin = new Padding(0, Px(8), 0, Px(8)) };
                groupTitle.Paint += delegate(object sender, PaintEventArgs e)
                {
                    using (var brush = new SolidBrush(palette.Accent)) e.Graphics.FillRectangle(brush, 0, Px(3), Math.Max(2, Px(3)), Math.Max(2, groupTitle.Height - Px(6)));
                };
                table.Controls.Add(groupTitle, 0, table.RowCount);
                table.SetColumnSpan(groupTitle, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
            }
            foreach (NativeField field in group.Fields) AddField(field);
            var spacer = new Panel { Height = Px(10), Dock = DockStyle.Fill, BackColor = palette.Background };
            table.Controls.Add(spacer, 0, table.RowCount);
            table.SetColumnSpan(spacer, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private List<KeyValuePair<string, List<KeyValuePair<string, string>>>> ComputeRemaining(NativeSection page)
        {
            var pending = new List<KeyValuePair<string, List<KeyValuePair<string, string>>>>();
            foreach (string configSection in page.Groups.SelectMany(group => group.Fields).Select(field => field.Section).Where(name => !String.IsNullOrEmpty(name)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var all = configuration.Entries(configSection).ToList();
                if (all.Count == 0) continue;
                bool coversAll = false;
                var exact = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var prefixes = new List<string>();
                foreach (NativeField field in page.Groups.SelectMany(group => group.Fields).Where(field => String.Equals(field.Section, configSection, StringComparison.OrdinalIgnoreCase)))
                {
                    if (field.Kind == NativeFieldKind.GroupedSection || field.Kind == NativeFieldKind.RawSections || field.Kind == NativeFieldKind.VirtualDrive) { coversAll = true; break; }
                    if (field.Kind == NativeFieldKind.Shares)
                    {
                        prefixes.AddRange(new[] { "sharename", "sharepath", "sharescan", "pincode", "sharetype", "shareserver", "sharelogin", "sharepassword", "shareport", "shareremotepath", "shareview", "sharewakeonlan", "hostdetectmethod", "sharedonotfolderjpgifpin", "videothumbscreate", "eachfolderismovie" });
                        continue;
                    }
                    if (field.Kind == NativeFieldKind.DynamicEntries || field.Kind == NativeFieldKind.DynamicBools)
                    {
                        if (field.EntryPrefixes == null || field.EntryPrefixes.Length == 0) { coversAll = true; break; }
                        prefixes.AddRange(field.EntryPrefixes);
                        continue;
                    }
                    if (field.Kind == NativeFieldKind.Info || field.Kind == NativeFieldKind.ProjectInfo || field.Kind == NativeFieldKind.PluginOverview || field.Kind == NativeFieldKind.XmlFiles || field.Kind == NativeFieldKind.Keymap || field.Kind == NativeFieldKind.Views || field.Kind == NativeFieldKind.Dsp
                        || field.Kind == NativeFieldKind.TvServer || field.Kind == NativeFieldKind.PluginConfig || field.Kind == NativeFieldKind.Classic) continue;
                    if (!String.IsNullOrEmpty(field.Entry)) exact.Add(field.Entry);
                }
                if (coversAll) continue;
                var leftovers = all.Where(entry => !exact.Contains(entry.Key)
                    && !prefixes.Any(prefix => entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))).ToList();
                if (leftovers.Count > 0) pending.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(configSection, leftovers));
            }
            return pending;
        }

        private void AddRemaining(List<KeyValuePair<string, List<KeyValuePair<string, string>>>> pending, bool showTitle)
        {
            if (pending == null || pending.Count == 0) return;

            if (showTitle)
            {
                var groupTitle = new Label { Text = Localization.T("Weitere Einstellungen"), Font = Typography.Create(12f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, UseMnemonic = false, Padding = new Padding(Px(12), 0, 0, 0), Margin = new Padding(0, Px(8), 0, Px(8)) };
                groupTitle.Paint += delegate(object sender, PaintEventArgs e)
                {
                    using (var brush = new SolidBrush(palette.Accent)) e.Graphics.FillRectangle(brush, 0, Px(3), Math.Max(2, Px(3)), Math.Max(2, groupTitle.Height - Px(6)));
                };
                table.Controls.Add(groupTitle, 0, table.RowCount);
                table.SetColumnSpan(groupTitle, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
            }

            foreach (var sectionEntries in pending)
            {
                var heading = new Label { Text = sectionEntries.Key, Font = Typography.Create(9.5f * scale, FontStyle.Bold), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(4), 0, Px(2)) };
                table.Controls.Add(heading, 0, table.RowCount);
                table.SetColumnSpan(heading, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                foreach (var entry in sectionEntries.Value)
                {
                    string key = entry.Key;
                    string value = entry.Value ?? string.Empty;
                    bool boolValue = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                        || String.Equals(value, "no", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
                    if (boolValue)
                    {
                        var check = new StudioCheck(scale) { Text = key, Checked = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), Font = body, Colors = () => palette, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0, Px(1), 0, Px(1)) };
                        check.CheckedChanged += delegate { configuration.SetBool(sectionEntries.Key, key, check.Checked); MarkDirty(); };
                        table.Controls.Add(check, 0, table.RowCount);
                        table.SetColumnSpan(check, 2);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                    else
                    {
                        var label = new Label { Text = key, Font = body, ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Margin = new Padding(0, 0, Px(12), 0) };
                        table.Controls.Add(label, 0, table.RowCount);
                        Control input;
                        List<ChoiceOption> choices = ChoiceCatalog.Resolve(sectionEntries.Key, key);
                        if (choices.Count > 0)
                        {
                            input = CreateChoiceControl(sectionEntries.Key, key, choices);
                        }
                        else
                        {
                            var text = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                            text.Text = value;
                            text.Box.TextChanged += delegate { configuration.Set(sectionEntries.Key, key, text.Text); MarkDirty(); };
                            input = text;
                        }
                        table.Controls.Add(input, 1, table.RowCount);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                }
            }
            var spacer = new Panel { Height = Px(10), Dock = DockStyle.Fill, BackColor = palette.Background };
            table.Controls.Add(spacer, 0, table.RowCount);
            table.SetColumnSpan(spacer, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private Control CreateChoiceControl(string section, string entry, List<ChoiceOption> options)
        {
            var combo = new StudioChoice(scale)
            {
                Font = body,
                Colors = () => palette,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            combo.SetOptions(options, configuration.Get(section, entry, string.Empty));
            combo.SelectedIndexChanged += delegate
            {
                if (combo.SelectedIndex < 0) return;
                configuration.Set(section, entry, combo.Options[combo.SelectedIndex].Value);
                MarkDirty();
            };
            return combo;
        }

        private void AddField(NativeField field)
        {
            if (field.Kind == NativeFieldKind.ProjectInfo)
            {
                var panel = new NativeProjectPanel(scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(640)));
                table.RowCount++;
                return;
            }
            if (field.Kind == NativeFieldKind.RawSections)
            {
                var editor = new NativeRawSectionEditor(configuration, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                editor.Changed += delegate { MarkDirty(); };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(500)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Classic)
            {
                var button = new StudioButton { Text = field.Label, Font = body, Height = Px(32), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(12), 0, Px(12), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, Px(2), 0, Px(4)) };
                button.FlatAppearance.BorderColor = palette.Border;
                button.Click += delegate { StartClassic(); };
                table.Controls.Add(button, 0, table.RowCount);
                table.SetColumnSpan(button, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Keymap)
            {
                var editor = new NativeKeymapEditor(Path.GetDirectoryName(configuration.Path), scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(620)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Views)
            {
                string language = configuration.Get("gui", "language", "English");
                var editor = new NativeViewsEditor(Path.GetDirectoryName(configuration.Path), field.Entry, language, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(640)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Dsp)
            {
                var editor = new NativeDspEditor(Path.GetDirectoryName(configuration.Path), runtime, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(580)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.TvServer)
            {
                var panel = new NativeTvServerPanel(runtime, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(280)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.SystemChecks)
            {
                var panel = new NativeSystemChecksPanel(runtime, scale, delegate { StartClassic(); }) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(210)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.VirtualDrive)
            {
                var panel = new NativeVirtualDrivePanel(configuration, runtime, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                panel.Changed += delegate { MarkDirty(); };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(300)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.PluginConfig)
            {
                var panel = new NativePluginConfigPanel(runtime, scale, delegate { StartClassic(); }) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(210)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.PluginOverview)
            {
                int overviewHeight = (int)Math.Round(Px(1600) * Math.Max(1f, scale));
                var panel = new NativePluginOverviewPanel(configuration, runtime, scale, delegate { StartClassic(); }, delegate { MarkDirty(); }, delegate { }) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(panel, 0, table.RowCount);
                table.SetColumnSpan(panel, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, overviewHeight));
                table.RowCount++;
                return;
            }
            if (field.Kind == NativeFieldKind.GroupedSection)
            {
                Func<string, string> group = String.Equals(field.Section, "Trakt", StringComparison.OrdinalIgnoreCase)
                    ? (Func<string, string>)NativeGroupedSectionEditor.TraktGroup
                    : NativeGroupedSectionEditor.OnlineVideosGroup;
                var editor = new NativeGroupedSectionEditor(configuration, field.Section, group, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                editor.Changed += delegate { MarkDirty(); };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(String.Equals(field.Section, "Trakt", StringComparison.OrdinalIgnoreCase) ? 640 : 300)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.XmlFiles)
            {
                var editor = new NativeXmlFileEditor(Path.GetDirectoryName(configuration.Path), runtime, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(8)) };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(500)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.DynamicEntries)
            {
                var entries = configuration.Entries(field.Section).ToArray();
                if (field.EntryPrefixes != null && field.EntryPrefixes.Length > 0)
                    entries = entries.Where(entry => field.EntryPrefixes.Any(prefix => entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))).ToArray();
                int count = 0;
                foreach (var entry in entries)
                {
                    string key = entry.Key;
                    string value = entry.Value ?? "";
                    bool boolValue = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                        || String.Equals(value, "no", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
                    if (boolValue)
                    {
                        var check = new StudioCheck(scale) { Text = key, Checked = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), Font = body, Colors = () => palette, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0, Px(1), 0, Px(1)) };
                        check.CheckedChanged += delegate { configuration.SetBool(field.Section, key, check.Checked); MarkDirty(); };
                        table.Controls.Add(check, 0, table.RowCount);
                        table.SetColumnSpan(check, 2);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                    else
                    {
                        var entryLabel = new Label { Text = key, Font = body, ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Margin = new Padding(0, 0, Px(12), 0) };
                        var text = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                        text.Text = value;
                        text.Box.TextChanged += delegate { configuration.Set(field.Section, key, text.Text); MarkDirty(); };
                        table.Controls.Add(entryLabel, 0, table.RowCount);
                        table.Controls.Add(text, 1, table.RowCount);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                    count++;
                }
                if (count == 0)
                {
                    var empty = new Label { Text = Localization.T("Keine Einträge in diesem Bereich gefunden."), Font = small, ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(2), 0, Px(10)) };
                    table.Controls.Add(empty, 0, table.RowCount);
                    table.SetColumnSpan(empty, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                }
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.DynamicBools)
            {
                int count = 0;
                foreach (var entry in configuration.Entries(field.Section).ToArray())
                {
                    if (String.IsNullOrEmpty(entry.Key)) continue;
                    string key = entry.Key;
                    var check = new StudioCheck(scale) { Text = key, Checked = configuration.GetBool(field.Section, key, false), Font = body, Colors = () => palette, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0, Px(1), 0, Px(1)) };
                    check.CheckedChanged += delegate { configuration.SetBool(field.Section, key, check.Checked); MarkDirty(); };
                    table.Controls.Add(check, 0, table.RowCount);
                    table.SetColumnSpan(check, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                    count++;
                }
                if (count == 0)
                {
                    var empty = new Label { Text = Localization.T("Keine Einträge in der Konfiguration gefunden."), Font = small, ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(2), 0, Px(10)) };
                    table.Controls.Add(empty, 0, table.RowCount);
                    table.SetColumnSpan(empty, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                }
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Shares)
            {
                var editor = new NativeShareEditor(configuration, field.Section, scale) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, Px(6)) };
                editor.Changed += delegate { MarkDirty(); };
                table.Controls.Add(editor, 0, table.RowCount);
                table.SetColumnSpan(editor, 2);
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, editor.PreferredHeight + Px(8)));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            if (field.Kind == NativeFieldKind.Info)
            {
                var info = new Label { Text = field.Help, Font = small, ForeColor = palette.Muted, AutoSize = true, MaximumSize = new Size(Px(760), 0), Margin = new Padding(0, Px(2), 0, Px(10)) };
                table.Controls.Add(info, 0, table.RowCount);
                table.SetColumnSpan(info, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                return;
            }
            string labelText = field.Label;
            if (field.Kind == NativeFieldKind.Bool)
            {
                var check = new StudioCheck(scale) { Text = labelText, Checked = configuration.GetBool(field.Section, field.Entry, false), Font = body, Colors = () => palette, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0, Px(1), 0, Px(1)) };
                check.CheckedChanged += delegate { configuration.SetBool(field.Section, field.Entry, check.Checked); MarkDirty(); };
                AttachFieldTip(check, field);
                table.Controls.Add(check, 0, table.RowCount);
                table.SetColumnSpan(check, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                AddHelp(field);
                return;
            }
            var label = new Label { Text = labelText, Font = body, ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Margin = new Padding(0, 0, Px(12), 0) };
            table.Controls.Add(label, 0, table.RowCount);
            Control input;
            List<ChoiceOption> catalogChoices = ChoiceCatalog.Resolve(field.Section, field.Entry);
            if ((field.Kind == NativeFieldKind.Text || field.Kind == NativeFieldKind.Integer) && catalogChoices.Count > 0)
            {
                input = CreateChoiceControl(field.Section, field.Entry, catalogChoices);
            }
            else if (field.Kind == NativeFieldKind.Combo)
            {
                var choices = new List<ChoiceOption>();
                foreach (NativeOption option in field.Options) choices.Add(new ChoiceOption(option.Value, option.Label));
                input = CreateChoiceControl(field.Section, field.Entry, choices);
            }
            else if (field.Kind == NativeFieldKind.Filter)
            {
                var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0), Height = Px(32) };
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(38)));
                var combo = new StudioChoice(scale) { Font = body, Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                string current = configuration.Get(field.Section, field.Entry, "");
                var choices = new List<ChoiceOption>();
                foreach (string name in NativeFilterCatalog.Names(field.FilterKind)) choices.Add(new ChoiceOption(name, name));
                combo.SetOptions(choices, current);
                combo.SelectedIndexChanged += delegate { configuration.Set(field.Section, field.Entry, combo.SelectedValue); MarkDirty(); };
                var configure = new GlassButton { Text = "…", Font = body, Colors = () => palette, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(Px(4), 0, 0, 0) };
                configure.FlatAppearance.BorderColor = palette.Border;
                configure.Click += delegate { NativeFilterProperties.Show(FindForm(), field.FilterKind, String.IsNullOrWhiteSpace(combo.SelectedValue) ? null : combo.SelectedValue); };
                row.Controls.Add(combo, 0, 0);
                row.Controls.Add(configure, 1, 0);
                input = row;
            }
            else if (field.Kind == NativeFieldKind.Integer)
            {
                var text = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                text.Text = configuration.GetInt(field.Section, field.Entry, field.Minimum).ToString();
                text.Box.Validated += delegate { int value; if (!Int32.TryParse(text.Text, out value)) value = field.Minimum; value = Math.Max(field.Minimum, Math.Min(field.Maximum, value)); text.Text = value.ToString(); configuration.SetInt(field.Section, field.Entry, value); MarkDirty(); };
                input = text;
            }
            else if (field.Kind == NativeFieldKind.Folder)
            {
                var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0), Height = Px(32) };
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(38)));
                var folder = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                folder.Text = configuration.Get(field.Section, field.Entry, "");
                folder.Box.TextChanged += delegate { configuration.Set(field.Section, field.Entry, folder.Text); MarkDirty(); };
                var browse = new GlassButton { Text = "…", Font = body, Colors = () => palette, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(Px(4), 0, 0, 0) };
                browse.FlatAppearance.BorderColor = palette.Border;
                browse.Click += delegate
                {
                    using (var picker = new FolderBrowserDialog())
                    {
                        picker.Description = Localization.T("Ordner auswählen");
                        if (Directory.Exists(folder.Text)) picker.SelectedPath = folder.Text;
                        if (picker.ShowDialog(FindForm()) == DialogResult.OK) folder.Text = picker.SelectedPath;
                    }
                };
                row.Controls.Add(folder, 0, 0);
                row.Controls.Add(browse, 1, 0);
                input = row;
            }
            else
            {
                var text = new StudioTextInput(scale, body, palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                text.Text = configuration.Get(field.Section, field.Entry, "");
                text.Box.TextChanged += delegate { configuration.Set(field.Section, field.Entry, text.Text); MarkDirty(); };
                input = text;
            }
            table.Controls.Add(input, 1, table.RowCount);
            if (!String.IsNullOrEmpty(labelText)) input.AccessibleName = labelText;
            AttachFieldTip(label, field);
            AttachFieldTip(input, field);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
            AddHelp(field);
        }

        private void AddHelp(NativeField field)
        {
            string help = field.Help;
            if (String.IsNullOrEmpty(help)) return;
            var label = new Label { Text = help, Font = small, ForeColor = palette.Muted, AutoSize = true, MaximumSize = new Size(Px(740), 0), Margin = new Padding(0, Px(1), 0, Px(10)) };
            table.Controls.Add(label, 0, table.RowCount);
            table.SetColumnSpan(label, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private ToolTip fieldTips;

        private ToolTip FieldTips()
        {
            if (fieldTips == null)
            {
                fieldTips = new ToolTip { AutoPopDelay = 15000, InitialDelay = 500, ReshowDelay = 120 };
            }
            return fieldTips;
        }

        private void AttachFieldTip(Control control, NativeField field)
        {
            if (control == null || field == null) return;
            string help = field.Help;
            if (String.IsNullOrEmpty(help))
            {
                string fallback;
                if (!MediaPortalDefaults.TryGet(field.Section, field.Entry, out fallback)) return;
                help = (Localization.T("Standard: ")) + fallback;
            }
            ApplyTip(control, help);
        }

        private void ApplyTip(Control control, string tip)
        {
            FieldTips().SetToolTip(control, tip);
            foreach (Control child in control.Controls) ApplyTip(child, tip);
        }

        private void MarkDirty()
        {
            dirty = HasUnsavedWork();
            if (statusTimer != null) statusTimer.Stop();
            if (dirty)
            {
                status.Text = Localization.T("Ungespeicherte Änderungen");
                status.ForeColor = palette.Accent;
                status.Font = smallUnderline;
                status.Cursor = Cursors.Hand;
            }
            else
            {
                RestoreStatus();
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                bool fileChanges = false;
                bool fileFailure = false;
                foreach (IStudioFileEditor editor in table.Controls.OfType<IStudioFileEditor>().Where(item => item.HasUnsavedChanges).ToArray())
                {
                    fileChanges = true;
                    if (!editor.SaveChanges()) fileFailure = true;
                }
                if (fileFailure)
                {
                    ShowStatusMessage(Localization.T("Eine oder mehrere Dateien konnten nicht gespeichert werden."), palette.Accent, 6000);
                    return;
                }
                string backup = configuration.Save();
                dirty = false;
                appearance.Save();
                string message = !String.IsNullOrEmpty(backup)
                    ? (Localization.T("Gespeichert · Sicherung: ")) + System.IO.Path.GetFileName(backup)
                    : fileChanges
                        ? (Localization.T("Dateien gespeichert"))
                        : (Localization.T("Keine Änderungen zu speichern"));
                ShowStatusMessage(message, palette.Accent, 6000);
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Native configuration save failed");
                MessageBox.Show(this, (Localization.T("Die Konfiguration konnte nicht gespeichert werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckForUpdates()
        {
            if (updateButton == null) return;
            updateButton.Enabled = false;
            status.Text = Localization.T("Update-Prüfung läuft …");
            status.ForeColor = palette.Muted;
            string current = UpdateChecker.CurrentVersion;
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateChecker.Result result = UpdateChecker.Check(current);
                try
                {
                    if (IsDisposed || !IsHandleCreated) return;
                    BeginInvoke(new Action(delegate
                    {
                        updateButton.Enabled = true;
                        if (!String.IsNullOrEmpty(result.Error))
                        {
                            status.Text = result.Error;
                            MessageBox.Show(this, (Localization.T("Update-Prüfung fehlgeschlagen:\r\n")) + result.Error, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (!result.Newer)
                        {
                            status.Text = (Localization.T2("Aktuell: ", "Up to date: ")) + result.Latest;
                            MessageBox.Show(this, (Localization.T("Sie verwenden die neueste Version (")) + current + ").", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        status.Text = (Localization.T("Update verfügbar: ")) + result.Latest;
                        if (MessageBox.Show(this, (Localization.T("Version ")) + result.Latest + (Localization.T(" ist verfügbar. Release-Seite öffnen?")), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            try { Process.Start(result.Url); } catch { }
                        }
                    }));
                }
                catch { }
            });
        }

        private void CreateBackup()
        {
            try
            {
                string folder = ConfigurationBackup.DefaultBackupDirectory;
                Directory.CreateDirectory(folder);
                string backup = System.IO.Path.Combine(folder, "native-manual-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".zip");
                ConfigurationBackup.Create(configuration.Path, backup);
                ShowStatusMessage((Localization.T("Sicherung erstellt · ")) + System.IO.Path.GetFileName(backup), palette.Accent, 6000);
                StudioLog.Info("Native manual backup: " + backup);
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Native manual backup failed");
                MessageBox.Show(this, (Localization.T("Die Sicherung konnte nicht erstellt werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowStatusMessage(string text, Color color, int milliseconds)
        {
            status.Text = text;
            status.ForeColor = color;
            status.Font = small;
            status.Cursor = dirty ? Cursors.Hand : Cursors.Default;
            if (statusTimer == null)
            {
                statusTimer = new System.Windows.Forms.Timer();
                statusTimer.Tick += delegate { statusTimer.Stop(); RestoreStatus(); };
            }
            statusTimer.Stop();
            if (milliseconds > 0)
            {
                statusTimer.Interval = milliseconds;
                statusTimer.Start();
            }
        }

        private void RestoreStatus()
        {
            if (dirty)
            {
                status.Text = Localization.T("Ungespeicherte Änderungen");
                status.ForeColor = palette.Accent;
                status.Font = smallUnderline;
                status.Cursor = Cursors.Hand;
            }
            else
            {
                status.Text = Localization.T("Bereit");
                status.ForeColor = palette.Muted;
                status.Font = small;
                status.Cursor = Cursors.Default;
            }
        }

        private void ShowPendingChanges()
        {
            int files = table.Controls.OfType<IStudioFileEditor>().Count(editor => editor.HasUnsavedChanges);
            if (!dirty && files == 0)
            {
                ShowStatusMessage(Localization.T("Keine ausstehenden Änderungen"), palette.Muted, 4000);
                return;
            }
            using (var dialog = new PendingChangesDialog(configuration.PendingChanges, files, scale)) dialog.ShowDialog(this);
        }

        private void ShowToolsMenu()
        {
            var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = palette.Panel, Padding = new Padding(Px(4)), Width = Px(250) };
            var menu = new ToolStripDropDown { Padding = Padding.Empty, AutoSize = true, DropShadowEnabled = true, BackColor = palette.Panel };
            Action<string, Action> add = delegate(string text, Action action)
            {
                var item = new StudioButton { Text = text, Font = body, Colors = () => palette, BackColor = palette.Panel, ForeColor = palette.Text, Width = Px(232), Height = Px(32), Margin = new Padding(0, 1, 0, 1) };
                item.Click += delegate { menu.Close(); action(); };
                panel.Controls.Add(item);
            };
            add(Localization.T("Jetzt sichern"), CreateBackup);
            add(Localization.T("Sicherungen verwalten…"), ShowBackupManager);
            add("English", delegate { SwitchLanguage("en"); });
            add("Deutsch", delegate { SwitchLanguage("de"); });
            add("Français", delegate { SwitchLanguage("fr"); });
            add("Nederlands", delegate { SwitchLanguage("nl"); });
            add(Localization.T("MediaPortal starten"), StartMediaPortal);
            add(Localization.T("Klassische Oberfläche"), StartClassic);
            add(Localization.T("Studio 1.9-Oberfläche"), StartStudioShell);

            add(Localization.T("Support-Bundle…"), ShowSupportBundle);
            add(Localization.T("Über"), ShowAbout);
            menu.Items.Add(new ToolStripControlHost(panel) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = true });
            menu.Show(toolsButton, new Point(0, toolsButton.Height + Px(2)));
        }

        private void ShowBackupManager()
        {
            using (var dialog = new BackupManagerDialog()) dialog.ShowDialog(this);
        }

        private void ShowSupportBundle()
        {
            using (var dialog = new SaveFileDialog { Title = Localization.T("Support-Bundle speichern"), Filter = "ZIP (*.zip)|*.zip", FileName = "mp1-studio-support-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    SupportBundle.Create(dialog.FileName, appearance.PathName);
                    ShowStatusMessage((Localization.T("Support-Bundle erstellt: ")) + System.IO.Path.GetFileName(dialog.FileName), palette.Accent, 6000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, (Localization.T("Das Support-Bundle konnte nicht erstellt werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowAbout()
        {
            using (var dialog = new AboutDialog(scale)) dialog.ShowDialog(this);
        }

        private bool ConfirmPreferenceRestart()
        {
            return SaveOrDiscardChanges();
        }

        private void SwitchLanguage(string code)
        {
            if (String.Equals(appearance.Language, code, StringComparison.OrdinalIgnoreCase)) return;
            if (!ConfirmPreferenceRestart()) return;
            appearance.Language = code;
            appearance.Save();
            Localization.Set(code);
            RestartSection = current == null ? null : current.Id;
            RestartNativeRequested = true;
            suppressClosePrompt = true;
            Close();
        }

        private void ChangeTheme(string mode)
        {
            string target = String.Equals(mode, "Hell", StringComparison.OrdinalIgnoreCase) ? "Hell" : "Dunkel";
            if (String.Equals(appearance.ThemeMode, target, StringComparison.OrdinalIgnoreCase)) return;
            if (!ConfirmPreferenceRestart())
            {
                themeChoice.SelectValue(appearance.ThemeMode);
                return;
            }
            appearance.ThemeMode = target;
            appearance.Dark = target == "Dunkel";
            appearance.Save();
            Palette.UseLight = target == "Hell";
            RestartSection = current == null ? null : current.Id;
            RestartNativeRequested = true;
            suppressClosePrompt = true;
            Close();
        }

        private void NavigateHistory(int delta)
        {
            int next = historyIndex + delta;
            if (next < 0 || next >= history.Count) return;
            historyNavigation = true;
            try
            {
                historyIndex = next;
                NativeSection section = history[next];
                StudioAction item = navigation.Controls.OfType<StudioAction>().FirstOrDefault(candidate => Object.ReferenceEquals(candidate.Tag, section));
                ShowSection(section, item);
            }
            finally
            {
                historyNavigation = false;
            }
            UpdateHistoryButtons();
        }

        private void UpdateHistoryButtons()
        {
            if (backButton != null) backButton.Enabled = historyIndex > 0;
            if (forwardButton != null) forwardButton.Enabled = historyIndex >= 0 && historyIndex < history.Count - 1;
            if (backButton != null) backButton.Invalidate();
            if (forwardButton != null) forwardButton.Invalidate();
        }

        private void ChangeScale()
        {
            int[] values = { 100, 115, 125, 150, 175 };
            if (sizeChoice.SelectedIndex < 0) return;
            appearance.TextScale = values[sizeChoice.SelectedIndex];
            appearance.Save();
            scale = appearance.TextScale / 100f;
            body = Typography.Create(10f * scale, FontStyle.Regular);
            heading = Typography.Create(18f * scale, FontStyle.Regular);
            small = Typography.Create(9f * scale, FontStyle.Regular);
            smallUnderline = new Font(small, small.Style | FontStyle.Underline);
            Font = body;
            ApplyMetrics();
            ShowSection(current);
        }

        private void ChangeScale(int delta)
        {
            if (sizeChoice == null || sizeChoice.Count == 0) return;
            int next = Math.Max(0, Math.Min(sizeChoice.Count - 1, sizeChoice.SelectedIndex + delta));
            if (next == sizeChoice.SelectedIndex) return;
            sizeChoice.SelectedIndex = next;
        }

        private void OpenSectionHelp()
        {
            NativeSection section = current;
            string query = section == null ? "MediaPortal" : (section.Title);
            string url = "https://www.team-mediaportal.com/wiki/dosearchsite.action?query=" + Uri.EscapeDataString(query);
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { StudioLog.Error(ex, "Could not open online help"); ShowStatusMessage(ex.Message, palette.Accent, 4000); }
        }

        private void StartStudioShell()
        {
            if (!SaveOrDiscardChanges()) return;
            suppressClosePrompt = true;
            Palette.UseLight = false;
            StartStudioRequested = true;
            Close();
        }

        private void StartClassic()
        {
            if (!SaveOrDiscardChanges()) return;
            suppressClosePrompt = true;
            Palette.UseLight = false;
            string executable = Path.Combine(runtime, "Configuration.exe");
            Program.StartNative(executable, runtime);
            Close();
        }

        private void StartMediaPortal()
        {
            if (!SaveOrDiscardChanges()) return;
            suppressClosePrompt = true;
            string executable = Path.Combine(runtime, "MediaPortal.exe");
            if (!File.Exists(executable))
            {
                suppressClosePrompt = false;
                MessageBox.Show(this, Localization.T("MediaPortal.exe wurde nicht gefunden:\r\n") + executable, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                StudioLog.Info("Starting MediaPortal");
                Process.Start(new ProcessStartInfo { FileName = executable, WorkingDirectory = runtime, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                suppressClosePrompt = false;
                StudioLog.Error(ex, "MediaPortal start failed");
                MessageBox.Show(this, Localization.T("MediaPortal konnte nicht gestartet werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Close();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (suppressClosePrompt) return;
            if (!SaveOrDiscardChanges()) e.Cancel = true;
        }

        private bool HasUnsavedWork()
        {
            if (configuration.PendingCount > 0) return true;
            return table.Controls.OfType<IStudioFileEditor>().Any(editor => editor.HasUnsavedChanges);
        }

        private bool SaveOrDiscardChanges()
        {
            if (!HasUnsavedWork()) return true;
            int files = table.Controls.OfType<IStudioFileEditor>().Count(editor => editor.HasUnsavedChanges);
            using (var dialog = new PendingChangesDialog(configuration.PendingChanges, files, scale, true))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                if (dialog.DiscardRequested) return true;
                if (dialog.SaveRequested)
                {
                    SaveConfiguration();
                    return !HasUnsavedWork();
                }
            }
            return false;
        }

        private int Px(float value)
        {
            return (int)Math.Round(value * scale);
        }

        private int ToolbarWidth(int baseWidth)
        {
            int cap;
            switch (baseWidth)
            {
                case 34: cap = 44; break;
                case 150: cap = 170; break;
                case 130: cap = 170; break;
                case 120: cap = 150; break;
                case 124: cap = 150; break;
                default: cap = Px(baseWidth); break;
            }
            return Math.Min(Px(baseWidth), cap);
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg != WM_MOUSEWHEEL || !IsHandleCreated) return false;
            Point screen = Cursor.Position;
            if (!ClientRectangle.Contains(PointToClient(screen))) return false;
            Control target = Control.FromHandle(WindowFromPoint(screen));
            int delta = (short)((long)m.WParam >> 16);
            if (Inside(target, navigation))
            {
                ScrollBy(navigation, delta, Px(42));
                if (sidebarScroll != null) sidebarScroll.SyncNow();
                ClearNavigationHover();
                return true;
            }
            if (Inside(target, scroll))
            {
                ScrollBy(scroll, delta, Px(48));
                if (contentScroll != null) contentScroll.SyncNow();
                return true;
            }
            return false;
        }

        private static bool Inside(Control control, Control container)
        {
            for (Control current = control; current != null; current = current.Parent)
                if (Object.ReferenceEquals(current, container)) return true;
            return false;
        }

        private static void ScrollBy(ScrollableControl panel, int delta, int step)
        {
            int notches = Math.Max(1, Math.Abs(delta) / 120);
            int lines = SystemInformation.MouseWheelScrollLines;
            if (lines < 1) lines = 3;
            int amount = step * notches * lines;
            int current = -panel.AutoScrollPosition.Y;
            int next = delta > 0 ? current - amount : current + amount;
            panel.AutoScrollPosition = new Point(0, Math.Max(0, next));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            base.OnFormClosed(e);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, string lParam);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeDarkMode.ApplyWindow(Handle, !Palette.UseLight);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            NativeDarkMode.ApplyWindow(Handle, !Palette.UseLight);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            NativeDarkMode.ApplyWindow(Handle, !Palette.UseLight);
            NativeDarkMode.Apply(navigation.Handle);
            NativeDarkMode.Apply(scroll.Handle);
            EnsureNavigationVisible();
            StudioAction focused = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Selected);
            if (focused != null) focused.Focus();
            ApplyCueBanner();
            if (!appearance.ExpertHintSeen)
            {
                appearance.ExpertHintSeen = true;
                appearance.Save();
                BeginInvoke(new Action(delegate
                {
                    using (var hint = new ExpertHintDialog(scale))
                    {
                        if (hint.ShowDialog(this) == DialogResult.OK && hint.EnableExpert)
                        {
                            appearance.ExpertMode = true;
                            appearance.Save();
                            if (modeChoice != null) modeChoice.SelectValue("expert");
                            ApplySearch();
                        }
                    }
                }));
            }
        }

        private void ApplyChromeStyle()
        {
            navigation.BackColor = palette.Panel;
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>()) item.BackColor = palette.Panel;
            status.BackColor = Color.Transparent;
            if (navigation.IsHandleCreated) navigation.Invalidate(true);
        }

        private void EnsureNavigationVisible()
        {
            StudioAction selected = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Selected);
            if (selected == null) return;
            if (selected.Bounds.Top < 0 || selected.Bounds.Bottom > navigation.ClientSize.Height)
                navigation.AutoScrollPosition = new Point(0, Math.Max(0, selected.Bounds.Top));
            if (sidebarScroll != null) sidebarScroll.SyncNow();
        }

        private void ClearNavigationHover()
        {
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>()) item.ClearHover();
        }

        private void SelectNavigationItem(StudioAction target)
        {
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>())
            {
                bool selected = Object.ReferenceEquals(item, target);
                if (item.Selected != selected)
                {
                    item.Selected = selected;
                    item.Invalidate();
                }
                item.ClearHover();
            }
            if (target != null && target.CanFocus) target.Focus();
            navigation.Invalidate();
        }

        private void ApplySearch()
        {
            string query = searchBox.Text.Trim();
            bool active = query.Length > 0;
            int matches = 0;
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>())
            {
                NativeSection section = item.Tag as NativeSection;
                bool visible = section == null || appearance.ExpertMode || !IsAdvancedSection(section);
                if (active && section != null)
                {
                    visible = visible && MatchesSection(section, query);
                    if (visible) matches++;
                }
                item.Visible = visible;
            }
            foreach (Label header in navigationHeaders)
            {
                string group = header.Tag as string;
                header.Visible = sections.Any(section => String.Equals(NavigationGroup(section), group, StringComparison.OrdinalIgnoreCase)
                    && (appearance.ExpertMode || !IsAdvancedSection(section))
                    && (!active || MatchesSection(section, query)));
            }
            ClearNavigationHover();
            navigation.AutoScrollPosition = Point.Empty;
            if (sidebarScroll != null) sidebarScroll.SyncNow();
            if (active)
            {
                status.Text = matches == 0
                    ? (Localization.T("Keine Treffer"))
                    : (Localization.T("Suche: ")) + matches + (matches == 1 ? Localization.T2(" Seite", " page") : Localization.T2(" Seiten", " pages"));
                status.ForeColor = palette.Muted;
            }
            else
            {
                RestoreStatus();
            }
        }

        private static string NavigationGroup(NativeSection section)
        {
            switch ((section.Id ?? String.Empty).ToLowerInvariant())
            {
                case "project": return "project";
                case "general":
                case "volume":
                case "keymap": return "general";
                case "gui":
                case "home": return "gui";
                case "movies":
                case "moviedatabase":
                case "videoviews":
                case "subtitles": return "videos";
                case "discs": return "discs";
                case "tv":
                case "radio":
                case "recordings": return "tvradio";
                case "music":
                case "musicfiles":
                case "musicviews":
                case "musicdsp": return "music";
                case "pictures": return "pictures";
                case "folders": return "folders";
                case "remote": return "remote";
                case "codecs": return "codecs";
                case "plugins":
                case "plugindata": return "plugins";
                case "mediainfo": return "mediainfo";
                case "system": return "system";
                default: return "extras";
            }
        }

        private static string NavigationGroupTitle(string group)
        {
            switch ((group ?? String.Empty).ToLowerInvariant())
            {
                case "general": return Localization.T("Allgemein");
                case "gui": return "GUI";
                case "videos": return Localization.T("Videos");
                case "tvradio": return "TV / Radio";
                case "music": return Localization.T("Musik");
                case "plugins": return "Plugins";
                default: return Localization.T("Weitere Bereiche");
            }
        }

        private static bool IsAdvancedSection(NativeSection section)
        {
            switch ((section.Id ?? String.Empty).ToLowerInvariant())
            {
                case "system":
                case "moviedatabase":
                case "videoviews":
                case "musicfiles":
                case "musicviews":
                case "musicdsp":
                case "keymap":
                case "codecs":
                case "plugins":
                case "plugindata":
                case "mediainfo":
                case "more":
                case "files":
                case "rawxml":
                case "remote":
                    return true;
            }
            return false;
        }

        private static bool MatchesSection(NativeSection section, string query)
        {
            if (Contains(section.TitleDe, query) || Contains(section.TitleEn, query) || Contains(section.SubtitleDe, query) || Contains(section.SubtitleEn, query)) return true;
            foreach (NativeGroup group in section.Groups)
            {
                if (Contains(group.TitleDe, query) || Contains(group.TitleEn, query)) return true;
                foreach (NativeField field in group.Fields)
                    if (Contains(field.LabelDe, query) || Contains(field.LabelEn, query) || Contains(field.HelpDe, query) || Contains(field.HelpEn, query)) return true;
            }
            return false;
        }

        private static bool Contains(string text, string query)
        {
            return !String.IsNullOrEmpty(text) && text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void ApplyCueBanner()
        {
            try { SendMessage(searchBox.Box.Handle, 0x1501, new IntPtr(1), Localization.T("Einstellungen durchsuchen…")); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Up || keyData == Keys.Down) && navigation.ContainsFocus)
            {
                if (MoveNavigationFocus(keyData == Keys.Down ? 1 : -1)) return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool MoveNavigationFocus(int delta)
        {
            var items = navigation.Controls.OfType<StudioAction>().Where(item => item.Visible).ToList();
            if (items.Count == 0) return false;
            int index = items.FindIndex(item => item.Focused);
            if (index < 0) index = delta > 0 ? -1 : items.Count;
            int next = Math.Max(0, Math.Min(items.Count - 1, index + delta));
            items[next].Focus();
            int position = -navigation.AutoScrollPosition.Y;
            int top = items[next].Top + position;
            int height = items[next].Height;
            int viewport = navigation.ClientSize.Height;
            if (top < position) navigation.AutoScrollPosition = new Point(0, Math.Max(0, top));
            else if (top + height > position + viewport) navigation.AutoScrollPosition = new Point(0, Math.Max(0, top + height - viewport));
            if (sidebarScroll != null) sidebarScroll.SyncNow();
            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveConfiguration();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.F)
            {
                searchBox.Box.Focus();
                searchBox.Box.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.W)
            {
                Close();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp))
            {
                Navigate(e.KeyCode == Keys.PageDown ? 1 : -1);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Left)
            {
                NavigateHistory(-1);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Right)
            {
                NavigateHistory(1);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus))
            {
                ChangeScale(1);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus))
            {
                ChangeScale(-1);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.F1)
            {
                if (e.Shift)
                {
                    using (var dialog = new KeyboardHelpDialog()) dialog.ShowDialog(this);
                }
                else
                {
                    OpenSectionHelp();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        private void Navigate(int delta)
        {
            int index = sections.IndexOf(current);
            if (index < 0) return;
            int next = Math.Max(0, Math.Min(sections.Count - 1, index + delta));
            if (next == index) return;
            NativeSection section = sections[next];
            StudioAction item = navigation.Controls.OfType<StudioAction>().FirstOrDefault(candidate => Object.ReferenceEquals(candidate.Tag, section));
            ShowSection(section, item);
            EnsureNavigationVisible();
        }
    }

    internal sealed class GlassFlowPanel : FlowLayoutPanel
    {
        public GlassFlowPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return AutoScrollPosition;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (BackColor.A > 0)
                using (var brush = new SolidBrush(BackColor)) e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }

    internal sealed class GlassPanel : Panel
    {
        public GlassPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bounds = ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            using (var brush = new SolidBrush(BackColor)) e.Graphics.FillRectangle(brush, bounds);
        }
    }

    internal sealed class GlassButton : Button
    {
        public bool Primary;
        public Func<Palette> Colors;
        private bool hover;

        public GlassButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            MouseEnter += delegate { hover = true; Invalidate(); };
            MouseLeave += delegate { hover = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = BackColor;
            if (!Enabled) fill = GlassPanelBlend(palette.Panel, palette.Background, 0.4f);
            else if (hover) fill = GlassPanelBlend(fill, Color.White, Primary ? 0.10f : 0.07f);
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), 6))
            using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
            if (!Primary)
            {
                using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), 6))
                using (var pen = new Pen(palette.Border)) e.Graphics.DrawPath(pen, path);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, bounds, Enabled ? ForeColor : palette.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private static Color GlassPanelBlend(Color first, Color second, float amount)
        {
            return Color.FromArgb(
                (int)(first.R + (second.R - first.R) * amount),
                (int)(first.G + (second.G - first.G) * amount),
                (int)(first.B + (second.B - first.B) * amount));
        }
    }
}
