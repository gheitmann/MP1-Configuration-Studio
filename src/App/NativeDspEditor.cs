using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeDspEditor : UserControl, IStudioFileEditor
    {
        private readonly string path;
        private readonly string runtime;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly StudioTextInput musicDirectory;
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Label status = new Label();
        private XmlDocument document;
        private Encoding encoding = new UTF8Encoding(false);
        private bool dirty;
        private bool loading;
        private bool building;
        private CheckBox gainCheck;
        private StudioTrackBar gainTrack;
        private Label gainLabel;
        private CheckBox dynCheck;
        private StudioChoice dynCombo;
        private CheckBox compCheck;
        private StudioTrackBar compTrack;
        private Label compLabel;

        public NativeDspEditor(string configDirectory, string runtime, float scale)
        {
            this.path = Path.Combine(configDirectory, "musicdsp.xml");
            this.runtime = runtime;
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 4, BackColor = palette.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            top.Controls.Add(new Label { Text = Localization.T("Musikordner"), ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            musicDirectory = new StudioTextInput(scale, body, palette) { Dock = DockStyle.Fill, Colors = () => palette, AccessibleName = Localization.T("Musikordner") };
            musicDirectory.Box.TextChanged += delegate { if (!loading && document != null) { SetRootAttribute("MusicDirectory", musicDirectory.Text); MarkDirty(); } };
            top.Controls.Add(musicDirectory, 1, 0);
            var browse = ButtonFor(Localization.T("Ordner …"), delegate { BrowseMusicDirectory(); }, body);
            browse.Dock = DockStyle.Fill;
            top.Controls.Add(browse, 2, 0);
            var save = ButtonFor(Localization.T("Speichern"), delegate { SaveFile(); }, body);
            save.Dock = DockStyle.Fill;
            top.Controls.Add(save, 3, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            actions.Controls.Add(ButtonFor(Localization.T("Neu laden"), delegate { Reload(); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("BASS-Effekt hinzufügen"), delegate { AddElement("BASS", "EffectName"); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("VST-Plugin hinzufügen"), delegate { AddElement("VST", "PluginDll"); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("VST suchen"), delegate { ScanPlugins("VST"); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("Winamp suchen"), delegate { ScanPlugins("Winamp"); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("Winamp-Plugin hinzufügen"), delegate { AddElement("Winamp", "PluginDll"); }, body));
            actions.Controls.Add(ButtonFor(Localization.T("Testwiedergabe"), delegate { TestPlayback(); }, body));
            status.AutoSize = false;
            status.Width = Px(160);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            actions.Controls.Add(status);

            scroll.Controls.Add(cards);
            scroll.Resize += delegate { ResizeCards(); };
            Controls.Add(new StudioScrollHost(scroll, delegate { return new Size(Math.Max(1, cards.Width), Math.Max(1, cards.Height)); }, () => palette, false, (Color?)palette.Background));
            Controls.Add(actions);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.T("Ungespeicherte Musik-DSP-Änderungen verwerfen?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public bool SaveChanges()
        {
            SaveFile();
            return !dirty;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private static string L(string de, string en) { return Localization.T2(de, en); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(80), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
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
            building = true;
            try
            {
                cards.Controls.Clear();
                if (document == null) return;
                cards.Controls.Add(BuildKnownEffects());
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    if (String.Equals(element.Name, "BASS", StringComparison.OrdinalIgnoreCase))
                    {
                        string name = element.GetAttribute("EffectName");
                        if (IsKnownEffect(name)) continue;
                        cards.Controls.Add(BuildEffectCard(element, "BASS"));
                    }
                    else if (String.Equals(element.Name, "VST", StringComparison.OrdinalIgnoreCase) || String.Equals(element.Name, "Winamp", StringComparison.OrdinalIgnoreCase))
                    {
                        cards.Controls.Add(BuildPluginCard(element));
                    }
                }
            }
            finally
            {
                building = false;
                cards.ResumeLayout(true);
            }
            ResizeCards();
            status.Text = L("Musik-DSP", "Music DSP") + (dirty ? L("  ·  ungespeichert", "  ·  unsaved") : "");
        }

        private void ResizeCards()
        {
            int width = Math.Max(Px(440), scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12));
            cards.Width = width;
            foreach (Control control in cards.Controls)
            {
                if (!(control is TableLayoutPanel)) continue;
                control.MinimumSize = new Size(width - Px(4), 0);
                control.MaximumSize = new Size(width - Px(4), 0);
                control.Width = width - Px(4);
            }
        }

        private static bool IsKnownEffect(string name)
        {
            return String.Equals(name, "Gain", StringComparison.OrdinalIgnoreCase)
                || String.Equals(name, "DynAmp", StringComparison.OrdinalIgnoreCase)
                || String.Equals(name, "Compressor", StringComparison.OrdinalIgnoreCase);
        }

        private Control BuildKnownEffects()
        {
            var card = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(10)), Padding = new Padding(Px(12), Px(8), Px(12), Px(10)) };
            card.Controls.Add(new Label { Text = L("Bekannte BASS-Effekte", "Known BASS effects"), Font = Typography.Create(11f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, Margin = new Padding(0, 0, 0, Px(6)) }, 0, 0);

            double gain = 0;
            XmlElement gainEffect = FindEffect("Gain");
            if (gainEffect != null)
                double.TryParse(ReadParameter(gainEffect, "Gain_dbV"), NumberStyles.Float, CultureInfo.InvariantCulture, out gain);
            gainTrack = new StudioTrackBar(scale) { Minimum = -160, Maximum = 160, TickFrequency = 20, SmallChange = 1, LargeChange = 10, Value = Math.Max(-160, Math.Min(160, (int)Math.Round(gain * 10))), Dock = DockStyle.Fill, Colors = () => palette, Height = Px(30), Margin = new Padding(0) };
            gainLabel = new Label { Text = gain.ToString("0.0", CultureInfo.InvariantCulture) + " dB", ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            gainCheck = new StudioCheck(scale) { Text = "Gain", Checked = gainEffect != null, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), Font = Typography.Create(9.5f * scale, FontStyle.Regular), Colors = () => palette, Margin = new Padding(0) };
            card.Controls.Add(KnownRow(gainCheck, gainTrack, gainLabel), 0, 1);
            gainCheck.CheckedChanged += delegate
            {
                if (building) return;
                gainTrack.Enabled = gainCheck.Checked;
                gainLabel.Enabled = gainCheck.Checked;
                if (gainCheck.Checked) SetEffect("Gain", "Gain_dbV", (gainTrack.Value / 10.0).ToString("0.0", CultureInfo.InvariantCulture));
                else RemoveEffect("Gain");
                MarkDirty();
            };
            gainTrack.ValueChanged += delegate
            {
                if (building) return;
                double value = gainTrack.Value / 10.0;
                gainLabel.Text = value.ToString("0.0", CultureInfo.InvariantCulture) + " dB";
                if (gainCheck.Checked) SetEffect("Gain", "Gain_dbV", value.ToString("0.0", CultureInfo.InvariantCulture));
                MarkDirty();
            };
            gainTrack.Enabled = gainEffect != null;
            gainLabel.Enabled = gainEffect != null;

            int preset = 0;
            XmlElement dynEffect = FindEffect("DynAmp");
            if (dynEffect != null) Int32.TryParse(ReadParameter(dynEffect, "Preset"), out preset);
            dynCombo = new StudioChoice(scale) { Font = Typography.Create(9.5f * scale, FontStyle.Regular), Colors = () => palette, Dock = DockStyle.Fill, AccessibleName = L("Dynamische Verstärkung", "Dynamic amplification") };
            dynCombo.SetOptions(new[] { new ChoiceOption("0", "Soft"), new ChoiceOption("1", "Medium"), new ChoiceOption("2", "Hard") }, Math.Max(0, Math.Min(2, preset)).ToString(CultureInfo.InvariantCulture));
            dynCheck = new StudioCheck(scale) { Text = L("Dynamische Verstärkung", "Dynamic amplification"), Checked = dynEffect != null, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), Font = Typography.Create(9.5f * scale, FontStyle.Regular), Colors = () => palette, Margin = new Padding(0) };
            card.Controls.Add(KnownRow(dynCheck, dynCombo, new Label { Text = "Preset", ForeColor = palette.Muted, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }), 0, 2);
            dynCheck.CheckedChanged += delegate
            {
                if (building) return;
                dynCombo.Enabled = dynCheck.Checked;
                if (dynCheck.Checked) SetEffect("DynAmp", "Preset", dynCombo.SelectedIndex.ToString());
                else RemoveEffect("DynAmp");
                MarkDirty();
            };
            dynCombo.SelectedIndexChanged += delegate
            {
                if (building || !dynCheck.Checked) return;
                SetEffect("DynAmp", "Preset", dynCombo.SelectedIndex.ToString());
                MarkDirty();
            };
            dynCombo.Enabled = dynEffect != null;
            dynCombo.SelectedIndex = Math.Max(0, Math.Min(2, preset));

            int threshold = -60;
            XmlElement compEffect = FindEffect("Compressor");
            if (compEffect != null) Int32.TryParse(ReadParameter(compEffect, "Threshold"), out threshold);
            compTrack = new StudioTrackBar(scale) { Minimum = -250, Maximum = 0, TickFrequency = 25, SmallChange = 1, LargeChange = 10, Value = Math.Max(-250, Math.Min(0, threshold)), Dock = DockStyle.Fill, Colors = () => palette, Height = Px(30), Margin = new Padding(0) };
            compLabel = new Label { Text = (threshold / 10.0).ToString("0.0", CultureInfo.InvariantCulture) + " dB", ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            compCheck = new StudioCheck(scale) { Text = L("Kompressor", "Compressor"), Checked = compEffect != null, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), Font = Typography.Create(9.5f * scale, FontStyle.Regular), Colors = () => palette, Margin = new Padding(0) };
            card.Controls.Add(KnownRow(compCheck, compTrack, compLabel), 0, 3);
            compCheck.CheckedChanged += delegate
            {
                if (building) return;
                compTrack.Enabled = compCheck.Checked;
                compLabel.Enabled = compCheck.Checked;
                if (compCheck.Checked) SetEffect("Compressor", "Threshold", compTrack.Value.ToString(CultureInfo.InvariantCulture));
                else RemoveEffect("Compressor");
                MarkDirty();
            };
            compTrack.ValueChanged += delegate
            {
                if (building) return;
                compLabel.Text = (compTrack.Value / 10.0).ToString("0.0", CultureInfo.InvariantCulture) + " dB";
                if (compCheck.Checked) SetEffect("Compressor", "Threshold", compTrack.Value.ToString(CultureInfo.InvariantCulture));
                MarkDirty();
            };
            compTrack.Enabled = compEffect != null;
            compLabel.Enabled = compEffect != null;
            return card;
        }

        private Control KnownRow(CheckBox check, Control control, Label value)
        {
            var row = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0, Px(4), 0, 0) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            row.Controls.Add(check, 0, 0);
            control.Dock = DockStyle.Fill;
            row.Controls.Add(control, 1, 0);
            value.Dock = DockStyle.Fill;
            row.Controls.Add(value, 2, 0);
            return row;
        }

        private XmlElement FindEffect(string name)
        {
            if (document == null) return null;
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, "BASS", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(element.GetAttribute("EffectName"), name, StringComparison.OrdinalIgnoreCase)) return element;
            }
            return null;
        }

        private void SetEffect(string name, string parameter, string value)
        {
            if (document == null) return;
            XmlElement effect = FindEffect(name);
            if (effect == null)
            {
                effect = document.CreateElement("BASS");
                effect.SetAttribute("EffectName", name);
                document.DocumentElement.AppendChild(effect);
            }
            XmlElement parameterNode = null;
            foreach (XmlNode node in effect.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, "Parameter", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(element.GetAttribute("Name"), parameter, StringComparison.OrdinalIgnoreCase)) { parameterNode = element; break; }
            }
            if (parameterNode == null)
            {
                parameterNode = document.CreateElement("Parameter");
                parameterNode.SetAttribute("Name", parameter);
                parameterNode.SetAttribute("Value", string.Empty);
                effect.AppendChild(parameterNode);
            }
            parameterNode.SetAttribute("Value", value ?? string.Empty);
        }

        private string ReadParameter(XmlElement effect, string name)
        {
            foreach (XmlNode node in effect.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.Name, "Parameter", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(element.GetAttribute("Name"), name, StringComparison.OrdinalIgnoreCase)) return element.GetAttribute("Value");
            }
            return string.Empty;
        }

        private void RemoveEffect(string name)
        {
            XmlElement effect = FindEffect(name);
            if (effect != null) document.DocumentElement.RemoveChild(effect);
        }

        private void ScanPlugins(string kind)
        {
            string root = Path.Combine(runtime, "musicplayer", "plugins", "dsp");
            if (!Directory.Exists(root))
            {
                MessageBox.Show(FindForm(), (Localization.T("DSP-Plugin-Ordner nicht gefunden:\r\n")) + root, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] files = Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories)
                .Select(file => file.Substring(root.Length).TrimStart('\\'))
                .OrderBy(file => file, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            using (var dialog = new PluginPickerDialog(scale, kind, files))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                XmlElement plugin = document.CreateElement(kind);
                plugin.SetAttribute("PluginDll", dialog.SelectedFile);
                document.DocumentElement.AppendChild(plugin);
                MarkDirty();
                BuildCards();
            }
        }

        private void TestPlayback()
        {
            double gain = gainCheck != null && gainCheck.Checked && gainTrack != null ? gainTrack.Value / 10.0 : 0;
            bool damp = dynCheck != null && dynCheck.Checked;
            int dampPreset = dynCombo == null ? 0 : Math.Max(0, dynCombo.SelectedIndex);
            bool compressor = compCheck != null && compCheck.Checked;
            int threshold = compTrack == null ? -60 : compTrack.Value;
            var vst = new List<string>();
            if (document != null)
            {
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    var element = node as XmlElement;
                    if (element == null || !String.Equals(element.Name, "VST", StringComparison.OrdinalIgnoreCase)) continue;
                    string dll = element.GetAttribute("PluginDll");
                    if (String.IsNullOrWhiteSpace(dll)) continue;
                    string full = Path.IsPathRooted(dll) ? dll : Path.Combine(runtime, "musicplayer", "plugins", "dsp", dll);
                    if (File.Exists(full)) vst.Add(full);
                }
            }
            using (var dialog = new DspTestDialog(scale, runtime, gain, damp, dampPreset, compressor, threshold, vst)) dialog.ShowDialog(FindForm());
        }

        private Control BuildEffectCard(XmlElement effect, string kind)
        {
            var card = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(10)), Padding = new Padding(Px(12), Px(8), Px(12), Px(10)) };
            var header = new TableLayoutPanel { ColumnCount = 3, RowCount = 1, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(100)));
            header.Controls.Add(new Label { Text = L("Effekt", "Effect"), Font = Typography.Create(9f * scale, FontStyle.Regular), ForeColor = palette.Muted, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            var name = new StudioTextInput(scale, Typography.Create(9.5f * scale, FontStyle.Regular), palette) { Colors = () => palette, Dock = DockStyle.Fill };
            name.Text = effect.GetAttribute("EffectName");
            name.Box.TextChanged += delegate { if (effect.GetAttribute("EffectName") != name.Text) { effect.SetAttribute("EffectName", name.Text); MarkDirty(); } };
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
            var dll = new StudioTextInput(scale, Typography.Create(9.5f * scale, FontStyle.Regular), palette) { Colors = () => palette, Dock = DockStyle.Fill };
            dll.Text = plugin.GetAttribute("PluginDll");
            dll.Box.TextChanged += delegate { if (plugin.GetAttribute("PluginDll") != dll.Text) { plugin.SetAttribute("PluginDll", dll.Text); MarkDirty(); } };
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
            var remove = new StudioButton { Text = "×", Font = Typography.Create(10f * scale, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Muted, Dock = DockStyle.Fill, Margin = new Padding(Px(4), 0, 0, 0) };
            remove.FlatAppearance.BorderColor = palette.Border;
            remove.Click += delegate { parameter.ParentNode.RemoveChild(parameter); MarkDirty(); BuildCards(); };
            row.Controls.Add(remove, withIndex ? 3 : 2, 0);
            return row;
        }

        private StudioTextInput Input(string value, Action<string> changed)
        {
            var box = new StudioTextInput(scale, Typography.Create(9f * scale, FontStyle.Regular), palette) { Colors = () => palette, Dock = DockStyle.Fill, Margin = new Padding(0, 0, Px(4), 0) };
            box.Text = value ?? string.Empty;
            box.Box.TextChanged += delegate { changed(box.Text); };
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

        private void BrowseDll(StudioTextInput target)
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
            if (MessageBox.Show(FindForm(), (Localization.T("Diesen Eintrag entfernen?")), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
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
                status.Text = (Localization.T("Gespeichert · Sicherung: ")) + Path.GetFileName(backup);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.T("Die Musik-DSP-Einstellungen konnten nicht gespeichert werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private sealed class PluginPickerDialog : Form
        {
            private readonly ListBox list = new ListBox();
            private readonly float scale;
            public string SelectedFile { get { return list.SelectedItem == null ? null : list.SelectedItem.ToString(); } }

            public PluginPickerDialog(float scale, string kind, string[] files)
            {
                this.scale = scale;
                Text = (Localization.T("Auswählen: ")) + kind;
                FormBorderStyle = FormBorderStyle.Sizable;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(Px(620), Px(420));
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                list.Dock = DockStyle.Fill;
                list.Font = Typography.Create(10f * scale, FontStyle.Regular);
                list.BackColor = Palette.For(true).Input;
                list.ForeColor = Palette.For(true).Text;
                list.IntegralHeight = false;
                foreach (string file in files) list.Items.Add(file);
                if (list.Items.Count > 0) list.SelectedIndex = 0;
                var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = Px(48), FlowDirection = FlowDirection.RightToLeft, BackColor = Palette.For(true).Panel, Padding = new Padding(Px(10), Px(8), Px(10), Px(8)) };
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate { if (list.SelectedItem == null) return; DialogResult = DialogResult.OK; Close(); };
                bottom.Controls.Add(cancel);
                bottom.Controls.Add(ok);
                Controls.Add(list);
                Controls.Add(bottom);
                AcceptButton = ok;
                CancelButton = cancel;
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }
        }

        private sealed class DspTestDialog : Form
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern bool SetDllDirectory(string path);
            [DllImport("bass.dll")]
            private static extern bool BASS_Init(int device, uint frequency, uint flags, IntPtr window, IntPtr clsid);
            [DllImport("bass.dll")]
            private static extern bool BASS_Free();
            [DllImport("bass.dll", CharSet = CharSet.Unicode)]
            private static extern int BASS_StreamCreateFile(bool memory, string file, long offset, long length, uint flags);
            [DllImport("bass.dll")]
            private static extern bool BASS_StreamFree(int handle);
            [DllImport("bass.dll")]
            private static extern bool BASS_ChannelPlay(int handle, bool restart);
            [DllImport("bass.dll")]
            private static extern bool BASS_ChannelStop(int handle);
            [DllImport("bass.dll")]
            private static extern bool BASS_ChannelSetAttribute(int handle, uint attribute, float value);
            [DllImport("bass.dll")]
            private static extern int BASS_ErrorGetCode();
            [DllImport("bass.dll", CharSet = CharSet.Unicode)]
            private static extern int BASS_PluginLoad(string file, uint flags);
            [DllImport("bass.dll")]
            private static extern int BASS_ChannelSetFX(int handle, int type, int priority);
            [DllImport("bass.dll")]
            private static extern bool BASS_FXSetParameters(int handle, ref BASS_BFX_DAMP parameters);
            [DllImport("bass.dll")]
            private static extern bool BASS_FXSetParameters(int handle, ref BASS_BFX_COMPRESSOR parameters);
            [DllImport("bass_vst.dll", CharSet = CharSet.Unicode)]
            private static extern int BASS_VST_ChannelSetDSP(int channel, string dllFile, int flags, int priority);

            [StructLayout(LayoutKind.Sequential)]
            private struct BASS_BFX_DAMP
            {
                public float fTarget;
                public float fQuiet;
                public float fRate;
                public float fGain;
                public float fDelay;
                public int lChannel;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct BASS_BFX_COMPRESSOR
            {
                public float fThreshold;
                public float fAttacktime;
                public float fReleasetime;
                public int lChannel;
            }

            private readonly float scale;
            private readonly string runtime;
            private readonly double gain;
            private readonly bool damp;
            private readonly int dampPreset;
            private readonly bool compressor;
            private readonly int compressorThreshold;
            private readonly List<string> vstPlugins;
            private readonly TextBox file = new TextBox();
            private readonly Label status = new Label();
            private readonly Button play;
            private readonly Button stop;
            private bool initialized;
            private int stream;
            private bool fxLoaded;
            private bool vstLoaded;

            public DspTestDialog(float scale, string runtime, double gain, bool damp, int dampPreset, bool compressor, int compressorThreshold, List<string> vstPlugins)
            {
                this.scale = scale;
                this.runtime = runtime;
                this.gain = gain;
                this.damp = damp;
                this.dampPreset = dampPreset;
                this.compressor = compressor;
                this.compressorThreshold = compressorThreshold;
                this.vstPlugins = vstPlugins ?? new List<string>();
                Text = Localization.T("DSP-Testwiedergabe");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(620), Px(240));
                var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(40), ColumnCount = 2, Padding = new Padding(Px(14), Px(10), Px(14), 0), BackColor = Palette.For(true).Panel };
                top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(110)));
                file.Dock = DockStyle.Fill;
                file.Font = Typography.Create(10f * scale, FontStyle.Regular);
                file.BackColor = Palette.For(true).Input;
                file.ForeColor = Palette.For(true).Text;
                file.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                top.Controls.Add(file, 0, 0);
                var browse = new StudioButton { Text = Localization.T("Datei …"), FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Dock = DockStyle.Fill };
                browse.FlatAppearance.BorderColor = Palette.For(true).Border;
                browse.Click += delegate
                {
                    using (var dialog = new OpenFileDialog { Title = Localization.T("Testdatei wählen"), Filter = "Audio (*.mp3;*.wav;*.flac;*.ogg;*.m4a)|*.mp3;*.wav;*.flac;*.ogg;*.m4a|All files (*.*)|*.*", CheckFileExists = true })
                    {
                        if (Directory.Exists(Path.GetDirectoryName(file.Text))) dialog.InitialDirectory = Path.GetDirectoryName(file.Text);
                        if (dialog.ShowDialog(this) == DialogResult.OK) file.Text = dialog.FileName;
                    }
                };
                top.Controls.Add(browse, 1, 0);

                var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(48), FlowDirection = FlowDirection.LeftToRight, BackColor = Palette.For(true).Panel, Padding = new Padding(Px(14), 0, Px(14), 0) };
                play = new StudioButton { Text = Localization.T2("Wiedergabe", "Play"), FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(130), Height = Px(30) };
                play.FlatAppearance.BorderColor = Palette.For(true).Border;
                play.Click += delegate { StartPlayback(); };
                stop = new StudioButton { Text = Localization.T("Stopp"), FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30), Enabled = false };
                stop.FlatAppearance.BorderColor = Palette.For(true).Border;
                stop.Click += delegate { StopPlayback(); };
                actions.Controls.Add(play);
                actions.Controls.Add(stop);
                var close = new StudioButton { Text = Localization.T("Schließen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                close.FlatAppearance.BorderColor = Palette.For(true).Border;
                actions.Controls.Add(close);
                status.AutoSize = true;
                status.ForeColor = Palette.For(true).Muted;
                status.Font = Typography.Create(9f * scale, FontStyle.Regular);
                status.Text = (Localization.T("Gain: ")) + gain.ToString("0.0", CultureInfo.InvariantCulture) + " dB  ·  " + (damp ? "DynAmp " : "") + (compressor ? "Compressor " : "") + (this.vstPlugins.Count > 0 ? this.vstPlugins.Count + " VST " : "") + (Localization.T("in dieser Vorschau aktiv"));
                actions.Controls.Add(status);
                Controls.Add(actions);
                Controls.Add(top);
                CancelButton = close;
                FormClosing += delegate { StopPlayback(); };
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }

            private static bool LoadBassFx(string runtime)
            {
                try
                {
                    string assemblyPath = Path.Combine(runtime, "Bass.Net.dll");
                    if (!File.Exists(assemblyPath)) return false;
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    Type fx = assembly.GetType("Un4seen.Bass.AddOn.Fx.BassFx");
                    if (fx == null) return false;
                    MethodInfo load = fx.GetMethod("LoadMe", Type.EmptyTypes);
                    if (load == null) return false;
                    object result = load.Invoke(null, null);
                    return result is bool && (bool)result;
                }
                catch (Exception ex)
                {
                    StudioLog.Warn("DSP test: bass_fx load failed: " + ex.Message);
                    return false;
                }
            }

            private static bool LoadBassVst(string runtime)
            {
                try
                {
                    string assemblyPath = Path.Combine(runtime, "Bass.Net.dll");
                    string dllPath = Path.Combine(runtime, "bass_vst.dll");
                    if (!File.Exists(assemblyPath) || !File.Exists(dllPath)) return false;
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    Type vst = assembly.GetType("Un4seen.Bass.AddOn.Vst.BassVst");
                    if (vst == null) return false;
                    MethodInfo load = vst.GetMethod("LoadMe", Type.EmptyTypes);
                    if (load == null) return false;
                    object result = load.Invoke(null, null);
                    return result is bool && (bool)result;
                }
                catch (Exception ex)
                {
                    StudioLog.Warn("DSP test: bass_vst load failed: " + ex.Message);
                    return false;
                }
            }

            private static bool LooksLikePlugin(string file)
            {
                try
                {
                    if (!File.Exists(file)) return false;
                    var info = new FileInfo(file);
                    if (info.Length < 8 * 1024) return false;
                    using (var stream = File.OpenRead(file))
                    {
                        return stream.ReadByte() == 'M' && stream.ReadByte() == 'Z';
                    }
                }
                catch { return false; }
            }

            private static BASS_BFX_DAMP DampParameters(int preset)
            {
                var parameters = new BASS_BFX_DAMP { lChannel = -1 };
                switch (preset)
                {
                    case 0:
                        parameters.fTarget = 0.92f;
                        parameters.fQuiet = 0.02f;
                        parameters.fRate = 0.01f;
                        parameters.fGain = 1f;
                        parameters.fDelay = 0.5f;
                        break;
                    case 2:
                        parameters.fTarget = 0.98f;
                        parameters.fQuiet = 0.04f;
                        parameters.fRate = 0.02f;
                        parameters.fGain = 2f;
                        parameters.fDelay = 0.2f;
                        break;
                    default:
                        parameters.fTarget = 0.94f;
                        parameters.fQuiet = 0.03f;
                        parameters.fRate = 0.01f;
                        parameters.fGain = 1f;
                        parameters.fDelay = 0.35f;
                        break;
                }
                return parameters;
            }

            private void StartPlayback()
            {
                StopPlayback();
                if (!File.Exists(file.Text))
                {
                    status.Text = Localization.T("Datei nicht gefunden.");
                    return;
                }
                try
                {
                    SetDllDirectory(runtime);
                    if (!initialized)
                    {
                        if (!BASS_Init(-1, 44100, 0, IntPtr.Zero, IntPtr.Zero))
                        {
                            status.Text = (Localization.T("BASS konnte nicht initialisieren (Fehler ")) + BASS_ErrorGetCode() + ").";
                            return;
                        }
                        initialized = true;
                    }
                    if (!fxLoaded)
                    {
                        fxLoaded = LoadBassFx(runtime);
                        StudioLog.Info("DSP test: bass_fx loaded=" + fxLoaded);
                    }
                    stream = BASS_StreamCreateFile(false, file.Text, 0, 0, 0x100 | 0x40000 | 0x4 | 0x80000000);
                    if (stream == 0)
                    {
                        status.Text = (Localization.T("Audiodatei kann nicht geöffnet werden (BASS-Fehler ")) + BASS_ErrorGetCode() + ").";
                        return;
                    }
                    BASS_ChannelSetAttribute(stream, 2, (float)Math.Pow(10, gain / 20.0));
                    string applied = "Gain";
                    if (damp)
                    {
                        var parameters = DampParameters(dampPreset);
                        int handle = BASS_ChannelSetFX(stream, 65544, 1);
                        if (handle != 0 && BASS_FXSetParameters(handle, ref parameters)) applied += ", DynAmp";
                        else applied += ", DynAmp error " + BASS_ErrorGetCode();
                    }
                    if (compressor)
                    {
                        double level = Math.Pow(10, (compressorThreshold / 10.0) / 20.0);
                        var parameters = new BASS_BFX_COMPRESSOR
                        {
                            fThreshold = (float)Math.Max(0.001, Math.Min(1.0, level)),
                            fAttacktime = 5f,
                            fReleasetime = 250f,
                            lChannel = -1
                        };
                        int handle = BASS_ChannelSetFX(stream, 65551, 2);
                        if (handle != 0 && BASS_FXSetParameters(handle, ref parameters)) applied += ", Compressor";
                        else applied += ", Compressor error " + BASS_ErrorGetCode();
                    }
                    if (vstPlugins.Count > 0)
                    {
                        if (!vstLoaded)
                        {
                            vstLoaded = LoadBassVst(runtime);
                            StudioLog.Info("DSP test: bass_vst loaded=" + vstLoaded + " plugins=" + vstPlugins.Count);
                        }
                        if (!vstLoaded)
                        {
                            applied += ", VST unavailable";
                        }
                        else
                        {
                            int priority = 3;
                            foreach (string plugin in vstPlugins)
                            {
                                string name = Path.GetFileNameWithoutExtension(plugin);
                                if (!LooksLikePlugin(plugin))
                                {
                                    applied += ", VST skipped " + name;
                                    StudioLog.Warn("DSP test: VST skipped, not a plugin DLL: " + plugin);
                                    continue;
                                }
                                try
                                {
                                    StudioLog.Info("DSP test: applying VST " + plugin);
                                    int handle = BASS_VST_ChannelSetDSP(stream, plugin, unchecked((int)0x80000000), priority++);
                                    StudioLog.Info("DSP test: VST " + name + " handle=" + handle + " error=" + BASS_ErrorGetCode());
                                    if (handle != 0) applied += ", VST " + name;
                                    else applied += ", VST error " + BASS_ErrorGetCode();
                                }
                                catch (Exception ex)
                                {
                                    applied += ", VST error " + name;
                                    StudioLog.Warn("DSP test: VST failed: " + ex.Message);
                                }
                            }
                        }
                    }
                    BASS_ChannelPlay(stream, false);
                    play.Enabled = false;
                    stop.Enabled = true;
                    status.Text = (Localization.T("Wiedergabe mit ")) + applied + " …";
                }
                catch (Exception ex)
                {
                    status.Text = ex.Message;
                }
            }

            private void StopPlayback()
            {
                try
                {
                    if (stream != 0)
                    {
                        BASS_ChannelStop(stream);
                        BASS_StreamFree(stream);
                        stream = 0;
                    }
                    if (initialized)
                    {
                        BASS_Free();
                        initialized = false;
                    }
                }
                catch { }
                if (play != null) play.Enabled = true;
                if (stop != null) stop.Enabled = false;
            }
        }
    }
}
