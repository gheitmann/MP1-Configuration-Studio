using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class ErrorDialog : Form
    {
        private readonly string nativePath;
        private readonly string runtime;

        public ErrorDialog(string message, string nativePath, string runtime)
        {
            this.nativePath = nativePath;
            this.runtime = runtime;

            Text = "MP1 Configuration Studio";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(680, 380);
            MinimumSize = new Size(520, 260);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(14) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var details = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = true,
                Text = (message ?? String.Empty) + "\r\n\r\n" + Localization.T("Protokoll: ") + StudioLog.FilePath
            };
            layout.Controls.Add(details, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = Localization.T("Schließen"), Width = 95, Height = 28, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(close);

            var log = new Button { Text = Localization.T("Protokoll öffnen"), Width = 125, Height = 28 };
            log.Click += delegate { OpenLog(); };
            buttons.Controls.Add(log);

            var bundle = new Button { Text = Localization.T("Support-Bundle…"), Width = 130, Height = 28 };
            bundle.Click += delegate { CreateSupportBundle(); };
            buttons.Controls.Add(bundle);

            if (!String.IsNullOrEmpty(nativePath) && File.Exists(nativePath))
            {
                var classic = new Button { Text = Localization.T("Klassisch starten"), Width = 130, Height = 28 };
                classic.Click += delegate
                {
                    Program.StartNative(nativePath, runtime);
                    DialogResult = DialogResult.OK;
                    Close();
                };
                buttons.Controls.Add(classic);
            }

            layout.Controls.Add(buttons, 0, 1);
            Controls.Add(layout);
            AcceptButton = close;
            CancelButton = close;
        }

        public static void Show(IWin32Window owner, string message, string nativePath, string runtime)
        {
            try
            {
                using (var dialog = new ErrorDialog(message, nativePath, runtime))
                {
                    if (owner != null)
                    {
                        dialog.ShowDialog(owner);
                    }
                    else
                    {
                        dialog.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Error dialog failed");
                MessageBox.Show(message ?? ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenLog()
        {
            try
            {
                if (File.Exists(StudioLog.FilePath))
                {
                    Process.Start("explorer.exe", "/select,\"" + StudioLog.FilePath + "\"");
                }
                else
                {
                    Process.Start("explorer.exe", "\"" + StudioLog.DirectoryPath + "\"");
                }
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Opening log folder failed");
            }
        }

        private void CreateSupportBundle()
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "ZIP-Archiv (*.zip)|*.zip",
                Title = "Support-Bundle speichern",
                FileName = "MP1-Configuration-Studio-Support-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor previous = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    string path = SupportBundle.Create(dialog.FileName, null, null);
                    if (!String.IsNullOrEmpty(path))
                    {
                        MessageBox.Show(this, Localization.T("Support-Bundle erstellt:\r\n") + path, "Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, Localization.T("Das Support-Bundle konnte nicht erstellt werden. Details stehen im Protokoll."), "Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                finally
                {
                    Cursor.Current = previous;
                }
            }
        }
    }
}
