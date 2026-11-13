using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class FirstRunDialog : Form
    {
        public FirstRunDialog()
        {
            Text = Localization.IsEnglish ? "Welcome to MP1 Configuration Studio" : "Willkommen bei MP1 Configuration Studio";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(660, 390);
            MinimumSize = new Size(520, 300);

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
                Text = Localization.IsEnglish
                    ? "Welcome to MP1 Configuration Studio.\r\n\r\n"
                        + "• A backup of MediaPortal.xml was created on first launch if the file existed.\r\n\r\n"
                        + "• Close MediaPortal and the original configurator before changing settings.\r\n\r\n"
                        + "• If something goes wrong: Tools → Start classic interface. The original MediaPortal pages remain unchanged there.\r\n\r\n"
                        + "• On 4K/high-DPI displays Studio starts at 100% size. Use the size selector at the top right if you need larger text.\r\n\r\n"
                        + "• The help column on the right explains the current section. Ctrl+F searches pages and settings.\r\n\r\n"
                        + "• MediaPortal validates the settings before saving as usual. Cancel discards changes."
                    : "Willkommen bei MP1 Configuration Studio.\r\n\r\n"
                        + "• Eine Sicherung von MediaPortal.xml wurde beim ersten Start angelegt, sofern die Datei vorhanden war.\r\n\r\n"
                        + "• Schließen Sie MediaPortal und den originalen Konfigurator, bevor Sie Änderungen vornehmen.\r\n\r\n"
                        + "• Bei Problemen: Werkzeuge → Klassische Oberfläche starten. Dort gelten die unveränderten MediaPortal-Seiten.\r\n\r\n"
                        + "• Auf 4K-/High-DPI-Displays startet Studio mit 100 % Größe. Ändern Sie die Größe oben rechts, wenn Sie mehr Text benötigen.\r\n\r\n"
                        + "• Die Hilfsspalte rechts erklärt den aktuellen Bereich. Mit Strg+F durchsuchen Sie Seiten und Einstellungen.\r\n\r\n"
                        + "• Vor dem Speichern prüft MediaPortal die Einstellungen wie gewohnt. Abbrechen verwirft die Änderungen."
            };
            layout.Controls.Add(text, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            var close = new Button { Text = Localization.T("Verstanden"), Width = 120, Height = 30, DialogResult = DialogResult.OK };
            buttons.Controls.Add(close);
            layout.Controls.Add(buttons, 0, 1);

            Controls.Add(layout);
            AcceptButton = close;
            CancelButton = close;
        }
    }
}
