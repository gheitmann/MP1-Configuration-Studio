using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class FirstRunDialog : Form
    {
        public FirstRunDialog()
        {
            Icon = StudioIcon.Value;
            Font = Typography.Create(Typography.DialogBody, FontStyle.Regular);
            Text = Localization.IsEnglish ? "Welcome to MP1 Configuration Studio" : "Willkommen bei MP1 Configuration Studio";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(720, 430);
            MinimumSize = new Size(560, 320);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(20) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            var text = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                WordWrap = true,
                TabStop = false,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Font = Typography.Create(Typography.DialogBody, FontStyle.Regular),
                Text = Localization.IsEnglish
                    ? "Welcome to MP1 Configuration Studio.\r\n\r\n"
                        + "• A backup of MediaPortal.xml was created on first launch if the file existed.\r\n\r\n"
                        + "• Close MediaPortal and the original configurator before changing settings.\r\n\r\n"
                        + "• If something goes wrong: Tools → Start classic interface. The original MediaPortal pages remain unchanged there.\r\n\r\n"
                        + "• The help column on the right explains the current section. Ctrl+F searches pages and settings.\r\n\r\n"
                        + "• Size, theme, and language are available at the top right and under Tools.\r\n\r\n"
                        + "• OK saves with the original MediaPortal validation. Cancel discards changes."
                    : "Willkommen bei MP1 Configuration Studio.\r\n\r\n"
                        + "• Eine Sicherung von MediaPortal.xml wurde beim ersten Start angelegt, sofern die Datei vorhanden war.\r\n\r\n"
                        + "• Schließen Sie MediaPortal und den originalen Konfigurator, bevor Sie Änderungen vornehmen.\r\n\r\n"
                        + "• Bei Problemen: Werkzeuge → Klassische Oberfläche starten. Dort gelten die unveränderten MediaPortal-Seiten.\r\n\r\n"
                        + "• Die Hilfsspalte rechts erklärt den aktuellen Bereich. Mit Strg+F durchsuchen Sie Seiten und Einstellungen.\r\n\r\n"
                        + "• Größe, Theme und Sprache ändern Sie oben rechts bzw. unter Werkzeuge.\r\n\r\n"
                        + "• OK speichert mit der originalen MediaPortal-Prüfung. Abbrechen verwirft die Änderungen."
            };
            text.SelectionStart = 0;
            text.SelectionLength = 0;
            layout.Controls.Add(text, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = Localization.T("Verstanden"), Width = 130, Height = 32, DialogResult = DialogResult.OK, Tag = "studio-primary" };
            buttons.Controls.Add(close);
            layout.Controls.Add(buttons, 0, 1);

            Controls.Add(layout);
            AcceptButton = close;
            CancelButton = close;
            Shown += delegate { text.Select(0, 0); close.Focus(); };
        }
    }
}
