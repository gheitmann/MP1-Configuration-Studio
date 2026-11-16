using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeXmlFileEditor : UserControl, IStudioFileEditor
    {
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly Dictionary<string, string> files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly StudioChoice choice;
        private readonly TextBox editor = new TextBox();
        private readonly Label status = new Label();
        private readonly string configDirectory;
        private string currentPath;
        private bool dirty;
        private Encoding fileEncoding = new UTF8Encoding(false);
        private bool suppress;

        public NativeXmlFileEditor(string configDirectory, string runtimeDirectory, float scale)
        {
            this.scale = scale;
            this.configDirectory = configDirectory;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);
            Add("keymap.xml", Path.Combine(configDirectory, "keymap.xml"), true);
            Add("MusicViews.xml", Path.Combine(configDirectory, "MusicViews.xml"), true);
            Add("VideoViews.xml", Path.Combine(configDirectory, "VideoViews.xml"), true);
            Add("musicdsp.xml", Path.Combine(configDirectory, "musicdsp.xml"), true);
            Add("MediaPortalDirs.xml", Path.Combine(runtimeDirectory, "MediaPortalDirs.xml"), true);
            Add("Gentle.config", Path.Combine(configDirectory, "Gentle.config"), false);
            Add("OnlineVideoSites.xml", Path.Combine(configDirectory, "OnlineVideoSites.xml"), false);
            Add("LatestMediaHandler.xml", Path.Combine(configDirectory, "LatestMediaHandler.xml"), false);
            Add("InfoService.xml", Path.Combine(configDirectory, "InfoService.xml"), false);
            Add("SubCentral.xml", Path.Combine(configDirectory, "SubCentral.xml"), false);
            Add("LogoManager.config", Path.Combine(configDirectory, "LogoManager.config"), false);
            Add("BackdropMenuEditor.xml", Path.Combine(configDirectory, "BackdropMenuEditor.xml"), false);
            Add("wikipedia.xml", Path.Combine(configDirectory, "wikipedia.xml"), false);
            Add("FanartHandler.xml", Path.Combine(configDirectory, "FanartHandler.xml"), false);
            Add("IncompatiblePlugins.xml", Path.Combine(configDirectory, "IncompatiblePlugins.xml"), false);
            foreach (string calibration in Directory.GetFiles(configDirectory, "ScreenCalibration*.xml").OrderBy(name => name))
                Add(Path.GetFileName(calibration), calibration, false);

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 2, BackColor = palette.Background, Padding = new Padding(0) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(180)));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.Controls.Add(new Label { Text = Localization.T("XML-Datei"), ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            choice = new StudioChoice(scale) { Dock = DockStyle.Fill, Font = body, Colors = () => palette, AccessibleName = Localization.T("XML-Datei") };
            choice.SetOptions(files.Keys.Select(name => new ChoiceOption(name, name)), files.Keys.FirstOrDefault() ?? String.Empty);
            choice.SelectedIndexChanged += delegate { SelectionChanged(); };
            top.Controls.Add(choice, 1, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(38), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            var reload = ButtonFor(Localization.T("Neu laden"), delegate { LoadFile(); }, body);
            var save = ButtonFor(Localization.T("Datei sichern"), delegate { SaveFile(); }, body);
            var browse = ButtonFor(Localization.T("Andere Datei …"), delegate { BrowseFile(); }, body);
            actions.Controls.Add(reload);
            actions.Controls.Add(save);
            actions.Controls.Add(browse);
            status.AutoSize = false;
            status.Width = Px(140);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            actions.Controls.Add(status);

            editor.Dock = DockStyle.Fill;
            editor.Multiline = true;
            editor.AcceptsReturn = true;
            editor.AcceptsTab = true;
            editor.WordWrap = false;
            editor.ScrollBars = ScrollBars.Both;
            editor.Font = new Font("Consolas", 10f * scale, FontStyle.Regular, GraphicsUnit.Point);
            editor.BackColor = palette.Input;
            editor.ForeColor = palette.Text;
            editor.BorderStyle = BorderStyle.FixedSingle;
            editor.TextChanged += delegate { if (!suppress) dirty = true; };

            Controls.Add(editor);
            Controls.Add(actions);
            Controls.Add(top);
            if (choice.Count > 0)
            {
                suppress = true;
                choice.SelectedIndex = 0;
                suppress = false;
                currentPath = files[choice.SelectedValue];
            }
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            return ConfirmDiscard();
        }

        public bool SaveChanges()
        {
            SaveFile();
            return !dirty;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void Add(string display, string path, bool required)
        {
            if (required || File.Exists(path)) files[display] = path;
        }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(100), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private string DisplayName(string path)
        {
            foreach (var pair in files)
                if (String.Equals(pair.Value, path, StringComparison.OrdinalIgnoreCase)) return pair.Key;
            return null;
        }

        private void SelectionChanged()
        {
            if (suppress || String.IsNullOrEmpty(choice.SelectedValue)) return;
            string next = files[choice.SelectedValue];
            if (String.Equals(next, currentPath, StringComparison.OrdinalIgnoreCase)) return;
            if (!ConfirmDiscard())
            {
                suppress = true;
                string previous = DisplayName(currentPath);
                if (previous != null) choice.SelectValue(previous);
                suppress = false;
                return;
            }
            currentPath = next;
            LoadFile();
        }

        private bool ConfirmDiscard()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.T("Ungespeicherte XML-Änderungen verwerfen?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void BrowseFile()
        {
            if (!ConfirmDiscard()) return;
            using (var dialog = new OpenFileDialog { Title = Localization.T("XML- oder Konfigurationsdatei öffnen"), InitialDirectory = configDirectory, Filter = "XML / Konfiguration (*.xml;*.config)|*.xml;*.config|Alle Dateien (*.*)|*.*", CheckFileExists = true })
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                currentPath = dialog.FileName;
                string display = DisplayName(currentPath);
                if (display == null)
                {
                    display = Path.GetFileName(currentPath);
                    if (files.ContainsKey(display)) display = Path.GetFileName(currentPath) + "  ·  " + Path.GetDirectoryName(currentPath);
                    files[display] = currentPath;
                    suppress = true;
                    choice.SetOptions(files.Keys.Select(name => new ChoiceOption(name, name)), display);
                    suppress = false;
                }
                else
                {
                    suppress = true;
                    choice.SelectValue(display);
                    suppress = false;
                }
                LoadFile();
            }
        }

        private void LoadFile()
        {
            try
            {
                suppress = true;
                if (currentPath != null && File.Exists(currentPath))
                {
                    fileEncoding = new UTF8Encoding(false);
                    editor.Text = NativeXmlFileStore.ReadText(currentPath, out fileEncoding);
                    editor.Select(0, 0);
                    status.Text = currentPath;
                    status.Tag = currentPath;
                }
                else
                {
                    fileEncoding = new UTF8Encoding(false);
                    editor.Text = string.Empty;
                    status.Text = (currentPath ?? string.Empty) + (Localization.T("  ·  neue Datei"));
                }
                dirty = false;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
            finally { suppress = false; }
        }

        private void SaveFile()
        {
            if (currentPath == null) return;
            try
            {
                string backup = NativeXmlFileStore.SaveText(currentPath, editor.Text, fileEncoding);
                dirty = false;
                status.Text = (Localization.T("Gespeichert · Sicherung: ")) + Path.GetFileName(backup);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.T("Die XML-Datei konnte nicht gespeichert werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
