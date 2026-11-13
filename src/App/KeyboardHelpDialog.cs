using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class KeyboardHelpDialog : Form
    {
        public KeyboardHelpDialog()
        {
            Text = "Tastenkürzel";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 330);
            MinimumSize = new Size(460, 260);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            var text = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Text = "Strg+F          Suche öffnen\r\n"
                    + "Enter           ersten Treffer öffnen\r\n"
                    + "Esc             Suche leeren oder Dialog schließen\r\n"
                    + "Alt+Links       einen Schritt zurück\r\n"
                    + "Alt+Rechts      einen Schritt vorwärts\r\n"
                    + "Strg+Plus       Schrift und Oberfläche vergrößern\r\n"
                    + "Strg+Minus      Schrift und Oberfläche verkleinern\r\n"
                    + "F1              Online-Hilfe für den aktuellen Bereich\r\n"
                    + "Umschalt+F1     diese Übersicht\r\n"
            };
            layout.Controls.Add(text, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = "Schließen", Width = 100, Height = 30, DialogResult = DialogResult.OK };
            buttons.Controls.Add(close);
            layout.Controls.Add(buttons, 0, 1);

            Controls.Add(layout);
            AcceptButton = close;
            CancelButton = close;
        }
    }
}
