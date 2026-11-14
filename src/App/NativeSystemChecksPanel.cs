using System;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeSystemChecksPanel : UserControl
    {
        private readonly string runtime;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly Action classic;
        private readonly Label mcs = new Label();
        private readonly CheckBox mptray = new CheckBox();
        private readonly Label status = new Label();
        private bool loading;

        public NativeSystemChecksPanel(string runtime, float scale, Action classic)
        {
            this.runtime = runtime;
            this.scale = scale;
            this.classic = classic;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = palette.Background, Padding = new Padding(0) };
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            mcs.AutoSize = true;
            mcs.Font = body;
            mcs.ForeColor = palette.Text;
            mcs.Margin = new Padding(0, 0, 0, Px(6));
            table.Controls.Add(mcs, 0, 0);

            mptray.Text = Localization.IsEnglish ? "Start MPTray with Windows" : "MPTray mit Windows starten";
            mptray.AutoSize = true;
            mptray.Font = body;
            mptray.ForeColor = palette.Text;
            mptray.BackColor = palette.Background;
            mptray.FlatStyle = FlatStyle.Flat;
            mptray.Margin = new Padding(0, 0, 0, Px(6));
            mptray.CheckedChanged += delegate
            {
                if (loading) return;
                SetMPTray(mptray.Checked);
                status.Text = Localization.IsEnglish ? "MPTray autostart updated." : "MPTray-Autostart aktualisiert.";
            };
            table.Controls.Add(mptray, 0, 1);

            var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = palette.Background, Margin = new Padding(0, 0, 0, Px(6)) };
            var open = new Button { Text = Localization.IsEnglish ? "Open classic checks" : "Klassische Prüfungen öffnen", Font = body, Height = Px(28), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            open.FlatAppearance.BorderColor = palette.Border;
            open.Click += delegate { if (classic != null) classic(); };
            actions.Controls.Add(open);
            table.Controls.Add(actions, 0, 2);

            status.AutoSize = true;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            status.Margin = new Padding(0, 0, 0, Px(6));
            table.Controls.Add(status, 0, 3);

            var note = new Label
            {
                Text = Localization.IsEnglish
                    ? "MPTray autostart is a per-user registry setting. Media Center service policy changes need administrator rights and remain in the classic checks page."
                    : "Der MPTray-Autostart ist eine Benutzer-Registry-Einstellung. Änderungen an der Media-Center-Dienstrichtlinie benötigen Administratorrechte und bleiben in der klassischen Prüfseite.",
                AutoSize = true,
                MaximumSize = new Size(Px(740), 0),
                Font = Typography.Create(9f * scale, FontStyle.Regular),
                ForeColor = palette.Muted,
                Margin = new Padding(0)
            };
            table.Controls.Add(note, 0, 4);
            Controls.Add(table);
            LoadState();
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void LoadState()
        {
            loading = true;
            mptray.Checked = ReadMPTray();
            loading = false;
            mcs.Text = McsState();
        }

        private string McsState()
        {
            bool installed = false;
            bool running = false;
            try
            {
                foreach (ServiceController service in ServiceController.GetServices())
                {
                    if (service.ServiceName == "ehRecvr" || service.ServiceName == "ehSched")
                    {
                        installed = true;
                        if (service.Status == ServiceControllerStatus.Running) running = true;
                    }
                }
            }
            catch { }
            if (!installed) return Localization.IsEnglish ? "Media Center services: not installed" : "Media-Center-Dienste: nicht installiert";
            bool policy = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsMediaCenter", false))
                {
                    object value = key == null ? null : key.GetValue("MediaCenter");
                    policy = value != null && value.ToString() == "1";
                }
            }
            catch { }
            if (policy) return Localization.IsEnglish ? "Media Center services: disabled by policy" : "Media-Center-Dienste: per Richtlinie deaktiviert";
            return running
                ? (Localization.IsEnglish ? "Media Center services: running" : "Media-Center-Dienste: laufen")
                : (Localization.IsEnglish ? "Media Center services: stopped" : "Media-Center-Dienste: gestoppt");
        }

        private bool ReadMPTray()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    return key != null && key.GetValue("MPTray") != null;
                }
            }
            catch { return false; }
        }

        private void SetMPTray(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (enabled) key.SetValue("MPTray", Path.Combine(runtime, "MPTray.exe"));
                    else if (key.GetValue("MPTray") != null) key.DeleteValue("MPTray");
                }
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }
    }
}
