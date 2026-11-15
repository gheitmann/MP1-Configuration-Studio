using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Project start page with MediaPortal information, links and donation entry.</summary>
    internal sealed class NativeProjectPanel : UserControl
    {
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private const string Homepage = "https://www.team-mediaportal.com/";
        private const string Forum = "https://forum.team-mediaportal.com/";
        private const string Wiki = "https://www.team-mediaportal.com/wiki/";
        private const string MediaPortalGitHub = "https://github.com/MediaPortal/MediaPortal-1";
        private const string StudioGitHub = "https://github.com/gheitmann/MP1-Configuration-Studio";
        private const string Donate = "https://forum.team-mediaportal.com/donate/";

        public NativeProjectPanel(float scale)
        {
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(9.5f * scale, FontStyle.Regular);
            var small = Typography.Create(8.5f * scale, FontStyle.Regular);
            var heading = Typography.Create(11.5f * scale, FontStyle.Bold);

            var root = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = palette.Background, Padding = new Padding(0), Margin = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var brand = new Panel { Dock = DockStyle.Fill, Height = Px(84), BackColor = Color.FromArgb(21, 26, 35), Margin = new Padding(0, 0, 0, Px(8)) };
            Image banner = LoadImage("MP1.ConfigurationStudio.MediaPortalLogo.png");
            if (banner != null)
            {
                var picture = new PictureBox { Image = banner, SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(Px(24), Px(8), Px(300), Px(43)), BackColor = Color.Transparent };
                brand.Controls.Add(picture);
            }
            else
            {
                var icon = new PictureBox { Image = LoadImage("MP1.ConfigurationStudio.MediaPortalIcon.png"), SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(Px(24), Px(16), Px(50), Px(50)), BackColor = Color.Transparent };
                brand.Controls.Add(icon);
            }
            brand.Controls.Add(new Label
            {
                Text = "MediaPortal 1.39 · Endurance · Open Source HTPC",
                Font = small,
                ForeColor = Color.FromArgb(177, 190, 210),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Px(26), Px(58))
            });
            root.Controls.Add(brand);

            root.Controls.Add(Paragraph(Localization.IsEnglish
                ? "MediaPortal is a free open source project that turns your computer into a full media center (HTPC). Watch and record TV, play videos, DVDs and Blu-rays, listen to music and radio, view pictures, and extend everything with plugins and skins."
                : "MediaPortal ist ein freies Open-Source-Projekt und verwandelt deinen PC in ein vollwertiges Media Center (HTPC). Sieh und nimm Fernsehen auf, spiele Videos, DVDs und Blu-rays ab, höre Musik und Radio, betrachte Bilder und erweitere alles mit Plugins und Skins.", body));

            root.Controls.Add(SectionTitle(Localization.IsEnglish ? "Contact" : "Kontakt", heading));
            var links = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, BackColor = palette.Background, Margin = new Padding(0, 0, 0, Px(8)) };
            links.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(190)));
            links.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddLink(links, Localization.IsEnglish ? "Homepage" : "Homepage", Homepage, small);
            AddLink(links, Localization.IsEnglish ? "Forum" : "Forum", Forum, small);
            AddLink(links, Localization.IsEnglish ? "Online documentation" : "Online-Dokumentation", Wiki, small);
            AddLink(links, Localization.IsEnglish ? "MediaPortal on GitHub" : "MediaPortal auf GitHub", MediaPortalGitHub, small);
            AddLink(links, Localization.IsEnglish ? "Studio on GitHub" : "Studio auf GitHub", StudioGitHub, small);
            AddLink(links, "IRC", "https://webchat.freenode.net/?channels=MediaPortal", small);
            root.Controls.Add(links);

            root.Controls.Add(Paragraph((Localization.IsEnglish ? "About: " : "Über: ") + "Version " + UpdateChecker.CurrentVersion + "  ·  MediaPortal 1.39  ·  x64  ·  Endurance", small));

            var donate = new Panel { Dock = DockStyle.Fill, Height = Px(104), BackColor = palette.Panel, Margin = new Padding(0, Px(6), 0, 0) };
            donate.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (var pen = new Pen(palette.Border)) e.Graphics.DrawRectangle(pen, 0, 0, donate.Width - 1, donate.Height - 1);
                using (var brush = new SolidBrush(palette.Accent)) e.Graphics.FillRectangle(brush, 0, 0, Px(4), donate.Height);
            };
            var donateLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = palette.Panel, Padding = new Padding(Px(20), Px(10), Px(16), Px(10)) };
            donateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            donateLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            donateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(38)));
            donateLayout.Controls.Add(new Label
            {
                Text = Localization.IsEnglish
                    ? "Support MediaPortal\r\nDonations help the MediaPortal project with hosting, development and infrastructure. The official donation page is hosted in the MediaPortal forum."
                    : "MediaPortal unterstützen\r\nSpenden helfen dem MediaPortal-Projekt bei Hosting, Entwicklung und Infrastruktur. Die offizielle Spendenseite liegt im MediaPortal-Forum.",
                Font = small,
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                MaximumSize = new Size(Px(780), 0),
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            }, 0, 0);
            var donateActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = palette.Panel, Margin = new Padding(0) };
            donateActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            donateActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(196)));
            var donateLink = new LinkLabel
            {
                Text = Donate,
                Font = small,
                LinkColor = palette.Accent,
                ActiveLinkColor = palette.Text,
                VisitedLinkColor = palette.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0)
            };
            donateLink.LinkClicked += delegate { Open(Donate); };
            donateActions.Controls.Add(donateLink, 0, 0);
            var donateButton = new StudioButton
            {
                Text = Localization.IsEnglish ? "Donate via PayPal" : "Über PayPal spenden",
                Primary = true,
                Colors = () => palette,
                BackColor = palette.Primary,
                ForeColor = palette.PrimaryText,
                Font = body,
                Width = Px(190),
                Height = Px(34),
                Anchor = AnchorStyles.Right,
                Margin = new Padding(Px(6), 0, 0, 0)
            };
            donateButton.Click += delegate { Open(Donate); };
            donateActions.Controls.Add(donateButton, 1, 0);
            donateLayout.Controls.Add(donateActions, 0, 1);
            donate.Controls.Add(donateLayout);
            root.Controls.Add(donate);

            Controls.Add(root);
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private Label Paragraph(string text, Font font)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                MaximumSize = new Size(Px(800), 0),
                Margin = new Padding(0, 0, 0, Px(6))
            };
        }

        private Label SectionTitle(string text, Font font)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = palette.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, Px(3), 0, Px(3))
            };
        }

        private void AddLink(TableLayoutPanel table, string caption, string url, Font font)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label = new Label { Text = caption + ":", Font = font, ForeColor = palette.Muted, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 0, Px(12), 0) };
            var link = new LinkLabel { Text = url, Font = font, LinkColor = palette.Accent, ActiveLinkColor = palette.Text, VisitedLinkColor = palette.Accent, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0) };
            link.LinkClicked += delegate { Open(url); };
            table.Controls.Add(label, 0, table.RowCount);
            table.Controls.Add(link, 1, table.RowCount);
            table.RowCount++;
        }

        private static Image LoadImage(string name)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                {
                    if (stream == null) return null;
                    using (var image = Image.FromStream(stream)) return new Bitmap(image);
                }
            }
            catch { return null; }
        }

        private static void Open(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }
    }
}
