using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeRawSectionEditor : UserControl
    {
        private readonly StudioConfiguration configuration;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly StudioChoice choice;
        private readonly StudioTextInput filter;
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
        private readonly List<string> sections;
        private readonly ToolTip tips = new ToolTip();
        private bool updating;

        public event EventHandler Changed;

        public NativeRawSectionEditor(StudioConfiguration configuration, float scale)
        {
            this.configuration = configuration;
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 3, BackColor = palette.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            top.Controls.Add(new Label { Text = Localization.IsEnglish ? "XML section" : "XML-Bereich", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            choice = new StudioChoice(scale) { Dock = DockStyle.Fill, Font = body, Colors = () => palette, AccessibleName = Localization.IsEnglish ? "XML section" : "XML-Bereich" };
            sections = configuration.Sections().OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase).ToList();
            choice.SetOptions(sections.Select(name => new ChoiceOption(name, name)), "general");
            choice.SelectedIndexChanged += delegate { if (!updating) Build(); };
            top.Controls.Add(choice, 1, 0);
            filter = new StudioTextInput(scale, body, palette) { Dock = DockStyle.Fill, Colors = () => palette, AccessibleName = Localization.IsEnglish ? "Filter sections" : "Bereiche filtern" };
            filter.Box.ForeColor = palette.Muted;
            filter.Box.Text = Localization.IsEnglish ? "Filter sections …" : "Bereiche filtern …";
            filter.Box.GotFocus += delegate { if (filter.Box.ForeColor == palette.Muted) { filter.Box.Text = string.Empty; filter.Box.ForeColor = palette.Text; RebuildChoices(); } };
            filter.Box.LostFocus += delegate { if (filter.Box.Text.Length == 0) { filter.Box.ForeColor = palette.Muted; filter.Box.Text = Localization.IsEnglish ? "Filter sections …" : "Bereiche filtern …"; RebuildChoices(); } };
            filter.Box.TextChanged += delegate { RebuildChoices(); };
            top.Controls.Add(filter, 2, 0);

            table.Padding = new Padding(0);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7));
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(36), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(3), 0, 0) };
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Add entry" : "Eintrag hinzufügen", delegate { AddEntry(); }, body));
            actions.Controls.Add(new Label { Text = Localization.IsEnglish ? "  × remove (confirmation) · ↺ reset to default · highlighted = differs from default" : "  × entfernen (Bestätigung) · ↺ auf Standard · farbig = weicht vom Standard ab", ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(Px(10), Px(8), 0, 0) });
            Controls.Add(new StudioScrollHost(scroll, delegate { return new Size(Math.Max(1, table.Width), Math.Max(1, table.Height)); }, () => palette, false, (Color?)palette.Background));
            Controls.Add(actions);
            Controls.Add(top);
            Build();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void RebuildChoices()
        {
            if (updating) return;
            string query = filter.Box.ForeColor == palette.Muted ? string.Empty : filter.Text.Trim();
            string selected = choice.SelectedValue;
            updating = true;
            try
            {
                var options = new List<ChoiceOption>();
                foreach (string name in sections)
                    if (query.Length == 0 || name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        options.Add(new ChoiceOption(name, name));
                choice.SetOptions(options, selected ?? String.Empty);
            }
            finally { updating = false; }
            Build();
        }

        private void LayoutTable()
        {
            table.Width = Math.Max(Px(300), scroll.ClientSize.Width - scroll.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
        }

        private void Build()
        {
            table.SuspendLayout();
            try
            {
                table.Controls.Clear();
                table.RowStyles.Clear();
                table.RowCount = 0;
                string section = choice.SelectedValue;
                if (section == null) return;
                foreach (var entry in configuration.Entries(section).ToArray())
                {
                    string key = entry.Key;
                    string value = entry.Value ?? "";
                    bool differs = DiffersFromDefault(section, key, value);
                    bool boolValue = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                        || String.Equals(value, "no", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
                    if (boolValue)
                    {
                        var check = new StudioCheck(scale) { Text = key, Checked = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), Font = Typography.Create(10f * scale, FontStyle.Regular), Colors = () => palette, Dock = DockStyle.Fill, Height = Px(30), Margin = new Padding(0, Px(1), 0, Px(1)), InkOverride = differs ? palette.Accent : Color.Empty };
                        check.CheckedChanged += delegate { configuration.SetBool(section, key, check.Checked); RaiseChanged(); };
                        table.Controls.Add(check, 0, table.RowCount);
                        table.SetColumnSpan(check, 2);
                        table.Controls.Add(DeleteButton(section, key), 2, table.RowCount);
                        table.Controls.Add(ResetButton(section, key), 3, table.RowCount);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                    else
                    {
                        var label = new Label { Text = key, Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = differs ? palette.Accent : palette.Text, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Margin = new Padding(0, 0, Px(12), 0) };
                        var text = new StudioTextInput(scale, Typography.Create(10f * scale, FontStyle.Regular), palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0) };
                        text.Text = value;
                        text.Box.ForeColor = differs ? palette.Accent : palette.Text;
                        text.Box.TextChanged += delegate { configuration.Set(section, key, text.Text); RaiseChanged(); };
                        table.Controls.Add(label, 0, table.RowCount);
                        table.Controls.Add(text, 1, table.RowCount);
                        table.Controls.Add(DeleteButton(section, key), 2, table.RowCount);
                        table.Controls.Add(ResetButton(section, key), 3, table.RowCount);
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        table.RowCount++;
                    }
                }
            }
            finally { table.ResumeLayout(true); }
            LayoutTable();
            scroll.AutoScrollPosition = Point.Empty;
        }

        private void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private bool DiffersFromDefault(string section, string key, string value)
        {
            string defaultValue;
            return MediaPortalDefaults.TryGet(section, key, out defaultValue) && !String.Equals(value ?? String.Empty, defaultValue, StringComparison.OrdinalIgnoreCase);
        }

        private Button ResetButton(string section, string key)
        {
            string defaultValue;
            bool hasDefault = MediaPortalDefaults.TryGet(section, key, out defaultValue);
            string target = hasDefault ? defaultValue : configuration.OriginalValue(section, key);
            var button = new StudioButton { Text = "↺", Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = hasDefault ? palette.Accent : palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(Px(4), Px(2), Px(2), Px(2)), Enabled = target != null };
            button.FlatAppearance.BorderColor = palette.Border;
            tips.SetToolTip(button, target == null
                ? (Localization.IsEnglish ? "No MediaPortal default is known for this entry." : "Für diesen Eintrag ist kein MediaPortal-Standardwert bekannt.")
                : (hasDefault ? (Localization.IsEnglish ? "Reset to MediaPortal default: " : "Auf MediaPortal-Standard zurücksetzen: ") : (Localization.IsEnglish ? "Restore last saved value: " : "Zuletzt gespeicherten Wert wiederherstellen: ")) + target);
            button.Click += delegate
            {
                if (target == null) return;
                configuration.Set(section, key, target);
                RaiseChanged();
                Build();
            };
            return button;
        }

        private Button DeleteButton(string section, string key)
        {
            var button = new StudioButton { Text = "×", Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(Px(4), Px(2), 0, Px(2)) };
            button.FlatAppearance.BorderColor = palette.Border;
            tips.SetToolTip(button, Localization.IsEnglish ? "Remove this entry from MediaPortal.xml" : "Diesen Eintrag aus MediaPortal.xml entfernen");
            button.Click += delegate
            {
                string message = Localization.IsEnglish
                    ? "Remove entry \"" + key + "\" from section \"" + section + "\"?\r\n\r\nThe entry is deleted when you save; a ZIP backup is created first."
                    : "Eintrag \"" + key + "\" aus dem Bereich \"" + section + "\" entfernen?\r\n\r\nDer Eintrag wird beim Speichern gelöscht; vorher wird eine ZIP-Sicherung erstellt.";
                if (MessageBox.Show(FindForm(), message, "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                configuration.Remove(section, key);
                RaiseChanged();
                Build();
            };
            return button;
        }

        private void AddEntry()
        {
            string section = choice.SelectedValue;
            if (section == null) return;
            using (var dialog = new EntryDialog(scale))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                string name = dialog.EntryName;
                if (configuration.Entries(section).Any(entry => String.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (MessageBox.Show(FindForm(), (Localization.IsEnglish ? "This entry already exists. Overwrite it?" : "Dieser Eintrag existiert bereits. Überschreiben?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                }
                configuration.Set(section, name, dialog.EntryValue);
                RaiseChanged();
                Build();
            }
        }

        private sealed class EntryDialog : Form
        {
            private readonly float scale;
            private readonly TextBox name = new TextBox();
            private readonly TextBox value = new TextBox();
            public string EntryName { get { return name.Text.Trim(); } }
            public string EntryValue { get { return value.Text; } }

            public EntryDialog(float scale)
            {
                this.scale = scale;
                Text = Localization.IsEnglish ? "Add entry" : "Eintrag hinzufügen";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(480), Px(220));
                var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(Px(16)), BackColor = Palette.For(true).Panel };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(110)));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                table.Controls.Add(new Label { Text = Localization.IsEnglish ? "Name" : "Name", ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
                name.Dock = DockStyle.Fill;
                name.Font = Typography.Create(10f * scale, FontStyle.Regular);
                name.BackColor = Palette.For(true).Input;
                name.ForeColor = Palette.For(true).Text;
                table.Controls.Add(name, 1, 0);
                table.Controls.Add(new Label { Text = Localization.IsEnglish ? "Value" : "Wert", ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
                value.Dock = DockStyle.Fill;
                value.Font = Typography.Create(10f * scale, FontStyle.Regular);
                value.BackColor = Palette.For(true).Input;
                value.ForeColor = Palette.For(true).Text;
                table.Controls.Add(value, 1, 1);
                var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Palette.For(true).Panel, Margin = new Padding(0, Px(8), 0, 0) };
                var cancel = new StudioButton { Text = Localization.IsEnglish ? "Cancel" : "Abbrechen", DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate
                {
                    if (name.Text.Trim().Length == 0)
                    {
                        MessageBox.Show(this, Localization.IsEnglish ? "Please enter a name." : "Bitte einen Namen eingeben.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(ok);
                table.Controls.Add(buttons, 0, 3);
                table.SetColumnSpan(buttons, 2);
                Controls.Add(table);
                AcceptButton = ok;
                CancelButton = cancel;
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }
        }
    }
}
