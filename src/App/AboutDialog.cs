using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Themed About window for the Studio.</summary>
    internal sealed class AboutDialog : Form
    {
        private readonly Palette palette = Palette.For(true);
        private readonly float scale;

        public AboutDialog(float scale)
        {
            this.scale = scale;
            Text = Localization.IsEnglish ? "About MP1 Configuration Studio" : "Über MP1 Configuration Studio";
            Icon = StudioIcon.Value;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = palette.Panel;
            ForeColor = palette.Text;
            ClientSize = new Size(Px(560), Px(380));
            Font = Typography.Create(10f * scale, FontStyle.Regular);

            var logo = new StudioLogo { Colors = () => palette, Bounds = new Rectangle(Px(24), Px(24), Px(48), Px(48)), Font = Typography.Create(15f * scale, FontStyle.Bold) };
            var title = new Label
            {
                Text = "MP1 Configuration Studio",
                Font = Typography.Create(16f * scale, FontStyle.Regular),
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                Location = new Point(Px(88), Px(24)),
                AutoSize = true
            };
            var version = new Label
            {
                Text = UpdateChecker.CurrentVersion + "  ·  MediaPortal 1.39 x64",
                Font = Typography.Create(9f * scale, FontStyle.Regular),
                ForeColor = palette.Muted,
                BackColor = Color.Transparent,
                Location = new Point(Px(90), Px(54)),
                AutoSize = true
            };
            var body = new Label
            {
                Text = Localization.IsEnglish
                    ? "Native configuration interface for MediaPortal 1.39.\r\n\r\nReads and writes MediaPortal.xml directly, preserves unknown sections and entries, and creates a ZIP backup before saving.\r\n\r\nLicense: GPL-3.0-or-later\r\nMediaPortal is a separate project and is not bundled with this application."
                    : "Native Konfigurationsoberfläche für MediaPortal 1.39.\r\n\r\nLiest und schreibt MediaPortal.xml direkt, erhält unbekannte Bereiche und Einträge und erstellt vor dem Speichern eine ZIP-Sicherung.\r\n\r\nLizenz: GPL-3.0-or-later\r\nMediaPortal ist ein separates Projekt und nicht Teil dieser Anwendung.",
                Font = Typography.Create(9.5f * scale, FontStyle.Regular),
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                Location = new Point(Px(26), Px(96)),
                Size = new Size(Px(508), Px(210))
            };
            var close = new StudioButton
            {
                Text = Localization.IsEnglish ? "Close" : "Schließen",
                Colors = () => palette,
                BackColor = palette.Panel,
                ForeColor = palette.Text,
                Font = Typography.Create(10f * scale, FontStyle.Regular),
                Width = Px(110),
                Height = Px(32),
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            close.FlatAppearance.BorderColor = palette.Border;
            close.Location = new Point(Px(560 - 26 - 110), Px(380 - 20 - 32));

            Controls.Add(logo);
            Controls.Add(title);
            Controls.Add(version);
            Controls.Add(body);
            Controls.Add(close);
            AcceptButton = close;
            CancelButton = close;
            StudioDialog.Apply(this);
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }
    }
}
