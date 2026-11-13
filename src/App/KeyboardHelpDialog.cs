using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class KeyboardHelpDialog : Form
    {
        public KeyboardHelpDialog()
        {
            Icon = StudioIcon.Value;
            Font = Typography.Create(Typography.DialogBody, FontStyle.Regular);
            Text = Localization.T("Tastenkürzel");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(600, 430);
            MinimumSize = new Size(480, 280);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

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
                    ? "Ctrl+F — Open search\r\n\r\n"
                        + "Enter — Open the first result\r\n\r\n"
                        + "Esc — Clear search or close dialog\r\n\r\n"
                        + "Alt+Left — Go back one step\r\n\r\n"
                        + "Alt+Right — Go forward one step\r\n\r\n"
                        + "Ctrl+Plus — Increase text and interface size\r\n\r\n"
                        + "Ctrl+Minus — Decrease text and interface size\r\n\r\n"
                        + "F1 — Online help for the current section\r\n\r\n"
                        + "Shift+F1 — This overview\r\n"
                    : "Strg+F — Suche öffnen\r\n\r\n"
                        + "Enter — ersten Treffer öffnen\r\n\r\n"
                        + "Esc — Suche leeren oder Dialog schließen\r\n\r\n"
                        + "Alt+Links — einen Schritt zurück\r\n\r\n"
                        + "Alt+Rechts — einen Schritt vorwärts\r\n\r\n"
                        + "Strg+Plus — Schrift und Oberfläche vergrößern\r\n\r\n"
                        + "Strg+Minus — Schrift und Oberfläche verkleinern\r\n\r\n"
                        + "F1 — Online-Hilfe für den aktuellen Bereich\r\n\r\n"
                        + "Umschalt+F1 — diese Übersicht\r\n"
            };
            text.SelectionStart = 0;
            text.SelectionLength = 0;
            layout.Controls.Add(text, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = Localization.T("Schließen"), Width = 110, Height = 32, DialogResult = DialogResult.OK, Tag = "studio-primary" };
            buttons.Controls.Add(close);
            layout.Controls.Add(buttons, 0, 1);

            Controls.Add(layout);
            AcceptButton = close;
            CancelButton = close;
            Shown += delegate { text.Select(0, 0); close.Focus(); };
        }
    }
}
