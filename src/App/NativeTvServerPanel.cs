using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeTvServerPanel : UserControl
    {
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly string tvServerDirectory;
        private readonly string setupPath;
        private readonly Label status = new Label();
        private readonly Label database = new Label();
        private readonly Label overview = new Label();

        public NativeTvServerPanel(string runtime, float scale)
        {
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            string root = Path.GetDirectoryName(runtime);
            string sibling = root == null ? null : Path.Combine(root, "MediaPortal TV Server");
            string standard = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Team MediaPortal", "MediaPortal TV Server");
            string standardX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal TV Server");
            tvServerDirectory = FirstWithSetup(sibling, standard, standardX86) ?? sibling;
            setupPath = tvServerDirectory == null ? null : Path.Combine(tvServerDirectory, "SetupTv.exe");
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = palette.Background, Padding = new Padding(0) };
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            status.AutoSize = true;
            status.Font = body;
            status.ForeColor = palette.Text;
            status.Margin = new Padding(0, 0, 0, Px(4));
            table.Controls.Add(status, 0, 0);
            database.AutoSize = true;
            database.Font = Typography.Create(9.5f * scale, FontStyle.Regular);
            database.ForeColor = palette.Muted;
            database.Margin = new Padding(0, 0, 0, Px(4));
            table.Controls.Add(database, 0, 1);
            overview.AutoSize = true;
            overview.Font = Typography.Create(9.5f * scale, FontStyle.Regular);
            overview.ForeColor = palette.Text;
            overview.Margin = new Padding(0, 0, 0, Px(8));
            table.Controls.Add(overview, 0, 2);

            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Margin = new Padding(0) };
            var setup = ButtonFor(Localization.IsEnglish ? "Open TV Server configuration" : "TV-Server-Konfiguration öffnen", delegate { OpenSetup(); }, body);
            var folder = ButtonFor(Localization.IsEnglish ? "Open TV Server folder" : "TV-Server-Ordner öffnen", delegate { OpenFolder(); }, body);
            actions.Controls.Add(setup);
            actions.Controls.Add(folder);
            table.Controls.Add(actions, 0, 3);

            var note = new Label
            {
                Text = Localization.IsEnglish
                    ? "The counts above are read-only and come from the TV Server business layer. Channels, EPG, cards, schedules, and the TV database are managed by the TV Server service and its own SetupTv configuration; this page does not replace channel scanning or TV Server administration."
                    : "Die Zähler oben sind schreibgeschützt und kommen aus der TV-Server-Business-Layer. Kanäle, EPG, Karten, Aufnahmen und die TV-Datenbank werden vom TV-Server-Dienst und der eigenen SetupTv-Konfiguration verwaltet; Kanalsuche und TV-Server-Verwaltung werden nicht ersetzt.",
                Font = Typography.Create(9f * scale, FontStyle.Regular),
                ForeColor = palette.Muted,
                AutoSize = true,
                MaximumSize = new Size(Px(760), 0),
                Margin = new Padding(0, Px(8), 0, 0)
            };
            table.Controls.Add(note, 0, 4);
            Controls.Add(table);
            RefreshStatus();
            LoadOverview();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private static string FirstWithSetup(params string[] candidates)
        {
            foreach (string candidate in candidates)
                if (!String.IsNullOrEmpty(candidate) && File.Exists(Path.Combine(candidate, "SetupTv.exe"))) return candidate;
            return null;
        }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button { Text = text, Font = font, Height = Px(30), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(10), 0, Px(10), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), Px(4)) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void RefreshStatus()
        {
            bool installed = setupPath != null && File.Exists(setupPath);
            string service = ServiceState();
            status.Text = (installed ? (Localization.IsEnglish ? "TV Server found" : "TV-Server gefunden") : (Localization.IsEnglish ? "TV Server not found" : "TV-Server nicht gefunden"))
                + (String.IsNullOrEmpty(service) ? string.Empty : "  ·  " + service);
            status.ForeColor = installed ? palette.Text : palette.Muted;
            database.Text = DatabaseSummary();
        }

        private static string ServiceState()
        {
            try
            {
                using (var controller = new ServiceController("TVService"))
                {
                    string state = controller.Status.ToString();
                    if (!Localization.IsEnglish)
                    {
                        switch (controller.Status)
                        {
                            case ServiceControllerStatus.Running: state = "Dienst läuft"; break;
                            case ServiceControllerStatus.Stopped: state = "Dienst gestoppt"; break;
                            case ServiceControllerStatus.StartPending: state = "Dienst startet"; break;
                            case ServiceControllerStatus.StopPending: state = "Dienst stoppt"; break;
                            case ServiceControllerStatus.Paused: state = "Dienst pausiert"; break;
                        }
                    }
                    else state = "Service " + state;
                    return state;
                }
            }
            catch { return string.Empty; }
        }

        private string DatabaseSummary()
        {
            string config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server", "Gentle.config");
            if (!File.Exists(config)) return Localization.IsEnglish ? "No TV Server Gentle.config found." : "Keine TV-Server-Gentle.config gefunden.";
            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.Load(config);
                XmlElement active = null;
                foreach (XmlNode node in document.SelectNodes("//DefaultProvider"))
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    string connection = element.GetAttribute("connectionString");
                    if (!String.IsNullOrEmpty(connection)) { active = element; break; }
                }
                if (active == null) return Localization.IsEnglish ? "No database provider configured." : "Kein Datenbank-Provider konfiguriert.";
                string provider = active.GetAttribute("name");
                string text = active.GetAttribute("connectionString");
                string server = ConnectionValue(text, "Server", "Data Source", "data source");
                string name = ConnectionValue(text, "Database", "Initial Catalog", "initial catalog");
                string summary = provider + (String.IsNullOrEmpty(name) ? string.Empty : "  ·  " + name) + (String.IsNullOrEmpty(server) ? string.Empty : "  ·  " + server);
                return (Localization.IsEnglish ? "Database: " : "Datenbank: ") + summary;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string ConnectionValue(string text, params string[] keys)
        {
            foreach (string part in (text ?? string.Empty).Split(';'))
            {
                int index = part.IndexOf('=');
                if (index <= 0) continue;
                string key = part.Substring(0, index).Trim();
                foreach (string candidate in keys)
                    if (String.Equals(key, candidate, StringComparison.OrdinalIgnoreCase))
                        return part.Substring(index + 1).Trim();
            }
            return string.Empty;
        }

        private void LoadOverview()
        {
            if (String.IsNullOrEmpty(tvServerDirectory) || !File.Exists(Path.Combine(tvServerDirectory, "TvBusinessLayer.dll")))
            {
                overview.Text = Localization.IsEnglish ? "No TV Server business layer found." : "Keine TV-Server-Business-Layer gefunden.";
                overview.ForeColor = palette.Muted;
                return;
            }
            overview.Text = Localization.IsEnglish ? "Reading TV Server overview …" : "TV-Server-Übersicht wird gelesen …";
            overview.ForeColor = palette.Muted;
            ThreadPool.QueueUserWorkItem(delegate
            {
                string text = ReadOverview();
                try
                {
                    if (!IsDisposed && IsHandleCreated)
                        BeginInvoke(new Action(delegate
                        {
                            overview.Text = text;
                            overview.ForeColor = text.StartsWith("TV:", StringComparison.Ordinal) ? palette.Text : palette.Muted;
                        }));
                }
                catch { }
            });
        }

        private string ReadOverview()
        {
            try
            {
                string config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server");
                var info = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--tv-overview \"" + tvServerDirectory + "\" \"" + config + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = tvServerDirectory
                };
                using (Process process = Process.Start(info))
                {
                    if (!process.WaitForExit(20000))
                    {
                        try { process.Kill(); } catch { }
                        return Localization.IsEnglish ? "TV Server overview timed out." : "Zeitüberschreitung bei der TV-Server-Übersicht.";
                    }
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string line in (output ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int index = line.IndexOf('=');
                        if (index > 0) values[line.Substring(0, index)] = line.Substring(index + 1);
                    }
                    if (!values.ContainsKey("ok") || values["ok"] != "1")
                    {
                        string message = values.ContainsKey("error") ? values["error"] : (String.IsNullOrWhiteSpace(error) ? "unknown error" : error.Trim());
                        return (Localization.IsEnglish ? "TV Server overview unavailable: " : "TV-Server-Übersicht nicht verfügbar: ") + message;
                    }
                    string Number(string key) { string value; return values.TryGetValue(key, out value) ? value : "0"; }
                    return (Localization.IsEnglish
                        ? "TV: " + Number("tvChannels") + " channels  ·  Radio: " + Number("radioChannels") + "  ·  " + Number("groups") + " groups  ·  " + Number("recordings") + " recordings  ·  " + Number("schedules") + " schedules  ·  " + Number("cards") + " cards"
                        : "TV: " + Number("tvChannels") + " Kanäle  ·  Radio: " + Number("radioChannels") + "  ·  " + Number("groups") + " Gruppen  ·  " + Number("recordings") + " Aufnahmen  ·  " + Number("schedules") + " Timer  ·  " + Number("cards") + " Karten");
                }
            }
            catch (Exception ex)
            {
                return (Localization.IsEnglish ? "TV Server overview unavailable: " : "TV-Server-Übersicht nicht verfügbar: ") + ex.Message;
            }
        }

        private void OpenSetup()
        {
            if (setupPath == null || !File.Exists(setupPath))
            {
                MessageBox.Show(FindForm(), Localization.IsEnglish ? "SetupTv.exe was not found." : "SetupTv.exe wurde nicht gefunden.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true, WorkingDirectory = tvServerDirectory }); }
            catch (Exception ex) { MessageBox.Show(FindForm(), ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void OpenFolder()
        {
            if (tvServerDirectory == null || !Directory.Exists(tvServerDirectory))
            {
                MessageBox.Show(FindForm(), Localization.IsEnglish ? "The TV Server folder was not found." : "Der TV-Server-Ordner wurde nicht gefunden.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { Process.Start("explorer.exe", "\"" + tvServerDirectory + "\""); }
            catch (Exception ex) { MessageBox.Show(FindForm(), ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
