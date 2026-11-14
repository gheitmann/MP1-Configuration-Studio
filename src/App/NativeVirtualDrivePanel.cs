using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeVirtualDrivePanel : UserControl
    {
        private readonly StudioConfiguration configuration;
        private readonly string runtime;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly CheckBox enabled = new CheckBox();
        private readonly TextBox path = new TextBox();
        private readonly TextBox extensions = new TextBox();
        private readonly NumericUpDown drive = new NumericUpDown();
        private readonly CheckBox ask = new CheckBox();
        private readonly Label status = new Label();
        private readonly Label detection = new Label();
        private readonly Button applyDetected;
        private bool loading;

        public event EventHandler Changed;

        public NativeVirtualDrivePanel(StudioConfiguration configuration, string runtime, float scale)
        {
            this.configuration = configuration;
            this.runtime = runtime;
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, BackColor = palette.Background, Padding = new Padding(0) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            for (int i = 0; i < 6; i++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(34)));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            enabled.Text = Localization.IsEnglish ? "Enable virtual drive" : "Virtuelles Laufwerk aktivieren";
            enabled.AutoSize = true;
            enabled.Font = body;
            enabled.ForeColor = palette.Text;
            enabled.BackColor = palette.Background;
            enabled.FlatStyle = FlatStyle.Flat;
            enabled.Margin = new Padding(0, Px(6), 0, 0);
            enabled.CheckedChanged += delegate { if (!loading) { configuration.SetBool("daemon", "enabled", enabled.Checked); RaiseChanged(); } };
            table.Controls.Add(enabled, 0, 0);
            table.SetColumnSpan(enabled, 2);

            AddLabel(table, 1, Localization.IsEnglish ? "Program path" : "Programmpfad");
            var pathRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = palette.Background, Margin = new Padding(0) };
            pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(96)));
            path.Dock = DockStyle.Fill;
            path.Font = body;
            path.BackColor = palette.Input;
            path.ForeColor = palette.Text;
            path.BorderStyle = BorderStyle.FixedSingle;
            path.Margin = new Padding(0, Px(3), Px(6), Px(3));
            path.TextChanged += delegate { if (!loading) { configuration.Set("daemon", "path", path.Text); RaiseChanged(); } };
            pathRow.Controls.Add(path, 0, 0);
            var browse = new Button { Text = Localization.IsEnglish ? "Browse …" : "Durchsuchen …", Font = body, Height = Px(28), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Dock = DockStyle.Fill, Margin = new Padding(0, Px(3), 0, Px(3)) };
            browse.FlatAppearance.BorderColor = palette.Border;
            browse.Click += delegate
            {
                using (var dialog = new OpenFileDialog { Title = Localization.IsEnglish ? "Select virtual drive program" : "Programm für virtuelles Laufwerk wählen", Filter = Localization.IsEnglish ? "Programs (*.exe)|*.exe|All files (*.*)|*.*" : "Programme (*.exe)|*.exe|Alle Dateien (*.*)|*.*", CheckFileExists = true })
                {
                    if (File.Exists(path.Text)) dialog.InitialDirectory = Path.GetDirectoryName(path.Text);
                    if (dialog.ShowDialog(this) == DialogResult.OK) path.Text = dialog.FileName;
                }
            };
            pathRow.Controls.Add(browse, 1, 0);
            table.Controls.Add(pathRow, 1, 1);

            AddLabel(table, 2, Localization.IsEnglish ? "Supported extensions" : "Unterstützte Endungen");
            extensions.Dock = DockStyle.Fill;
            extensions.Font = body;
            extensions.BackColor = palette.Input;
            extensions.ForeColor = palette.Text;
            extensions.BorderStyle = BorderStyle.FixedSingle;
            extensions.Margin = new Padding(0, Px(3), 0, Px(3));
            extensions.TextChanged += delegate { if (!loading) { configuration.Set("daemon", "extensions", extensions.Text); RaiseChanged(); } };
            table.Controls.Add(extensions, 1, 2);

            AddLabel(table, 3, Localization.IsEnglish ? "Drive number" : "Laufwerksnummer");
            drive.Dock = DockStyle.Fill;
            drive.Font = body;
            drive.Minimum = 0;
            drive.Maximum = 25;
            drive.BackColor = palette.Input;
            drive.ForeColor = palette.Text;
            drive.BorderStyle = BorderStyle.FixedSingle;
            drive.Margin = new Padding(0, Px(3), 0, Px(3));
            drive.ValueChanged += delegate { if (!loading) { configuration.SetInt("daemon", "driveNo", (int)drive.Value); RaiseChanged(); } };
            table.Controls.Add(drive, 1, 3);

            ask.Text = Localization.IsEnglish ? "Ask before playing" : "Vor Wiedergabe fragen";
            ask.AutoSize = true;
            ask.Font = body;
            ask.ForeColor = palette.Text;
            ask.BackColor = palette.Background;
            ask.FlatStyle = FlatStyle.Flat;
            ask.Margin = new Padding(0, Px(6), 0, 0);
            ask.CheckedChanged += delegate { if (!loading) { configuration.SetBool("daemon", "askbeforeplaying", ask.Checked); RaiseChanged(); } };
            table.Controls.Add(ask, 0, 4);
            table.SetColumnSpan(ask, 2);

            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Margin = new Padding(0, Px(2), 0, Px(2)) };
            var check = new Button { Text = Localization.IsEnglish ? "Check path" : "Pfad prüfen", Font = body, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            check.FlatAppearance.BorderColor = palette.Border;
            check.Click += delegate { CheckPath(); };
            applyDetected = new Button { Text = Localization.IsEnglish ? "Apply detected path" : "Erkannten Pfad übernehmen", Font = body, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0), Enabled = false };
            applyDetected.FlatAppearance.BorderColor = palette.Border;
            applyDetected.Click += delegate
            {
                string detected = Detect();
                if (detected == null) return;
                path.Text = detected;
                status.Text = (Localization.IsEnglish ? "Detected path applied: " : "Erkannter Pfad übernommen: ") + detected;
                status.ForeColor = palette.Text;
            };
            actions.Controls.Add(check);
            actions.Controls.Add(applyDetected);
            table.Controls.Add(actions, 0, 5);
            table.SetColumnSpan(actions, 2);

            detection.AutoSize = true;
            detection.Font = Typography.Create(9f * scale, FontStyle.Regular);
            detection.ForeColor = palette.Muted;
            detection.MaximumSize = new Size(Px(760), 0);
            detection.Margin = new Padding(0, Px(4), 0, Px(2));
            table.Controls.Add(detection, 0, 6);
            table.SetColumnSpan(detection, 2);

            status.AutoSize = true;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            status.Margin = new Padding(0);
            table.Controls.Add(status, 0, 7);
            table.SetColumnSpan(status, 2);
            table.RowCount = 8;

            Controls.Add(table);
            LoadState();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void AddLabel(TableLayoutPanel table, int row, string text)
        {
            table.Controls.Add(new Label { Text = text, Font = Typography.Create(9.5f * scale, FontStyle.Regular), ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Dock = DockStyle.Fill, UseMnemonic = false, Margin = new Padding(0, 0, Px(10), 0) }, 0, row);
        }

        private void LoadState()
        {
            loading = true;
            try
            {
                enabled.Checked = configuration.GetBool("daemon", "enabled", false);
                path.Text = configuration.Get("daemon", "path", string.Empty);
                extensions.Text = configuration.Get("daemon", "extensions", ".cue,.bin,.iso,.ccd,.bwt,.mds,.cdi,.nrg,.pdi,.b5t,.img");
                int number;
                if (!Int32.TryParse(configuration.Get("daemon", "driveNo", "0"), out number)) number = 0;
                drive.Value = Math.Max(drive.Minimum, Math.Min(drive.Maximum, number));
                ask.Checked = configuration.GetBool("daemon", "askbeforeplaying", false);
            }
            finally { loading = false; }
            RefreshDetection();
        }

        private void RefreshDetection()
        {
            string detected = Detect();
            applyDetected.Enabled = detected != null && !String.Equals(detected, path.Text, StringComparison.OrdinalIgnoreCase);
            detection.Text = detected == null
                ? (Localization.IsEnglish ? "Daemon Tools was not detected on this system. Select the program manually." : "Daemon Tools wurde auf diesem System nicht erkannt. Programm manuell auswählen.")
                : (Localization.IsEnglish ? "Detected: " : "Erkannt: ") + detected;
            detection.ForeColor = detected == null ? palette.Muted : palette.Text;
        }

        private void CheckPath()
        {
            string value = path.Text.Trim();
            if (value.Length == 0)
            {
                status.Text = Localization.IsEnglish ? "No path configured." : "Kein Pfad konfiguriert.";
                status.ForeColor = palette.Muted;
                return;
            }
            if (!File.Exists(value))
            {
                status.Text = (Localization.IsEnglish ? "File not found: " : "Datei nicht gefunden: ") + value;
                status.ForeColor = palette.Muted;
                return;
            }
            var info = new FileInfo(value);
            status.Text = (Localization.IsEnglish ? "Path is valid · " : "Pfad ist gültig · ") + info.Length.ToString("N0") + (Localization.IsEnglish ? " bytes" : " Bytes");
            status.ForeColor = palette.Text;
            RefreshDetection();
        }

        private void RaiseChanged()
        {
            RefreshDetection();
            var handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private static string Detect()
        {
            string[] keys =
            {
                @"SOFTWARE\DAEMON Tools Lite",
                @"SOFTWARE\DAEMON Tools Pro",
                @"SOFTWARE\DiscSoft\DAEMON Tools Lite",
                @"SOFTWARE\DiscSoft\DAEMON Tools Pro"
            };
            string[] names = { "InstallPath", "InstallDir", "Path", "TargetPath", "ProgramPath" };
            string[] executables = { "DTLite.exe", "DAEMON.exe", "daemon.exe", "DTAgent.exe" };
            foreach (RegistryHive hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
            {
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    try
                    {
                        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
                        {
                            foreach (string keyName in keys)
                            {
                                using (RegistryKey key = baseKey.OpenSubKey(keyName, false))
                                {
                                    if (key == null) continue;
                                    foreach (string name in names)
                                    {
                                        string directory = key.GetValue(name) as string;
                                        string found = Probe(directory, executables);
                                        if (found != null) return found;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            foreach (string folder in DefaultFolders())
            {
                string found = Probe(folder, executables);
                if (found != null) return found;
            }
            return null;
        }

        private static IEnumerable<string> DefaultFolders()
        {
            var folders = new List<string>();
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            foreach (string root in new[] { programFiles, programFilesX86 })
            {
                if (String.IsNullOrWhiteSpace(root)) continue;
                folders.Add(Path.Combine(root, "DAEMON Tools Lite"));
                folders.Add(Path.Combine(root, "DAEMON Tools Pro"));
                folders.Add(Path.Combine(root, "DAEMON Tools"));
            }
            return folders;
        }

        private static string Probe(string directory, string[] executables)
        {
            if (String.IsNullOrWhiteSpace(directory)) return null;
            directory = directory.Trim().Trim('"');
            try
            {
                foreach (string executable in executables)
                {
                    string candidate = Path.Combine(directory, executable);
                    if (File.Exists(candidate)) return candidate;
                }
                if (File.Exists(directory) && String.Equals(Path.GetExtension(directory), ".exe", StringComparison.OrdinalIgnoreCase)) return directory;
            }
            catch { }
            return null;
        }
    }
}
