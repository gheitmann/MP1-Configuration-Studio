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
        private readonly ComboBox contextChoice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
        private readonly TextBox search = new TextBox { BorderStyle = BorderStyle.FixedSingle };
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
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
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(150)));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(220)));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(100)));
            top.Controls.Add(new Label { Text = Localization.IsEnglish ? "Context" : "Bereich", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            contextChoice.Dock = DockStyle.Fill;
            contextChoice.Font = body;
            contextChoice.BackColor = palette.Input;
            contextChoice.ForeColor = palette.Text;
            contextChoice.SelectedIndexChanged += delegate { if (!building) BuildTable(); };
            top.Controls.Add(contextChoice, 1, 0);
            search.Dock = DockStyle.Fill;
            search.Font = body;
            search.BackColor = palette.Input;
            search.ForeColor = palette.Muted;
            search.Text = Localization.IsEnglish ? "Filter actions …" : "Aktionen filtern …";
            search.GotFocus += delegate { if (search.ForeColor == palette.Muted) { search.Text = string.Empty; search.ForeColor = palette.Text; } };
            search.LostFocus += delegate { if (search.Text.Length == 0) { search.ForeColor = palette.Muted; search.Text = Localization.IsEnglish ? "Filter actions …" : "Aktionen filtern …"; } };
            search.TextChanged += delegate { if (!building) BuildTable(); };
            top.Controls.Add(search, 2, 0);
            var save = ButtonFor(Localization.IsEnglish ? "Save" : "Speichern", delegate { SaveFile(); }, body);
            save.Dock = DockStyle.Fill;
            top.Controls.Add(save, 3, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(38), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            var reload = ButtonFor(Localization.IsEnglish ? "Reload" : "Neu laden", delegate { Reload(); }, body);
            actions.Controls.Add(reload);
            status.AutoSize = false;
            status.Width = Px(600);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            actions.Controls.Add(status);

            table.Padding = new Padding(0);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(150)));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(34)));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(160)));
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            Controls.Add(scroll);
            Controls.Add(actions);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.IsEnglish ? "Discard unsaved key mapping changes?" : "Ungespeicherte Tastenbelegung verwerfen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(80), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LayoutTable()
        {
            table.Width = Math.Max(Px(620), scroll.ClientSize.Width - scroll.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
        }

        private void LoadFile()
        {
            try
            {
                if (!File.Exists(path))
                {
                    status.Text = (Localization.IsEnglish ? "File not found: " : "Datei nicht gefunden: ") + path;
                    return;
                }
                string text = NativeXmlFileStore.ReadText(path, out encoding);
                document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                document.LoadXml(text);
                contexts.Clear();
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    if (String.Equals(element.Name, "global", StringComparison.OrdinalIgnoreCase))
                        contexts.Add(new Context { Label = Localization.IsEnglish ? "Global" : "Global", Element = element });
                    else if (String.Equals(element.Name, "window", StringComparison.OrdinalIgnoreCase))
                    {
                        string description = ChildText(element, "description");
                        string id = ChildText(element, "id");
                        contexts.Add(new Context { Label = (String.IsNullOrEmpty(description) ? (Localization.IsEnglish ? "Window" : "Fenster") : description) + (String.IsNullOrEmpty(id) ? "" : "  ·  #" + id), Element = element });
                    }
                }
                building = true;
                contextChoice.Items.Clear();
                foreach (Context context in contexts) contextChoice.Items.Add(context.Label);
                if (contextChoice.Items.Count > 0) contextChoice.SelectedIndex = 0;
                building = false;
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
                    var empty = new Label { Text = Localization.IsEnglish ? "No actions in this context." : "Keine Aktionen in diesem Bereich.", Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(6), 0, Px(6)) };
                    table.Controls.Add(empty, 0, table.RowCount);
                    table.SetColumnSpan(empty, 4);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                }
                status.Text = context.Label + "  ·  " + visible + (Localization.IsEnglish ? " actions" : " Aktionen") + (dirty ? (Localization.IsEnglish ? "  ·  unsaved" : "  ·  ungespeichert") : "");
            }
            finally { table.ResumeLayout(true); }
            LayoutTable();
        }

        private void AddHeader()
        {
            string[] titles = Localization.IsEnglish
                ? new[] { "Action", "Key", "", "Sound" }
                : new[] { "Aktion", "Taste", "", "Sound" };
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
            var info = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, Px(2), Px(8), Px(2)), BackColor = palette.Background };
            info.Controls.Add(new Label { Text = String.IsNullOrEmpty(description) ? (Localization.IsEnglish ? "(no description)" : "(ohne Beschreibung)") : description, Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Text, AutoSize = true, UseMnemonic = false, Margin = new Padding(0) }, 0, 0);
            info.Controls.Add(new Label { Text = "#" + id, Font = Typography.Create(8.5f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0) }, 0, 1);
            table.Controls.Add(info, 0, table.RowCount);

            var keyButton = new Button { Text = String.IsNullOrEmpty(key) ? (Localization.IsEnglish ? "(empty)" : "(leer)") : key, Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), Px(4), Px(2)) };
            keyButton.FlatAppearance.BorderColor = palette.Border;
            keyButton.Click += delegate { CaptureKey(action, keyButton); };
            table.Controls.Add(keyButton, 1, table.RowCount);

            var clear = new Button { Text = "×", Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), Px(8), Px(2)) };
            clear.FlatAppearance.BorderColor = palette.Border;
            clear.Click += delegate { SetActionValue(action, "key", string.Empty); keyButton.Text = Localization.IsEnglish ? "(empty)" : "(leer)"; MarkDirty(); BuildTable(); };
            table.Controls.Add(clear, 2, table.RowCount);

            var sound = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, FlatStyle = FlatStyle.Flat, Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(0, Px(2), 0, Px(2)) };
            foreach (string name in new[] { "", "click.wav", "back.wav", "cursor.wav", "photo.wav" })
                sound.Items.Add(name);
            string soundValue = ChildText(action, "sound") ?? string.Empty;
            if (!sound.Items.Contains(soundValue)) sound.Items.Add(soundValue);
            sound.SelectedItem = soundValue;
            sound.Text = soundValue;
            sound.SelectedIndexChanged += delegate { if (sound.SelectedItem != null) { SetActionValue(action, "sound", sound.SelectedItem.ToString()); MarkDirty(); } };
            sound.Leave += delegate { SetActionValue(action, "sound", sound.Text); MarkDirty(); };
            table.Controls.Add(sound, 3, table.RowCount);

            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowCount++;
        }

        private void CaptureKey(XmlElement action, Button button)
        {
            using (var dialog = new KeyCaptureDialog(scale, ChildText(action, "key")))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                string key = dialog.CapturedKey;
                SetActionValue(action, "key", key);
                button.Text = String.IsNullOrEmpty(key) ? (Localization.IsEnglish ? "(empty)" : "(leer)") : key;
                MarkDirty();
                string duplicate = FindDuplicate(action, key);
                if (duplicate != null)
                    status.Text = (Localization.IsEnglish ? "Note: key " + key + " is already used by “" + duplicate + "”." : "Hinweis: Taste " + key + " ist bereits mit „" + duplicate + "“ belegt.");
                else
                    BuildTable();
            }
        }

        private string FindDuplicate(XmlElement edited, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;
            Context context = contexts.FirstOrDefault(item => item.Element == edited.ParentNode);
            if (context == null) return null;
            foreach (XmlNode node in context.Element.ChildNodes)
            {
                var action = node as XmlElement;
                if (action == null || action == edited) continue;
                if (String.Equals(ChildText(action, "key"), key, StringComparison.OrdinalIgnoreCase))
                    return ChildText(action, "description");
            }
            return null;
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
            if (status.Text.IndexOf(Localization.IsEnglish ? "unsaved" : "ungespeichert", StringComparison.OrdinalIgnoreCase) < 0)
                status.Text += Localization.IsEnglish ? "  ·  unsaved" : "  ·  ungespeichert";
        }

        private void SaveFile()
        {
            if (document == null) return;
            try
            {
                string backup = NativeXmlFileStore.SaveDocument(path, document, encoding);
                dirty = false;
                status.Text = (Localization.IsEnglish ? "Saved · backup: " : "Gespeichert · Sicherung: ") + Path.GetFileName(backup);
                BuildTable();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.IsEnglish ? "The key mapping could not be saved.\r\n\r\n" : "Die Tastenbelegung konnte nicht gespeichert werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private sealed class KeyCaptureDialog : Form
        {
            private readonly float scale;
            public string CapturedKey { get; private set; }

            public KeyCaptureDialog(float scale, string current)
            {
                this.scale = scale;
                Text = Localization.IsEnglish ? "Press a key" : "Taste drücken";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                KeyPreview = true;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(430), Px(170));
                var title = new Label { Text = Localization.IsEnglish ? "Press the new key for this action." : "Neue Taste für diese Aktion drücken.", Font = Typography.Create(11f * scale, FontStyle.Bold), ForeColor = Palette.For(true).Text, AutoSize = true, Left = Px(18), Top = Px(18) };
                var hint = new Label { Text = (Localization.IsEnglish ? "Current: " : "Aktuell: ") + (String.IsNullOrEmpty(current) ? (Localization.IsEnglish ? "(empty)" : "(leer)") : current), Font = Typography.Create(9.5f * scale, FontStyle.Regular), ForeColor = Palette.For(true).Muted, AutoSize = true, Left = Px(18), Top = Px(52) };
                var cancel = new Button { Text = Localization.IsEnglish ? "Cancel" : "Abbrechen", DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Left = Px(310), Top = Px(120), Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                Controls.Add(title);
                Controls.Add(hint);
                Controls.Add(cancel);
                CancelButton = cancel;
                KeyDown += delegate(object sender, KeyEventArgs e)
                {
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
