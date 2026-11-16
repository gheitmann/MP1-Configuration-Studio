using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeKeymapEditor : UserControl, IStudioFileEditor
    {
        private sealed class Context
        {
            public string Label;
            public XmlElement Element;
        }

        private readonly string path;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly StudioChoice contextChoice;
        private readonly StudioTextInput search;
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = false, ColumnCount = 5 };
        private readonly Label status = new Label();
        private readonly List<Context> contexts = new List<Context>();
        private XmlDocument document;
        private Encoding encoding = new UTF8Encoding(false);
        private bool dirty;
        private bool building;

        public NativeKeymapEditor(string configDirectory, float scale)
        {
            this.path = Path.Combine(configDirectory, "keymap.xml");
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 4, BackColor = palette.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            top.Controls.Add(new Label { Text = Localization.T("Bereich"), ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            contextChoice = new StudioChoice(scale) { Dock = DockStyle.Fill, Font = body, Colors = () => palette, AccessibleName = Localization.T("Bereich") };
            contextChoice.SelectedIndexChanged += delegate { if (!building) BuildTable(); };
            top.Controls.Add(contextChoice, 1, 0);
            search = new StudioTextInput(scale, body, palette) { Dock = DockStyle.Fill, Colors = () => palette, AccessibleName = Localization.T("Aktionen filtern") };
            search.Box.ForeColor = palette.Muted;
            search.Box.Text = Localization.T("Aktionen filtern …");
            search.Box.GotFocus += delegate { if (search.Box.ForeColor == palette.Muted) { search.Box.Text = string.Empty; search.Box.ForeColor = palette.Text; } };
            search.Box.LostFocus += delegate { if (search.Box.Text.Length == 0) { search.Box.ForeColor = palette.Muted; search.Box.Text = Localization.T("Aktionen filtern …"); } };
            search.Box.TextChanged += delegate { if (!building) BuildTable(); };
            top.Controls.Add(search, 2, 0);
            var save = ButtonFor(Localization.T("Speichern"), delegate { SaveFile(); }, body);
            save.Dock = DockStyle.Fill;
            top.Controls.Add(save, 3, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            var reload = ButtonFor(Localization.T("Neu laden"), delegate { Reload(); }, body);
            var add = ButtonFor(Localization.T("Aktion hinzufügen"), delegate { AddAction(); }, body);
            actions.Controls.Add(reload);
            actions.Controls.Add(add);
            actions.Controls.Add(ButtonFor(Localization.T("Fenster hinzufügen"), delegate { AddWindow(); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("Fenster löschen"), delegate { DeleteWindow(); }, body));
            status.AutoSize = false;
            status.Width = Px(260);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            actions.Controls.Add(status);

            table.Padding = new Padding(0);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            Controls.Add(new StudioScrollHost(scroll, delegate { return new Size(Math.Max(1, table.Width), Math.Max(1, table.Height)); }, () => palette, false, (Color?)palette.Background));
            Controls.Add(actions);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.T("Ungespeicherte Tastenbelegung verwerfen?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public bool SaveChanges()
        {
            SaveFile();
            return !dirty;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(80), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LayoutTable()
        {
            int width = Math.Max(Px(420), scroll.ClientSize.Width - scroll.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
            table.Width = width;
            table.PerformLayout();
            table.Height = Math.Max(Px(40), table.GetPreferredSize(new Size(width, 0)).Height);
        }

        private void LoadFile()
        {
            try
            {
                if (!File.Exists(path))
                {
                    status.Text = (Localization.T("Datei nicht gefunden: ")) + path;
                    return;
                }
                string text = NativeXmlFileStore.ReadText(path, out encoding);
                document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                document.LoadXml(text);
                BuildContexts();
                dirty = false;
                BuildTable();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

        private void Reload()
        {
            if (!ConfirmDiscardChanges()) return;
            LoadFile();
        }

        private void BuildContexts()
        {
            string previous = contextChoice.SelectedValue;
            contexts.Clear();
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var element = node as XmlElement;
                if (element == null) continue;
                if (String.Equals(element.Name, "global", StringComparison.OrdinalIgnoreCase))
                    contexts.Add(new Context { Label = Localization.T("Global"), Element = element });
                else if (String.Equals(element.Name, "window", StringComparison.OrdinalIgnoreCase))
                {
                    string description = ChildText(element, "description");
                    string id = ChildText(element, "id");
                    contexts.Add(new Context { Label = (String.IsNullOrEmpty(description) ? (Localization.T("Fenster")) : description) + (String.IsNullOrEmpty(id) ? "" : "  ·  #" + id), Element = element });
                }
            }
            building = true;
            var labels = new List<ChoiceOption>();
            foreach (Context context in contexts) labels.Add(new ChoiceOption(context.Label, context.Label));
            contextChoice.SetOptions(labels, previous ?? string.Empty);
            building = false;
        }

        private void AddWindow()
        {
            using (var dialog = new WindowDialog(scale))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                XmlElement window = document.CreateElement("window");
                AppendText(window, "id", dialog.WindowId);
                AppendText(window, "description", dialog.Description);
                document.DocumentElement.AppendChild(window);
                dirty = true;
                BuildContexts();
                contextChoice.SelectedIndex = contexts.Count - 1;
                BuildTable();
                status.Text = (Localization.T("Fenster hinzugefügt · ungespeichert"));
            }
        }

        private void DeleteWindow()
        {
            Context context = SelectedContext();
            if (context == null || context.Element == null || !String.Equals(context.Element.Name, "window", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(FindForm(), Localization.T("Nur Fenster-Bereiche können gelöscht werden; Global ist erforderlich."), "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(FindForm(), (Localization.T("Fenster „")) + context.Label + (Localization.T("“ und alle Zuordnungen löschen?")), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            context.Element.ParentNode.RemoveChild(context.Element);
            dirty = true;
            BuildContexts();
            BuildTable();
            status.Text = (Localization.T("Fenster gelöscht · ungespeichert"));
        }

        private void BuildTable()
        {
            if (document == null) return;
            int selectedIndex = contextChoice.SelectedIndex;
            Context context = selectedIndex >= 0 && selectedIndex < contexts.Count ? contexts[selectedIndex] : contexts.FirstOrDefault();
            table.SuspendLayout();
            try
            {
                table.Controls.Clear();
                table.RowStyles.Clear();
                table.RowCount = 0;
                if (context == null) return;
                string query = search.ForeColor == palette.Muted ? string.Empty : search.Text.Trim();
                AddHeader();
                int visible = 0;
                foreach (XmlNode node in context.Element.ChildNodes)
                {
                    var action = node as XmlElement;
                    if (action == null || !String.Equals(action.Name, "action", StringComparison.OrdinalIgnoreCase)) continue;
                    string description = ChildText(action, "description");
                    string id = ChildText(action, "id");
                    string key = ChildText(action, "key");
                    if (query.Length > 0
                        && (description ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0
                        && (key ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0
                        && (id ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    AddActionRow(action, description, id, key);
                    visible++;
                }
                if (visible == 0)
                {
                    var empty = new Label { Text = Localization.T("Keine Aktionen in diesem Bereich."), Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(6), 0, Px(6)) };
                    table.Controls.Add(empty, 0, table.RowCount);
                    table.SetColumnSpan(empty, 5);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                }
                status.Text = context.Label + "  ·  " + visible + (Localization.T(" Aktionen")) + (dirty ? (Localization.T("  ·  ungespeichert")) : "");
            }
            finally { table.ResumeLayout(true); }
            LayoutTable();
        }

        private void AddHeader()
        {
            string[] titles = new[] { Localization.T2("Aktion","Action"), Localization.T2("Taste","Key"), "", Localization.T2("Sound","Sound"), "" };
            for (int i = 0; i < titles.Length; i++)
            {
                var label = new Label { Text = titles[i], Font = Typography.Create(9f * scale, FontStyle.Bold), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, 0, 0, Px(4)) };
                table.Controls.Add(label, i, table.RowCount);
            }
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private void AddActionRow(XmlElement action, string description, string id, string key)
        {
            var info = new Label
            {
                Text = (String.IsNullOrEmpty(description) ? (Localization.T("(ohne Beschreibung)")) : description) + "\n#" + id,
                Font = Typography.Create(10f * scale, FontStyle.Regular),
                ForeColor = palette.Text,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Height = Px(42),
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false,
                Margin = new Padding(0, Px(2), Px(8), Px(2))
            };
            table.Controls.Add(info, 0, table.RowCount);

            var keyButton = new StudioButton { Text = String.IsNullOrEmpty(key) ? (Localization.T("(leer)")) : key, Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), Px(4), Px(2)) };
            keyButton.FlatAppearance.BorderColor = palette.Border;
            keyButton.Click += delegate { CaptureKey(action, keyButton); };
            table.Controls.Add(keyButton, 1, table.RowCount);

            var clear = new StudioButton { Text = Localization.T("Leer"), Font = Typography.Create(8f * scale, FontStyle.Regular), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), Px(4), Px(2)) };
            clear.FlatAppearance.BorderColor = palette.Border;
            clear.Click += delegate { SetActionValue(action, "key", string.Empty); keyButton.Text = Localization.T("(leer)"); MarkDirty(); BuildTable(); };
            table.Controls.Add(clear, 2, table.RowCount);

            var sound = new StudioComboInput(scale, Typography.Create(9.5f * scale, FontStyle.Regular), palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), Px(4), Px(2)) };
            foreach (string name in new[] { "", "click.wav", "back.wav", "cursor.wav", "photo.wav" })
                sound.Items.Add(name);
            string soundValue = ChildText(action, "sound") ?? string.Empty;
            if (!sound.Items.Contains(soundValue)) sound.Items.Add(soundValue);
            sound.Text = soundValue;
            sound.SelectedIndexChanged += delegate { SetActionValue(action, "sound", sound.Text); MarkDirty(); };
            sound.Leave += delegate { SetActionValue(action, "sound", sound.Text); MarkDirty(); };
            table.Controls.Add(sound, 3, table.RowCount);

            var remove = new StudioButton { Text = Localization.T("Löschen"), Font = Typography.Create(8f * scale, FontStyle.Regular), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), 0, Px(2)) };
            remove.FlatAppearance.BorderColor = palette.Border;
            remove.Click += delegate { RemoveAction(action); };
            table.Controls.Add(remove, 4, table.RowCount);

            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private void AddAction()
        {
            Context context = SelectedContext();
            if (context == null || document == null) return;
            using (var dialog = new ActionDialog(scale, NextActionId(context)))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                string key = dialog.ActionKey;
                var conflicts = DuplicateActions(context.Element, key, null);
                if (conflicts.Count > 0)
                {
                    if (!ConfirmResolveKey(key, conflicts)) return;
                    foreach (XmlElement conflict in conflicts) ClearActionKey(conflict);
                }
                XmlElement action = document.CreateElement("action");
                AppendText(action, "id", dialog.ActionId);
                AppendText(action, "key", key);
                AppendText(action, "sound", dialog.ActionSound);
                AppendText(action, "description", dialog.ActionDescription);
                context.Element.AppendChild(action);
                dirty = true;
                BuildTable();
                status.Text = conflicts.Count > 0
                    ? (Localization.T("Aktion hinzugefügt · Konflikt aufgelöst"))
                    : (Localization.T("Aktion hinzugefügt · ungespeichert"));
            }
        }

        private void RemoveAction(XmlElement action)
        {
            if (MessageBox.Show(FindForm(), (Localization.T("Diese Aktion löschen?")), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            action.ParentNode.RemoveChild(action);
            dirty = true;
            BuildTable();
            status.Text = (Localization.T("Aktion gelöscht · ungespeichert"));
        }

        private Context SelectedContext()
        {
            int index = contextChoice.SelectedIndex;
            return index >= 0 && index < contexts.Count ? contexts[index] : contexts.FirstOrDefault();
        }

        private string NextActionId(Context context)
        {
            int maximum = 0;
            foreach (XmlNode node in context.Element.ChildNodes)
            {
                var action = node as XmlElement;
                if (action == null) continue;
                int value;
                if (Int32.TryParse(ChildText(action, "id"), out value) && value > maximum) maximum = value;
            }
            return (maximum + 1).ToString();
        }

        private static void AppendText(XmlElement parent, string name, string value)
        {
            XmlElement element = parent.OwnerDocument.CreateElement(name);
            element.InnerText = value ?? string.Empty;
            parent.AppendChild(element);
        }

        private void CaptureKey(XmlElement action, Button button)
        {
            using (var dialog = new KeyCaptureDialog(scale, ChildText(action, "key")))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                string key = dialog.CapturedKey;
                string previous = ChildText(action, "key");
                Context context = contexts.FirstOrDefault(item => item.Element == action.ParentNode);
                var conflicts = context == null ? new List<XmlElement>() : DuplicateActions(context.Element, key, action);
                if (conflicts.Count > 0)
                {
                    if (!ConfirmResolveKey(key, conflicts))
                    {
                        BuildTable();
                        status.Text = Localization.T("Tastenkonflikt unverändert gelassen.");
                        return;
                    }
                    foreach (XmlElement conflict in conflicts) ClearActionKey(conflict);
                }
                SetActionValue(action, "key", key);
                button.Text = String.IsNullOrEmpty(key) ? (Localization.T("(leer)")) : key;
                MarkDirty();
                BuildTable();
                if (conflicts.Count > 0)
                    status.Text = (Localization.T("Taste ")) + key + (Localization.T(" zugewiesen · Konflikt aufgelöst"));
                else if (!String.Equals(previous, key, StringComparison.OrdinalIgnoreCase))
                    status.Text = (Localization.T("Taste ")) + key + (Localization.T(" zugewiesen · ungespeichert"));
            }
        }

        private bool ConfirmResolveKey(string key, List<XmlElement> conflicts)
        {
            var names = new List<string>();
            int shown = Math.Min(conflicts.Count, 5);
            for (int i = 0; i < shown; i++)
            {
                string description = ChildText(conflicts[i], "description");
                string id = ChildText(conflicts[i], "id");
                names.Add("• " + (String.IsNullOrWhiteSpace(description) ? (Localization.T("(ohne Beschreibung)")) : description) + (String.IsNullOrWhiteSpace(id) ? "" : " (#" + id + ")"));
            }
            if (conflicts.Count > shown) names.Add("…");
            string message = (Localization.T("Die Taste ")) + key + (Localization.T(" ist bereits belegt durch:\r\n\r\n"))
                + String.Join("\r\n", names.ToArray())
                + (Localization.T("\r\n\r\nTaste bei diesen Aktionen entfernen und hier zuweisen?"));
            return MessageBox.Show(FindForm(), message, "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        internal static List<XmlElement> DuplicateActions(XmlElement context, string key, XmlElement except)
        {
            var result = new List<XmlElement>();
            if (context == null || String.IsNullOrWhiteSpace(key)) return result;
            foreach (XmlNode node in context.ChildNodes)
            {
                var action = node as XmlElement;
                if (action == null || action == except) continue;
                if (String.Equals(ChildText(action, "key"), key, StringComparison.OrdinalIgnoreCase)) result.Add(action);
            }
            return result;
        }

        internal static void ClearActionKey(XmlElement action)
        {
            if (action == null) return;
            SetActionValue(action, "key", string.Empty);
        }

        private static string ChildText(XmlElement parent, string name)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                var element = node as XmlElement;
                if (element == null) continue;
                if (String.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase)) return element.InnerText;
            }
            return string.Empty;
        }

        private static void SetActionValue(XmlElement action, string name, string value)
        {
            XmlElement target = null;
            foreach (XmlNode node in action.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase)) { target = element; break; }
            }
            if (target == null)
            {
                target = action.OwnerDocument.CreateElement(name);
                XmlElement sound = null;
                foreach (XmlNode node in action.ChildNodes)
                {
                    var element = node as XmlElement;
                    if (element != null && String.Equals(element.Name, "sound", StringComparison.OrdinalIgnoreCase)) { sound = element; break; }
                }
                if (sound != null) action.InsertBefore(target, sound);
                else action.AppendChild(target);
            }
            target.InnerText = value ?? string.Empty;
        }

        private void MarkDirty()
        {
            dirty = true;
            if (status.Text.IndexOf(Localization.T("ungespeichert"), StringComparison.OrdinalIgnoreCase) < 0)
                status.Text += Localization.T("  ·  ungespeichert");
        }

        private void SaveFile()
        {
            if (document == null) return;
            try
            {
                string backup = NativeXmlFileStore.SaveDocument(path, document, encoding);
                dirty = false;
                status.Text = (Localization.T("Gespeichert · Sicherung: ")) + Path.GetFileName(backup);
                BuildTable();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.T("Die Tastenbelegung konnte nicht gespeichert werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private sealed class WindowDialog : Form
        {
            private readonly float scale;
            private readonly TextBox id = new TextBox();
            private readonly TextBox description = new TextBox();
            public string WindowId { get { return id.Text.Trim(); } }
            public string Description { get { return description.Text.Trim(); } }

            public WindowDialog(float scale)
            {
                this.scale = scale;
                Text = Localization.T("Fenster hinzufügen");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(480), Px(220));
                var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(Px(16)), BackColor = Palette.For(true).Panel };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                table.Controls.Add(new Label { Text = Localization.T("Fenster-ID"), ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
                id.Dock = DockStyle.Fill;
                id.Font = Typography.Create(10f * scale, FontStyle.Regular);
                id.BackColor = Palette.For(true).Input;
                id.ForeColor = Palette.For(true).Text;
                table.Controls.Add(id, 1, 0);
                table.Controls.Add(new Label { Text = Localization.T("Beschreibung"), ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
                description.Dock = DockStyle.Fill;
                description.Font = Typography.Create(10f * scale, FontStyle.Regular);
                description.BackColor = Palette.For(true).Input;
                description.ForeColor = Palette.For(true).Text;
                table.Controls.Add(description, 1, 1);
                var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Palette.For(true).Panel, Margin = new Padding(0, Px(8), 0, 0) };
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate
                {
                    int value;
                    if (!Int32.TryParse(id.Text.Trim(), out value) || value <= 0)
                    {
                        MessageBox.Show(this, Localization.T("Die Fenster-ID muss eine positive Zahl sein."), "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(description.Text))
                    {
                        MessageBox.Show(this, Localization.T("Bitte eine Beschreibung eingeben."), "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private sealed class ActionDialog : Form
        {
            private readonly float scale;
            private readonly TextBox id = new TextBox();
            private readonly TextBox description = new TextBox();
            private readonly TextBox key = new TextBox();
            private readonly StudioComboInput sound;
            public string ActionId { get { return id.Text.Trim(); } }
            public string ActionDescription { get { return description.Text.Trim(); } }
            public string ActionKey { get { return key.Text.Trim(); } }
            public string ActionSound { get { return sound.Text.Trim(); } }

            public ActionDialog(float scale, string suggestedId)
            {
                this.scale = scale;
                Text = Localization.T("Aktion hinzufügen");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(500), Px(300));

                sound = new StudioComboInput(scale, Typography.Create(10f * scale, FontStyle.Regular), Palette.For(true)) { Colors = () => Palette.For(true), Dock = DockStyle.Fill };
                foreach (string name in new[] { "", "click.wav", "back.wav", "cursor.wav", "photo.wav" }) sound.Items.Add(name);
                sound.Text = "click.wav";

                var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 5, Padding = new Padding(Px(16)), BackColor = Palette.For(true).Panel };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(110)));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(110)));
                AddRow(table, 0, Localization.T("Aktions-ID"), id);
                id.Text = suggestedId ?? string.Empty;
                AddRow(table, 1, Localization.T("Beschreibung"), description);
                AddRow(table, 2, Localization.T("Taste"), key, false);
                var capture = new StudioButton { Text = Localization.T("Aufnehmen …"), FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Dock = DockStyle.Fill };
                capture.FlatAppearance.BorderColor = Palette.For(true).Border;
                capture.Click += delegate
                {
                    using (var dialog = new KeyCaptureDialog(this.scale, key.Text))
                        if (dialog.ShowDialog(this) == DialogResult.OK) key.Text = dialog.CapturedKey;
                };
                table.Controls.Add(capture, 2, 2);
                AddRow(table, 3, "Sound", sound);

                var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Palette.For(true).Panel, Margin = new Padding(0, Px(8), 0, 0) };
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate
                {
                    int value;
                    if (!Int32.TryParse(id.Text.Trim(), out value) || value < 0)
                    {
                        MessageBox.Show(this, Localization.T("Die Aktions-ID muss eine nicht negative Zahl sein."), "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(description.Text))
                    {
                        MessageBox.Show(this, Localization.T("Bitte eine Beschreibung eingeben."), "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(ok);
                table.Controls.Add(buttons, 0, 4);
                table.SetColumnSpan(buttons, 3);
                Controls.Add(table);
                AcceptButton = ok;
                CancelButton = cancel;
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }

            private void AddRow(TableLayoutPanel table, int row, string label, Control input, bool span = true)
            {
                table.Controls.Add(new Label { Text = label, ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
                input.Dock = DockStyle.Fill;
                input.Font = Typography.Create(10f * scale, FontStyle.Regular);
                if (input is TextBox)
                {
                    input.BackColor = Palette.For(true).Input;
                    input.ForeColor = Palette.For(true).Text;
                }
                table.Controls.Add(input, 1, row);
                if (span) table.SetColumnSpan(input, 2);
            }
        }

        private sealed class KeyCaptureDialog : Form
        {
            private readonly float scale;
            public string CapturedKey { get; private set; }

            public KeyCaptureDialog(float scale, string current)
            {
                this.scale = scale;
                Text = Localization.T("Taste drücken");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                KeyPreview = true;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(430), Px(196));
                var title = new Label { Text = Localization.T("Neue Taste für diese Aktion drücken."), Font = Typography.Create(11f * scale, FontStyle.Bold), ForeColor = Palette.For(true).Text, AutoSize = true, Left = Px(18), Top = Px(18) };
                var hint = new Label { Text = (Localization.T("Aktuell: ")) + (String.IsNullOrEmpty(current) ? (Localization.T("(leer)")) : current), Font = Typography.Create(9.5f * scale, FontStyle.Regular), ForeColor = Palette.For(true).Muted, AutoSize = true, Left = Px(18), Top = Px(52) };
                var warning = new Label { Text = string.Empty, Font = Typography.Create(9f * scale, FontStyle.Regular), ForeColor = Palette.For(true).Accent, AutoSize = false, Width = Px(394), Height = Px(48), Left = Px(18), Top = Px(78), Visible = false };
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Left = Px(310), Top = Px(148), Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                Controls.Add(title);
                Controls.Add(hint);
                Controls.Add(warning);
                Controls.Add(cancel);
                CancelButton = cancel;
                KeyDown += delegate(object sender, KeyEventArgs e)
                {
                    if (e.Control || e.Alt)
                    {
                        warning.Text = Localization.T("keymap.xml speichert nur einzelne Tasten. Strg-/Alt-Kombinationen sind nicht Teil der nativen MediaPortal-Tastenbelegung; bitte die Taste ohne Strg oder Alt drücken.");
                        warning.Visible = true;
                        return;
                    }
                    string value = MapKey(e.KeyCode);
                    if (value == null) return;
                    CapturedKey = value;
                    DialogResult = DialogResult.OK;
                    Close();
                };
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }

            private static string MapKey(Keys key)
            {
                if (key == Keys.ShiftKey || key == Keys.ControlKey || key == Keys.Menu || key == Keys.LWin || key == Keys.RWin || key == Keys.None) return null;
                if (key >= Keys.D0 && key <= Keys.D9) return ((int)key - (int)Keys.D0).ToString();
                if (key >= Keys.A && key <= Keys.Z) return ((char)('A' + ((int)key - (int)Keys.A))).ToString();
                if (key >= Keys.F1 && key <= Keys.F12) return "F" + ((int)key - (int)Keys.F1 + 1);
                if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return "num" + ((int)key - (int)Keys.NumPad0);
                switch (key)
                {
                    case Keys.Left: return "left";
                    case Keys.Right: return "right";
                    case Keys.Up: return "up";
                    case Keys.Down: return "down";
                    case Keys.Home: return "home";
                    case Keys.End: return "end";
                    case Keys.PageUp: return "pageup";
                    case Keys.PageDown: return "pagedown";
                    case Keys.Insert: return "insert";
                    case Keys.Delete: return "delete";
                    case Keys.Enter: return "enter";
                    case Keys.Escape: return "esc";
                    case Keys.Space: return "SPACE";
                    case Keys.Back: return "back";
                    case Keys.Tab: return "tab";
                    case Keys.Oemplus: return "+";
                    case Keys.OemMinus: return "-";
                    case Keys.OemOpenBrackets: return "[";
                    case Keys.OemCloseBrackets: return "]";
                    case Keys.OemSemicolon: return ";";
                    case Keys.OemQuotes: return "'";
                    case Keys.Oemcomma: return ",";
                    case Keys.OemPeriod: return ".";
                    case Keys.OemQuestion: return "/";
                    case Keys.OemPipe: return "\\";
                    case Keys.Oemtilde: return "`";
                }
                return key.ToString().ToLowerInvariant();
            }
        }
    }
}
