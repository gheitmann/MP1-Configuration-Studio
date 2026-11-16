using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        private readonly StudioChoice choice;
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
            table.Controls.Add(new Label { Text = Localization.T("Plugin"), ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            choice = new StudioChoice(scale) { Dock = DockStyle.Fill, Font = body, Colors = () => palette, AccessibleName = Localization.T("Plugin") };
            table.Controls.Add(choice, 1, 0);

            var actions = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Background, Margin = new Padding(0) };
            configure = ButtonFor(Localization.T("Konfigurieren"), delegate { ConfigurePlugin(); }, body);
            configure.Enabled = false;
            actions.Controls.Add(ButtonFor(Localization.T("Aktualisieren"), delegate { LoadPlugins(); }, body));
            actions.Controls.Add(configure);
            actions.Controls.Add(ButtonFor(Localization.T("Klassische Konfiguration"), delegate { if (classic != null) classic(); }, body));
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
                Text = Localization.T("Plugin-Fenster behalten ihre eigene Oberfläche. Das Laden kann einen Moment dauern; Fehler bleiben isoliert und können erneut versucht oder über die klassische Konfiguration geöffnet werden."),
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
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void LoadPlugins()
        {
            if (loading) return;
            loading = true;
            choice.SetOptions(new ChoiceOption[0], String.Empty);
            plugins.Clear();
            configure.Enabled = false;
            status.Text = Localization.T("Plugin-Liste wird gelesen …");
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
                            var options = new List<ChoiceOption>();
                            for (int index = 0; index < plugins.Count; index++) options.Add(new ChoiceOption(index.ToString(CultureInfo.InvariantCulture), plugins[index].ToString()));
                            choice.SetOptions(options, plugins.Count > 0 ? "0" : String.Empty);
                            configure.Enabled = plugins.Count > 0;
                            status.Text = plugins.Count == 0
                                ? (Localization.T("Kein Plugin mit Konfigurationsfenster gefunden."))
                                : plugins.Count + (Localization.T(" konfigurierbare Plugins"));
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
                    Arguments = "--plugin-list " + PluginConfigHelper.QuoteArgument(runtime),
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
            int index;
            PluginInfo plugin = Int32.TryParse(choice.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out index) && index >= 0 && index < plugins.Count ? plugins[index] : null;
            if (plugin == null) return;
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--plugin-config " + PluginConfigHelper.QuoteArgument(runtime) + " " + PluginConfigHelper.QuoteArgument(plugin.Dll) + " " + PluginConfigHelper.QuoteArgument(plugin.Type),
                    UseShellExecute = true,
                    WorkingDirectory = runtime
                };
                Process.Start(info);
                status.Text = (Localization.T("Geöffnet: ")) + plugin.Name;
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
