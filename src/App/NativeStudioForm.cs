using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        private readonly FlowLayoutPanel navigation = new SidebarPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        private Panel header, footer, sidebar;
        private StudioLogo logo;
        private FlowLayoutPanel footerActions;
        private Button saveButton, closeButton, backupButton;
        private readonly Label title = new Label { AutoSize = true };
        private readonly Label subtitle = new Label { AutoSize = true };
        private readonly Label status = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        private readonly Label pageTitle = new Label { AutoSize = true };
        private readonly Label pageLead = new Label { AutoSize = true };
        private readonly ComboBox sizeChoice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
        private TextBox searchBox;
        private NativeSection current;
        private float scale;
        private Font body, heading, small;
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
            appearance = new Appearance(preferences ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "appearance.xml"));
            appearance.NativeOriginal = true;
            scale = appearance.TextScale / 100f;
            body = Typography.Create(10f * scale, FontStyle.Regular);
            heading = Typography.Create(18f * scale, FontStyle.Regular);
            small = Typography.Create(9f * scale, FontStyle.Regular);
            Font = body;
            palette = Palette.For(true);
            BackColor = palette.Background;
            ForeColor = palette.Text;
            string configPath = StudioConfiguration.FindConfigPath(runtime);
            configuration = StudioConfiguration.Load(configPath);
            BuildChrome();
            ShowSection(FindSection(initialSection) ?? sections[0]);
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
            header = new Panel { Dock = DockStyle.Top, Height = Px(82), BackColor = palette.Background };
            logo = new StudioLogo { Colors = () => palette, Bounds = new Rectangle(Px(22), Px(20), Px(40), Px(40)), Font = Typography.Create(13f * scale, FontStyle.Bold) };
            header.Controls.Add(logo);
            title.Text = "MP1 / CONFIGURATION";
            title.Font = Typography.Create(16f * scale, FontStyle.Regular);
            title.ForeColor = palette.Text;
            title.Location = new Point(Px(76), Px(14));
            header.Controls.Add(title);
            subtitle.Text = Localization.IsEnglish ? "New interface · MediaPortal 1.39" : "Neue Oberfläche · MediaPortal 1.39";
            subtitle.Font = small;
            subtitle.ForeColor = palette.Muted;
            subtitle.Location = new Point(Px(78), Px(44));
            header.Controls.Add(subtitle);
            sizeChoice.Items.AddRange(new object[] { "100 %", "115 %", "125 %", "150 %", "175 %" });
            sizeChoice.SelectedIndex = Math.Max(0, Array.IndexOf(new[] { 100, 115, 125, 150, 175 }, appearance.TextScale));
            sizeChoice.AccessibleName = Localization.IsEnglish ? "Text size" : "Schriftgröße";
            sizeChoice.Font = body;
            sizeChoice.BackColor = palette.Input;
            sizeChoice.ForeColor = palette.Text;
            sizeChoice.Width = Px(110);
            sizeChoice.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sizeChoice.Location = new Point(ClientSize.Width - Px(140), Px(24));
            sizeChoice.SelectedIndexChanged += delegate { ChangeScale(); };
            header.Controls.Add(sizeChoice);
            searchBox = new TextBox { Font = body, BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, AccessibleName = Localization.IsEnglish ? "Search settings" : "Einstellungen durchsuchen" };
            searchBox.TextChanged += delegate { ApplySearch(); };
            searchBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) { searchBox.Clear(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter)
                {
                    StudioAction first = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Visible && item.Tag is NativeSection);
                    if (first != null) ShowSection((NativeSection)first.Tag, first);
                    e.SuppressKeyPress = true;
                }
            };
            header.Controls.Add(searchBox);
            header.Resize += delegate { sizeChoice.Left = header.ClientSize.Width - sizeChoice.Width - Px(24); };

            var bodyPanel = new Panel { Dock = DockStyle.Fill, BackColor = palette.Background };
            sidebar = new Panel { Dock = DockStyle.Left, Width = Px(230), BackColor = palette.Panel, Padding = new Padding(Px(10), Px(14), Px(8), Px(10)) };
            navigation.BackColor = palette.Panel;
            navigation.Padding = new Padding(0);
            sidebar.Controls.Add(navigation);
            foreach (NativeSection section in sections)
            {
                var item = new StudioAction { Text = Localization.IsEnglish ? section.TitleEn : section.TitleDe, IconName = section.Icon, Colors = () => palette, Navigation = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(0, 2, 0, 2), BackColor = palette.Panel, AccessibleName = section.TitleDe };
                item.Click += delegate { ShowSection(section, item); };
                item.Tag = section;
                navigation.Controls.Add(item);
            }
            var classic = new StudioAction { Text = Localization.IsEnglish ? "Classic / plugins" : "Klassisch / Plugins", IconName = "plugin", Colors = () => palette, Navigation = true, Secondary = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(0, 18, 0, 2), BackColor = palette.Panel };
            classic.Click += delegate { StartClassic(); };
            navigation.Controls.Add(classic);
            var legacy = new StudioAction { Text = Localization.IsEnglish ? "Studio 1.9 interface" : "Studio 1.9-Oberfläche", IconName = "surface", Colors = () => palette, Navigation = true, Secondary = true, Font = body, Height = Px(38), Width = Px(198), Margin = new Padding(0, 4, 0, 2), BackColor = palette.Panel };
            legacy.Click += delegate { StartStudioShell(); };
            navigation.Controls.Add(legacy);
            bodyPanel.Controls.Add(scroll);
            bodyPanel.Controls.Add(sidebar);

            scroll.BackColor = palette.Background;
            scroll.Padding = new Padding(Px(28), Px(18), Px(28), Px(18));
            table.BackColor = palette.Background;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(300)));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.Padding = new Padding(0);
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            Controls.Add(bodyPanel);

            footer = new Panel { Dock = DockStyle.Bottom, Height = Px(62), BackColor = palette.Background, Padding = new Padding(Px(24), Px(12), Px(20), Px(10)) };
            footer.Paint += delegate(object sender, PaintEventArgs e) { using (var pen = new Pen(palette.Border)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0); };
            status.Text = Localization.IsEnglish ? "Ready" : "Bereit";
            status.Font = small;
            status.ForeColor = palette.Muted;
            footer.Controls.Add(status);
            footerActions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Width = Px(360) };
            closeButton = new Button { Text = Localization.IsEnglish ? "Close" : "Schließen", Width = Px(110), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            closeButton.FlatAppearance.BorderColor = palette.Border;
            closeButton.Click += delegate { Close(); };
            footerActions.Controls.Add(closeButton);
            saveButton = new Button { Text = Localization.IsEnglish ? "Save" : "Speichern", Width = Px(120), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Primary, ForeColor = palette.PrimaryText, Font = body, Margin = new Padding(0) };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += delegate { SaveConfiguration(); };
            footerActions.Controls.Add(saveButton);
            backupButton = new Button { Text = Localization.IsEnglish ? "Backup" : "Sicherung", Width = Px(110), Height = Px(34), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Font = body, Margin = new Padding(Px(8), 0, 0, 0) };
            backupButton.FlatAppearance.BorderColor = palette.Border;
            backupButton.Click += delegate { CreateBackup(); };
            footerActions.Controls.Add(backupButton);
            footer.Controls.Add(footerActions);
            Controls.Add(footer);
            Controls.Add(header);
            ApplyMetrics();
        }

        private void ApplyMetrics()
        {
            header.Height = Px(82);
            logo.Bounds = new Rectangle(Px(22), Px(20), Px(40), Px(40));
            logo.Font = Typography.Create(13f * scale, FontStyle.Bold);
            title.Font = Typography.Create(16f * scale, FontStyle.Regular);
            title.Location = new Point(Px(76), Px(14));
            subtitle.Font = small;
            subtitle.Location = new Point(Px(78), Px(44));
            sizeChoice.Font = body;
            sizeChoice.Width = Px(110);
            sizeChoice.Left = header.ClientSize.Width - sizeChoice.Width - Px(24);
            sizeChoice.Top = Px(24);
            searchBox.Font = body;
            int titleRight = title.Left + TextRenderer.MeasureText(title.Text, title.Font).Width;
            int searchLeft = titleRight + Px(24);
            int searchRight = sizeChoice.Left - Px(12);
            int searchWidth = Math.Min(Px(280), Math.Max(Px(90), searchRight - searchLeft));
            searchBox.Bounds = new Rectangle(searchLeft, Px(24), searchWidth, Px(28));
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
            footerActions.Width = Px(360);
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
            table.Width = Math.Max(Px(300), scroll.ClientSize.Width - scroll.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
            float labelWidth = Math.Max(Px(220), Math.Min(Px(320), table.Width * 0.4f));
            if (table.ColumnStyles.Count > 0) table.ColumnStyles[0].Width = (int)Math.Round(labelWidth);
            table.PerformLayout();
            if (scroll.HorizontalScroll.Visible) scroll.AutoScrollPosition = Point.Empty;
        }

        private void ShowSection(NativeSection section, StudioAction selected = null)
        {
            current = section;
            table.SuspendLayout();
            try
            {
                table.Controls.Clear();
                table.RowStyles.Clear();
                table.RowCount = 0;
                pageTitle.Text = Localization.IsEnglish ? section.TitleEn : section.TitleDe;
                pageTitle.Font = heading;
                pageTitle.ForeColor = palette.Text;
                pageTitle.Margin = new Padding(0, 0, 0, Px(2));
                table.Controls.Add(pageTitle, 0, table.RowCount);
                table.SetColumnSpan(pageTitle, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                pageLead.Text = Localization.IsEnglish ? section.SubtitleEn : section.SubtitleDe;
                pageLead.Font = small;
                pageLead.ForeColor = palette.Muted;
                pageLead.Margin = new Padding(0, 0, 0, Px(18));
                table.Controls.Add(pageLead, 0, table.RowCount);
                table.SetColumnSpan(pageLead, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                foreach (NativeGroup group in section.Groups) AddGroup(group);
            }
            finally { table.ResumeLayout(true); }
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>()) item.Selected = Object.ReferenceEquals(item, selected) || (selected == null && item.Tag == section);
            navigation.Invalidate();
            if (selected == null)
            {
                StudioAction target = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => Object.ReferenceEquals(item.Tag, section));
                if (target != null && (target.Bounds.Top < 0 || target.Bounds.Bottom > navigation.ClientSize.Height))
                    navigation.AutoScrollPosition = new Point(0, Math.Max(0, target.Bounds.Top));
            }
            LayoutTable();
            scroll.AutoScrollPosition = Point.Empty;
        }

        private void AddGroup(NativeGroup group)
        {
            var groupTitle = new Label { Text = Localization.IsEnglish ? group.TitleEn : group.TitleDe, Font = Typography.Create(12f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, UseMnemonic = false, Margin = new Padding(0, Px(8), 0, Px(8)) };
            table.Controls.Add(groupTitle, 0, table.RowCount);
            table.SetColumnSpan(groupTitle, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
            foreach (NativeField field in group.Fields) AddField(field);
            var spacer = new Panel { Height = Px(10), Dock = DockStyle.Fill, BackColor = palette.Background };
            table.Controls.Add(spacer, 0, table.RowCount);
            table.SetColumnSpan(spacer, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private void AddField(NativeField field)
        {
            if (field.Kind == NativeFieldKind.DynamicBools)
            {
                int count = 0;
                foreach (var entry in configuration.Entries(field.Section).ToArray())
                {
                    if (String.IsNullOrEmpty(entry.Key)) continue;
                    string key = entry.Key;
                    var check = new CheckBox { Text = key, Checked = configuration.GetBool(field.Section, key, false), AutoSize = true, Font = body, ForeColor = palette.Text, BackColor = palette.Background, FlatStyle = FlatStyle.Flat, UseMnemonic = false, Margin = new Padding(0, Px(2), 0, Px(2)) };
                    check.CheckedChanged += delegate { configuration.SetBool(field.Section, key, check.Checked); MarkDirty(); };
                    table.Controls.Add(check, 0, table.RowCount);
                    table.SetColumnSpan(check, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                    count++;
                }
                if (count == 0)
                {
                    var empty = new Label { Text = Localization.IsEnglish ? "No entries found in the configuration." : "Keine Einträge in der Konfiguration gefunden.", Font = small, ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(2), 0, Px(10)) };
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
                var info = new Label { Text = Localization.IsEnglish ? field.HelpEn : field.HelpDe, Font = small, ForeColor = palette.Muted, AutoSize = true, MaximumSize = new Size(Px(760), 0), Margin = new Padding(0, Px(2), 0, Px(10)) };
                table.Controls.Add(info, 0, table.RowCount);
                table.SetColumnSpan(info, 2);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount++;
                return;
            }
            string labelText = Localization.IsEnglish ? field.LabelEn : field.LabelDe;
            if (field.Kind == NativeFieldKind.Bool)
            {
                var check = new CheckBox { Text = labelText, Checked = configuration.GetBool(field.Section, field.Entry, false), AutoSize = true, Font = body, ForeColor = palette.Text, BackColor = palette.Background, FlatStyle = FlatStyle.Flat, UseMnemonic = false, Margin = new Padding(0, Px(2), 0, Px(2)) };
                check.CheckedChanged += delegate { configuration.SetBool(field.Section, field.Entry, check.Checked); MarkDirty(); };
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
            if (field.Kind == NativeFieldKind.Combo)
            {
                var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = body, BackColor = palette.Input, ForeColor = palette.Text, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0) };
                foreach (NativeOption option in field.Options) combo.Items.Add(Localization.IsEnglish ? option.LabelEn : option.LabelDe);
                string value = configuration.Get(field.Section, field.Entry, field.Options.Count > 0 ? field.Options[0].Value : "");
                int index = field.Options.FindIndex(option => String.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase));
                combo.SelectedIndex = index >= 0 ? index : 0;
                combo.SelectedIndexChanged += delegate { if (combo.SelectedIndex >= 0) { configuration.Set(field.Section, field.Entry, field.Options[combo.SelectedIndex].Value); MarkDirty(); } };
                input = combo;
            }
            else if (field.Kind == NativeFieldKind.Integer)
            {
                var text = new TextBox { Text = configuration.GetInt(field.Section, field.Entry, field.Minimum).ToString(), Font = body, BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0) };
                text.Validated += delegate { int value; if (!Int32.TryParse(text.Text, out value)) value = field.Minimum; value = Math.Max(field.Minimum, Math.Min(field.Maximum, value)); text.Text = value.ToString(); configuration.SetInt(field.Section, field.Entry, value); MarkDirty(); };
                input = text;
            }
            else if (field.Kind == NativeFieldKind.Folder)
            {
                var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0), Height = Px(30) };
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(34)));
                var folder = new TextBox { Text = configuration.Get(field.Section, field.Entry, ""), Font = body, BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0) };
                folder.TextChanged += delegate { configuration.Set(field.Section, field.Entry, folder.Text); MarkDirty(); };
                var browse = new Button { Text = "…", Font = body, FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(Px(4), 0, 0, 0) };
                browse.FlatAppearance.BorderColor = palette.Border;
                browse.Click += delegate
                {
                    using (var picker = new FolderBrowserDialog())
                    {
                        picker.Description = Localization.IsEnglish ? "Select a folder" : "Ordner auswählen";
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
                var text = new TextBox { Text = configuration.Get(field.Section, field.Entry, ""), Font = body, BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0) };
                text.TextChanged += delegate { configuration.Set(field.Section, field.Entry, text.Text); MarkDirty(); };
                input = text;
            }
            table.Controls.Add(input, 1, table.RowCount);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
            AddHelp(field);
        }

        private void AddHelp(NativeField field)
        {
            string help = Localization.IsEnglish ? field.HelpEn : field.HelpDe;
            if (String.IsNullOrEmpty(help)) return;
            var label = new Label { Text = help, Font = small, ForeColor = palette.Muted, AutoSize = true, MaximumSize = new Size(Px(740), 0), Margin = new Padding(0, Px(1), 0, Px(10)) };
            table.Controls.Add(label, 0, table.RowCount);
            table.SetColumnSpan(label, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private void MarkDirty()
        {
            dirty = true;
            status.Text = Localization.IsEnglish ? "Unsaved changes" : "Ungespeicherte Änderungen";
            status.ForeColor = palette.Accent;
        }

        private void SaveConfiguration()
        {
            try
            {
                string backup = configuration.Save();
                dirty = false;
                appearance.Save();
                status.Text = String.IsNullOrEmpty(backup)
                    ? (Localization.IsEnglish ? "No changes to save" : "Keine Änderungen zu speichern")
                    : (Localization.IsEnglish ? "Saved · backup: " : "Gespeichert · Sicherung: ") + System.IO.Path.GetFileName(backup);
                status.ForeColor = palette.Muted;
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Native configuration save failed");
                MessageBox.Show(this, (Localization.IsEnglish ? "The configuration could not be saved.\r\n\r\n" : "Die Konfiguration konnte nicht gespeichert werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateBackup()
        {
            try
            {
                string folder = ConfigurationBackup.DefaultBackupDirectory;
                Directory.CreateDirectory(folder);
                string backup = System.IO.Path.Combine(folder, "native-manual-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".zip");
                ConfigurationBackup.Create(configuration.Path, backup);
                status.Text = (Localization.IsEnglish ? "Backup created · " : "Sicherung erstellt · ") + System.IO.Path.GetFileName(backup);
                status.ForeColor = palette.Muted;
                StudioLog.Info("Native manual backup: " + backup);
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Native manual backup failed");
                MessageBox.Show(this, (Localization.IsEnglish ? "The backup could not be created.\r\n\r\n" : "Die Sicherung konnte nicht erstellt werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            Font = body;
            ApplyMetrics();
            ShowSection(current);
        }

        private void StartStudioShell()
        {
            if (dirty)
            {
                if (MessageBox.Show(this, Localization.IsEnglish ? "Discard unsaved changes and open the 1.9 interface?" : "Ungespeicherte Änderungen verwerfen und die 1.9-Oberfläche öffnen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            }
            suppressClosePrompt = true;
            StartStudioRequested = true;
            Close();
        }

        private void StartClassic()
        {
            if (dirty)
            {
                if (MessageBox.Show(this, Localization.IsEnglish ? "Discard unsaved changes and open the classic interface?" : "Ungespeicherte Änderungen verwerfen und die klassische Oberfläche öffnen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            }
            suppressClosePrompt = true;
            string executable = Path.Combine(runtime, "Configuration.exe");
            Program.StartNative(executable, runtime);
            Close();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (!dirty || suppressClosePrompt) return;
            if (MessageBox.Show(this, Localization.IsEnglish ? "Discard unsaved changes?" : "Ungespeicherte Änderungen verwerfen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) e.Cancel = true;
        }

        private int Px(float value)
        {
            return (int)Math.Round(value * scale);
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
                return true;
            }
            if (Inside(target, scroll))
            {
                ScrollBy(scroll, delta, Px(48));
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
            NativeDarkMode.ApplyWindow(Handle, true);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            NativeDarkMode.ApplyWindow(Handle, true);
            NativeDarkMode.Apply(navigation.Handle);
            NativeDarkMode.Apply(scroll.Handle);
            EnsureNavigationVisible();
            ApplyCueBanner();
        }

        private void EnsureNavigationVisible()
        {
            StudioAction selected = navigation.Controls.OfType<StudioAction>().FirstOrDefault(item => item.Selected);
            if (selected == null) return;
            if (selected.Bounds.Top < 0 || selected.Bounds.Bottom > navigation.ClientSize.Height)
                navigation.AutoScrollPosition = new Point(0, Math.Max(0, selected.Bounds.Top));
        }

        private void ApplySearch()
        {
            string query = searchBox.Text.Trim();
            bool active = query.Length > 0;
            int matches = 0;
            foreach (StudioAction item in navigation.Controls.OfType<StudioAction>())
            {
                NativeSection section = item.Tag as NativeSection;
                bool visible = true;
                if (active && section != null)
                {
                    visible = MatchesSection(section, query);
                    if (visible) matches++;
                }
                item.Visible = visible;
            }
            navigation.AutoScrollPosition = Point.Empty;
            if (active)
            {
                status.Text = matches == 0
                    ? (Localization.IsEnglish ? "No matches" : "Keine Treffer")
                    : (Localization.IsEnglish ? "Search: " : "Suche: ") + matches + (Localization.IsEnglish ? (matches == 1 ? " page" : " pages") : (matches == 1 ? " Seite" : " Seiten"));
                status.ForeColor = palette.Muted;
            }
            else
            {
                status.Text = dirty ? (Localization.IsEnglish ? "Unsaved changes" : "Ungespeicherte Änderungen") : (Localization.IsEnglish ? "Ready" : "Bereit");
                status.ForeColor = dirty ? palette.Accent : palette.Muted;
            }
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
            try { SendMessage(searchBox.Handle, 0x1501, new IntPtr(1), Localization.IsEnglish ? "Search settings…" : "Einstellungen durchsuchen…"); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveConfiguration();
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

    internal sealed class SidebarPanel : FlowLayoutPanel
    {
        protected override Point ScrollToControl(Control activeControl)
        {
            return AutoScrollPosition;
        }
    }
}
