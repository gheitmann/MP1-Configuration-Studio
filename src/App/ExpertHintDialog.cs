using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>One-time explanation of the Standard and Expert page sets.</summary>
    internal sealed class ExpertHintDialog : Form
    {
        private readonly Palette palette = Palette.For(true);
        public bool EnableExpert { get; private set; }

        public ExpertHintDialog(float scale)
        {
            Icon = StudioIcon.Value;
            Text = Localization.IsEnglish ? "Standard and Expert mode" : "Standard- und Expertenmodus";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = palette.Panel;
            ForeColor = palette.Text;
            ClientSize = new Size(Px(620, scale), Px(360, scale));
            Font = Typography.Create(10f * scale, FontStyle.Regular);

            var title = new Label
            {
                Text = Localization.IsEnglish ? "Standard mode keeps the everyday pages" : "Der Standard-Modus zeigt die alltäglichen Seiten",
                Font = Typography.Create(14f * scale, FontStyle.Bold),
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                Location = new Point(Px(24, scale), Px(22, scale)),
                AutoSize = true
            };
            var body = new Label
            {
                Text = Localization.IsEnglish
                    ? "The sidebar starts in Standard mode with general, playback, media, TV, subtitle and disc settings.\r\n\r\nSwitch the mode selector in the toolbar to Expert to show video/music databases, views, DSP, keyboard, codecs, plugins, remote and raw XML editors.\r\n\r\nYou can change the mode at any time; this hint appears only once."
                    : "Die Seitenleiste startet im Standard-Modus mit Allgemein, Wiedergabe, Medien, TV, Untertiteln und Discs.\r\n\r\nStelle den Modus in der Werkzeugleiste auf Experte, um Video-/Musikdatenbanken, Ansichten, DSP, Tastatur, Codecs, Plugins, Fernbedienung und XML-Rohdaten zu sehen.\r\n\r\nDer Modus lässt sich jederzeit wechseln; dieser Hinweis erscheint nur einmal.",
                Font = Typography.Create(9.5f * scale, FontStyle.Regular),
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                Location = new Point(Px(26, scale), Px(64, scale)),
                Size = new Size(Px(568, scale), Px(210, scale))
            };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = Px(56, scale), FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = palette.Panel, Padding = new Padding(Px(16, scale), Px(10, scale), Px(16, scale), Px(10, scale)) };
            var close = new StudioButton
            {
                Text = Localization.IsEnglish ? "Got it" : "Verstanden",
                Colors = () => palette,
                BackColor = palette.Panel,
                ForeColor = palette.Text,
                Width = Px(110, scale),
                Height = Px(32, scale),
                DialogResult = DialogResult.Cancel
            };
            close.FlatAppearance.BorderColor = palette.Border;
            actions.Controls.Add(close);
            var expert = new StudioButton
            {
                Text = Localization.IsEnglish ? "Show Expert mode" : "Expertenmodus zeigen",
                Primary = true,
                Colors = () => palette,
                BackColor = palette.Primary,
                ForeColor = palette.PrimaryText,
                Width = Px(160, scale),
                Height = Px(32, scale),
                Margin = new Padding(Px(8, scale), 0, 0, 0)
            };
            expert.Click += delegate { EnableExpert = true; DialogResult = DialogResult.OK; Close(); };
            actions.Controls.Add(expert);

            Controls.Add(actions);
            Controls.Add(body);
            Controls.Add(title);
            AcceptButton = expert;
            CancelButton = close;
            StudioDialog.Apply(this);
        }

        private static int Px(float value, float scale) { return (int)Math.Round(value * scale); }
    }
}
