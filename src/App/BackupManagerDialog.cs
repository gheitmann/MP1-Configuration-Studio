using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class BackupManagerDialog : Form
    {
        private readonly string backupRoot;
        private readonly ListBox list = new ListBox();
        private readonly Button restore = new Button();
        private readonly Button delete = new Button();
        private readonly Label empty = new Label();

        public string SelectedBackupPath { get; private set; }

        public BackupManagerDialog(string backupRoot = null)
        {
            this.backupRoot = String.IsNullOrWhiteSpace(backupRoot) ? ConfigurationBackup.DefaultBackupDirectory : backupRoot;
            Text = Localization.T("Konfigurations-Backups");
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(620, 380);
            MinimumSize = new Size(480, 280);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var header = new Label
            {
                Dock = DockStyle.Fill,
                Text = Localization.T("Sicherungen unter:") + "\r\n" + this.backupRoot,
                AutoEllipsis = true
            };
            layout.Controls.Add(header, 0, 0);

            list.Dock = DockStyle.Fill;
            list.IntegralHeight = false;
            list.DisplayMember = "Display";
            list.SelectedIndexChanged += delegate { UpdateButtons(); };
            list.DoubleClick += delegate { RestoreSelected(); };
            layout.Controls.Add(list, 0, 1);

            empty.Dock = DockStyle.Fill;
            empty.TextAlign = ContentAlignment.MiddleCenter;
            empty.Text = Localization.T("Keine Backups gefunden.");
            empty.Visible = false;
            layout.Controls.Add(empty, 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = Localization.T("Schließen"), Width = 95, Height = 28, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(close);
            var folder = new Button { Text = Localization.T("Ordner öffnen"), Width = 115, Height = 28 };
            folder.Click += delegate { OpenFolder(); };
            buttons.Controls.Add(folder);
            delete.Text = Localization.T("Löschen");
            delete.Width = 95;
            delete.Height = 28;
            delete.Click += delegate { DeleteSelected(); };
            buttons.Controls.Add(delete);
            restore.Text = Localization.T("Wiederherstellen");
            restore.Width = 125;
            restore.Height = 28;
            restore.Click += delegate { RestoreSelected(); };
            buttons.Controls.Add(restore);
            layout.Controls.Add(buttons, 0, 2);

            Controls.Add(layout);
            AcceptButton = restore;
            CancelButton = close;
            Reload();
        }

        private void Reload()
        {
            list.BeginUpdate();
            try
            {
                list.Items.Clear();
                foreach (BackupInfo info in ConfigurationBackup.ListBackups(backupRoot))
                {
                    list.Items.Add(info);
                }
            }
            finally
            {
                list.EndUpdate();
            }

            bool hasItems = list.Items.Count > 0;
            list.Visible = hasItems;
            empty.Visible = !hasItems;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool selected = list.SelectedItem is BackupInfo;
            restore.Enabled = selected;
            delete.Enabled = selected;
        }

        private void RestoreSelected()
        {
            var info = list.SelectedItem as BackupInfo;
            if (info == null)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(
                this,
                Localization.IsEnglish
                    ? "The current MediaPortal.xml is backed up first. Studio then closes and the original interface starts with the restored configuration.\r\n\r\nUnsaved changes will be lost. Continue?"
                    : "Die aktuelle MediaPortal.xml wird vorher gesichert. Danach wird Studio geschlossen und die originale Oberfläche mit der wiederhergestellten Konfiguration gestartet.\r\n\r\nNicht gespeicherte Änderungen gehen verloren. Fortfahren?",
                Localization.T("Konfiguration wiederherstellen"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes)
            {
                return;
            }

            SelectedBackupPath = info.Path;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteSelected()
        {
            var info = list.SelectedItem as BackupInfo;
            if (info == null)
            {
                return;
            }

            if (MessageBox.Show(this, Localization.T("Backup wirklich löschen?\r\n\r\n") + info.Path, Localization.T("Backup löschen"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ConfigurationBackup.DeleteBackup(info.Path);
                Reload();
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Deleting backup failed");
                MessageBox.Show(this, Localization.T("Das Backup konnte nicht gelöscht werden.\r\n\r\n") + ex.Message, Localization.T("Konfigurations-Backups"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenFolder()
        {
            try
            {
                if (System.IO.Directory.Exists(backupRoot))
                {
                    Process.Start("explorer.exe", "\"" + backupRoot + "\"");
                }
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Opening backup folder failed");
            }
        }
    }
}
