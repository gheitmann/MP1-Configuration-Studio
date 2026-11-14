using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeDspEditor : UserControl, IStudioFileEditor
    {
        private readonly string path;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly TextBox musicDirectory = new TextBox { BorderStyle = BorderStyle.FixedSingle };
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Label status = new Label();
        private XmlDocument document;
        private Encoding encoding = new UTF8Encoding(false);
        private bool dirty;
        private bool loading;

        public NativeDspEditor(string configDirectory, float scale)
        {
            this.path = Path.Combine(configDirectory, "musicdsp.xml");
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 4, BackColor = palette.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(150)));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(100)));
            top.Controls.Add(new Label { Text = Localization.IsEnglish ? "Music folder" : "Musikordner", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            musicDirectory.Dock = DockStyle.Fill;
            musicDirectory.Font = body;
            musicDirectory.BackColor = palette.Input;
            musicDirectory.ForeColor = palette.Text;
            musicDirectory.TextChanged += delegate { if (!loading && document != null) { SetRootAttribute("MusicDirectory", musicDirectory.Text); MarkDirty(); } };
            top.Controls.Add(musicDirectory, 1, 0);
            var browse = ButtonFor(Localization.IsEnglish ? "Browse …" : "Ordner …", delegate { BrowseMusicDirectory(); }, body);
            browse.Dock = DockStyle.Fill;
            top.Controls.Add(browse, 2, 0);
            var save = ButtonFor(Localization.IsEnglish ? "Save" : "Speichern", delegate { SaveFile(); }, body);
            save.Dock = DockStyle.Fill;
            top.Controls.Add(save, 3, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(38), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Reload" : "Neu laden", delegate { Reload(); }, body));
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Add BASS effect" : "BASS-Effekt hinzufügen", delegate { AddElement("BASS", "EffectName"); }, body));
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Add VST plugin" : "VST-Plugin hinzufügen", delegate { AddElement("VST", "PluginDll"); }, body));
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Add Winamp plugin" : "Winamp-Plugin hinzufügen", delegate { AddElement("Winamp", "PluginDll"); }, body));
            status.AutoSize = false;
            status.Width = Px(300);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            actions.Controls.Add(status);

            scroll.Controls.Add(cards);
            scroll.Resize += delegate { cards.Width = Math.Max(Px(520), scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12)); };
            Controls.Add(scroll);
            Controls.Add(actions);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.IsEnglish ? "Discard unsaved music DSP changes?" : "Ungespeicherte Musik-DSP-Änderungen verwerfen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private static string L(string de, string en) { return Localization.IsEnglish ? en : de; }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(80), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LoadFile()
        {
            try
            {
                if (File.Exists(path))
                {
                    string text = NativeXmlFileStore.ReadText(path, out encoding);
                    document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                    document.LoadXml(text);
                }
                else
                {
                    encoding = new UTF8Encoding(false);
                    document = new XmlDocument();
                    XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
                    document.AppendChild(declaration);
                    XmlElement root = document.CreateElement("Settings");
                    root.SetAttribute("MusicDirectory", "C:\\");
                    document.AppendChild(root);
                }
                loading = true;
                musicDirectory.Text = document.DocumentElement.GetAttribute("MusicDirectory");
                loading = false;
                dirty = false;
                BuildCards();
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

        private void SetRootAttribute(string name, string value)
        {
            if (document == null) return;
            document.DocumentElement.SetAttribute(name, value ?? string.Empty);
        }

        private void BuildCards()
        {
            cards.SuspendLayout();
            try
            {
                cards.Controls.Clear();
                if (document == null) return;
                int count = 0;
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    if (String.Equals(element.Name, "BASS", StringComparison.OrdinalIgnoreCase))
                    {
                        cards.Controls.Add(BuildEffectCard(element, "BASS"));
                        count++;
                    }
                    else if (String.Equals(element.Name, "VST", StringComparison.OrdinalIgnoreCase) || String.Equals(element.Name, "Winamp", StringComparison.OrdinalIgnoreCase))
                    {
                        cards.Controls.Add(BuildPluginCard(element));
                        count++;
                    }
                }
                if (count == 0)
                    cards.Controls.Add(new Label { Text = L("Keine DSP-Effekte oder Plugins konfiguriert.", "No DSP effects or plugins configured."), Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(8), 0, Px(8)) });
            }
            finally { cards.ResumeLayout(true); }
            cards.Width = Math.Max(Px(520), scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12));
            status.Text = L("Musik-DSP", "Music DSP") + (dirty ? L("  ·  ungespeichert", "  ·  unsaved") : "");
        }

        private Control BuildEffectCard(XmlElement effect, string kind)
        {
            var card = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(10)), Padding = new Padding(Px(12), Px(8), Px(12), Px(10)) };
            var header = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(100)));
            header.Controls.Add(new Label { Text = L("Effekt", "Effect"), Font = Typography.Create(9f * scale, FontStyle.Regular), ForeColor = palette.Muted, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            var name = new TextBox { Text = effect.GetAttribute("EffectName"), Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };
            name.TextChanged += delegate { if (effect.GetAttribute("EffectName") != name.Text) { effect.SetAttribute("EffectName", name.Text); MarkDirty(); } };
            header.Controls.Add(name, 1, 0);
            var remove = ButtonFor(L("Entfernen", "Remove"), delegate { RemoveElement(effect, kind); }, Typography.Create(9f * scale, FontStyle.Regular));
            remove.Dock = DockStyle.Fill;
            header.Controls.Add(remove, 2, 0);
            card.Controls.Add(header, 0, 0);

            var parameters = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, Px(6), 0, 0) };
            foreach (XmlNode node in effect.ChildNodes)
            {
                var parameter = node as XmlElement;
                if (parameter == null || !String.Equals(parameter.Name, "Parameter", StringComparison.OrdinalIgnoreCase)) continue;
                parameters.Controls.Add(BuildParameterRow(parameter, false));
            }
            card.Controls.Add(parameters, 0, 1);
            var add = ButtonFor(L("Parameter hinzufügen", "Add parameter"), delegate { AddParameter(effect, false); }, Typography.Create(9f * scale, FontStyle.Regular));
            add.Margin = new Padding(0, Px(4), 0, 0);
            card.Controls.Add(add, 0, 2);
            return card;
        }

        private Control BuildPluginCard(XmlElement plugin)
        {
            var card = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(10)), Padding = new Padding(Px(12), Px(8), Px(12), Px(10)) };
            var header = new TableLayoutPanel { ColumnCount = 4, RowCount = 1, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(90)));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(100)));
            header.Controls.Add(new Label { Text = plugin.Name, Font = Typography.Create(9f * scale, FontStyle.Regular), ForeColor = palette.Muted, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            var dll = new TextBox { Text = plugin.GetAttribute("PluginDll"), Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };
            dll.TextChanged += delegate { if (plugin.GetAttribute("PluginDll") != dll.Text) { plugin.SetAttribute("PluginDll", dll.Text); MarkDirty(); } };
            header.Controls.Add(dll, 1, 0);
            var browse = ButtonFor("…", delegate { BrowseDll(dll); }, Typography.Create(9f * scale, FontStyle.Regular));
            browse.Dock = DockStyle.Fill;
            header.Controls.Add(browse, 2, 0);
            var remove = ButtonFor(L("Entfernen", "Remove"), delegate { RemoveElement(plugin, plugin.Name); }, Typography.Create(9f * scale, FontStyle.Regular));
            remove.Dock = DockStyle.Fill;
            header.Controls.Add(remove, 3, 0);
            card.Controls.Add(header, 0, 0);

            var parameters = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, Px(6), 0, 0) };
            foreach (XmlNode node in plugin.ChildNodes)
            {
                var parameter = node as XmlElement;
                if (parameter == null || !String.Equals(parameter.Name, "Parameter", StringComparison.OrdinalIgnoreCase)) continue;
                parameters.Controls.Add(BuildParameterRow(parameter, true));
            }
            card.Controls.Add(parameters, 0, 1);
            if (String.Equals(plugin.Name, "VST", StringComparison.OrdinalIgnoreCase))
            {
                var add = ButtonFor(L("Parameter hinzufügen", "Add parameter"), delegate { AddParameter(plugin, true); }, Typography.Create(9f * scale, FontStyle.Regular));
                add.Margin = new Padding(0, Px(4), 0, 0);
                card.Controls.Add(add, 0, 2);
            }
            return card;
        }

        private Control BuildParameterRow(XmlElement parameter, bool withIndex)
        {
            var row = new TableLayoutPanel { ColumnCount = withIndex ? 4 : 3, RowCount = 1, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(4)) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(150)));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            if (withIndex) row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(70)));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(34)));
            var name = Input(parameter.GetAttribute("Name"), delegate(string value) { parameter.SetAttribute("Name", value); MarkDirty(); });
            row.Controls.Add(name, 0, 0);
            var value = Input(parameter.GetAttribute("Value"), delegate(string text) { parameter.SetAttribute("Value", text); MarkDirty(); });
            row.Controls.Add(value, 1, 0);
            if (withIndex)
            {
                var index = Input(parameter.GetAttribute("Index"), delegate(string text) { parameter.SetAttribute("Index", text); MarkDirty(); });
                row.Controls.Add(index, 2, 0);
            }
            var remove = new Button { Text = "×", Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(Px(4), 0, 0, 0) };
            remove.FlatAppearance.BorderColor = palette.Border;
            remove.Click += delegate { parameter.ParentNode.RemoveChild(parameter); MarkDirty(); BuildCards(); };
            row.Controls.Add(remove, withIndex ? 3 : 2, 0);
            return row;
        }

        private TextBox Input(string value, Action<string> changed)
        {
            var box = new TextBox { Text = value ?? string.Empty, Font = Typography.Create(9f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0, 0, Px(4), 0) };
            box.TextChanged += delegate { changed(box.Text); };
            return box;
        }

        private void BrowseMusicDirectory()
        {
            using (var dialog = new FolderBrowserDialog { Description = L("Musikordner wählen", "Choose music folder"), SelectedPath = musicDirectory.Text })
            {
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    musicDirectory.Text = dialog.SelectedPath;
                    SetRootAttribute("MusicDirectory", musicDirectory.Text);
                    MarkDirty();
                }
            }
        }

        private void BrowseDll(TextBox target)
        {
            using (var dialog = new OpenFileDialog { Title = L("DSP-Plugin wählen", "Choose DSP plugin"), Filter = "DLL (*.dll)|*.dll|Alle Dateien (*.*)|*.*", CheckFileExists = true })
            {
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK) target.Text = dialog.FileName;
            }
        }

        private void AddElement(string name, string attribute)
        {
            if (document == null) return;
            XmlElement element = document.CreateElement(name);
            element.SetAttribute(attribute, string.Empty);
            document.DocumentElement.AppendChild(element);
            MarkDirty();
            BuildCards();
            scroll.AutoScrollPosition = new Point(0, scroll.VerticalScroll.Maximum);
        }

        private void AddParameter(XmlElement parent, bool withIndex)
        {
            XmlElement parameter = parent.OwnerDocument.CreateElement("Parameter");
            parameter.SetAttribute("Name", string.Empty);
            parameter.SetAttribute("Value", string.Empty);
            if (withIndex) parameter.SetAttribute("Index", "0");
            parent.AppendChild(parameter);
            MarkDirty();
            BuildCards();
        }

        private void RemoveElement(XmlElement element, string kind)
        {
            if (MessageBox.Show(FindForm(), (Localization.IsEnglish ? "Remove this entry?" : "Diesen Eintrag entfernen?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            document.DocumentElement.RemoveChild(element);
            MarkDirty();
            BuildCards();
        }

        private void MarkDirty()
        {
            dirty = true;
            status.Text = L("Musik-DSP  ·  ungespeichert", "Music DSP  ·  unsaved");
        }

        private void SaveFile()
        {
            if (document == null) return;
            try
            {
                string backup = NativeXmlFileStore.SaveDocument(path, document, encoding);
                dirty = false;
                status.Text = (Localization.IsEnglish ? "Saved · backup: " : "Gespeichert · Sicherung: ") + Path.GetFileName(backup);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.IsEnglish ? "The music DSP settings could not be saved.\r\n\r\n" : "Die Musik-DSP-Einstellungen konnten nicht gespeichert werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
