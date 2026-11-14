using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativePluginConfigPanel : UserControl
    {
        private sealed class PluginInfo
        {
            public string Name;
            public string Dll;
            public string Type;
            public override string ToString() { return Name + "  ·  " + Path.GetFileName(Dll); }
        }

        private readonly string runtime;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly Action classic;
        private readonly ComboBox choice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
        private readonly Label status = new Label();
        private readonly Button configure;
        private readonly List<PluginInfo> plugins = new List<PluginInfo>();
        private bool loading;

        public NativePluginConfigPanel(string runtime, float scale, Action classic)
        {
            this.runtime = runtime;
            this.scale = scale;
            this.classic = classic;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, BackColor = palette.Background, Padding = new Padding(0) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(150)));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(34)));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.Controls.Add(new Label { Text = Localization.IsEnglish ? "Plugin" : "Plugin", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            choice.Dock = DockStyle.Fill;
            choice.Font = body;
            choice.BackColor = palette.Input;
            choice.ForeColor = palette.Text;
            table.Controls.Add(choice, 1, 0);

            var actions = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Background, Margin = new Padding(0) };
            configure = ButtonFor(Localization.IsEnglish ? "Configure" : "Konfigurieren", delegate { ConfigurePlugin(); }, body);
            configure.Enabled = false;
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Refresh" : "Aktualisieren", delegate { LoadPlugins(); }, body));
            actions.Controls.Add(configure);
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Classic configuration" : "Klassische Konfiguration", delegate { if (classic != null) classic(); }, body));
            table.Controls.Add(actions, 0, 1);
            table.SetColumnSpan(actions, 2);

            status.AutoSize = true;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            status.Margin = new Padding(0, 0, 0, Px(4));
            table.Controls.Add(status, 0, 2);
            table.SetColumnSpan(status, 2);

            var note = new Label
            {
                Text = Localization.IsEnglish
                    ? "Plugin windows use the plugin's own UI. Loading a plugin can take a moment; failures are isolated and can be retried or opened through the classic configuration."
                    : "Plugin-Fenster behalten ihre eigene Oberfläche. Das Laden kann einen Moment dauern; Fehler bleiben isoliert und können erneut versucht oder über die klassische Konfiguration geöffnet werden.",
                Font = Typography.Create(9f * scale, FontStyle.Regular),
                ForeColor = palette.Muted,
                AutoSize = true,
                MaximumSize = new Size(Px(740), 0),
                Margin = new Padding(0, Px(4), 0, 0)
            };
            table.Controls.Add(note, 0, 3);
            table.SetColumnSpan(note, 2);
            Controls.Add(table);
            LoadPlugins();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button { Text = text, Font = font, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LoadPlugins()
        {
            if (loading) return;
            loading = true;
            choice.Items.Clear();
            plugins.Clear();
            configure.Enabled = false;
            status.Text = Localization.IsEnglish ? "Reading plugin list …" : "Plugin-Liste wird gelesen …";
            status.ForeColor = palette.Muted;
            ThreadPool.QueueUserWorkItem(delegate
            {
                var found = ReadPlugins();
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        BeginInvoke(new Action(delegate
                        {
                            plugins.Clear();
                            plugins.AddRange(found);
                            choice.Items.Clear();
                            foreach (PluginInfo plugin in plugins) choice.Items.Add(plugin);
                            if (choice.Items.Count > 0) choice.SelectedIndex = 0;
                            configure.Enabled = choice.Items.Count > 0;
                            status.Text = plugins.Count == 0
                                ? (Localization.IsEnglish ? "No plugin with a configuration window found." : "Kein Plugin mit Konfigurationsfenster gefunden.")
                                : plugins.Count + (Localization.IsEnglish ? " configurable plugins" : " konfigurierbare Plugins");
                            status.ForeColor = plugins.Count == 0 ? palette.Muted : palette.Text;
                            loading = false;
                        }));
                }
                catch { loading = false; }
            });
        }

        private List<PluginInfo> ReadPlugins()
        {
            var result = new List<PluginInfo>();
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-list \"" + runtime + "\"",
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
                    if (!process.WaitForExit(30000))
                    {
                        try { process.Kill(); } catch { }
                        return result;
                    }
                    string output = outputTask.Result;
                    errorTask.Result.ToString();
                    foreach (string line in (output ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!line.StartsWith("plugin=", StringComparison.OrdinalIgnoreCase)) continue;
                        string[] parts = line.Substring(7).Split('|');
                        if (parts.Length < 3) continue;
                        var plugin = new PluginInfo { Name = parts[0].Trim(), Dll = parts[1].Trim(), Type = parts[2].Trim() };
                        if (plugin.Name.Length == 0 || plugin.Dll.Length == 0 || plugin.Type.Length == 0) continue;
                        if (result.Exists(item => String.Equals(item.Dll, plugin.Dll, StringComparison.OrdinalIgnoreCase)
                            && String.Equals(item.Type, plugin.Type, StringComparison.OrdinalIgnoreCase))) continue;
                        result.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(delegate { status.Text = ex.Message; status.ForeColor = palette.Muted; }));
                }
                catch { }
            }
            return result;
        }

        private void ConfigurePlugin()
        {
            var plugin = choice.SelectedItem as PluginInfo;
            if (plugin == null) return;
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-config \"" + runtime + "\" \"" + plugin.Dll + "\" \"" + plugin.Type + "\"",
                    UseShellExecute = true,
                    WorkingDirectory = runtime
                };
                Process.Start(info);
                status.Text = (Localization.IsEnglish ? "Opened: " : "Geöffnet: ") + plugin.Name;
                status.ForeColor = palette.Text;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                status.ForeColor = palette.Muted;
                MessageBox.Show(FindForm(), ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
