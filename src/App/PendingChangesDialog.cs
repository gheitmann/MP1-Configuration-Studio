using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Lists the configuration entries that are still waiting to be saved.</summary>
    internal sealed class PendingChangesDialog : Form
    {
        private readonly Palette palette = Palette.For(true);
        private readonly ListBox list = new ListBox();
        private readonly float scale;

        public PendingChangesDialog(IEnumerable<PendingChange> changes, int fileEditors, float scale)
        {
            this.scale = scale;
            List<PendingChange> items = changes == null ? new List<PendingChange>() : changes.ToList();
            Text = Localization.IsEnglish ? "Pending changes" : "Ungespeicherte Änderungen";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = palette.Panel;
            ForeColor = palette.Text;
            ClientSize = new Size(Px(820), Px(520));
            MinimumSize = new Size(Px(560), Px(320));
            Font = Typography.Create(10f * scale, FontStyle.Regular);

            var header = new Label
            {
                Text = (Localization.IsEnglish ? "Pending configuration changes" : "Ausstehende Konfigurationsänderungen") + " · " + items.Count + (fileEditors > 0 ? (Localization.IsEnglish ? " · " + fileEditors + " XML editor(s)" : " · " + fileEditors + " XML-Editor(en)") : String.Empty),
                Dock = DockStyle.Top,
                Height = Px(38),
                Padding = new Padding(Px(16), Px(12), Px(16), 0),
                Font = Typography.Create(11f * scale, FontStyle.Bold),
                ForeColor = palette.Text,
                BackColor = palette.Panel
            };

            list.Dock = DockStyle.Fill;
            list.Margin = new Padding(0);
            list.BorderStyle = BorderStyle.None;
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = Px(46);
            list.IntegralHeight = false;
            list.BackColor = palette.Input;
            list.ForeColor = palette.Text;
            list.Font = Typography.Create(9.5f * scale, FontStyle.Regular);
            list.DrawItem += DrawItem;
            list.HandleCreated += delegate { NativeDarkMode.Apply(list.Handle); };
            if (items.Count == 0)
                list.Items.Add(Localization.IsEnglish ? "No pending configuration entries. XML file editors may still contain unsaved text." : "Keine ausstehenden Konfigurationseinträge. XML-Editoren können weiterhin ungespeicherten Text enthalten.");
            else
                foreach (PendingChange change in items) list.Items.Add(change);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(Px(16), 0, Px(16), Px(8)), BackColor = palette.Panel };
            body.Controls.Add(list);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = Px(52), Padding = new Padding(Px(16), Px(8), Px(16), Px(8)), BackColor = palette.Panel };
            var close = new StudioButton { Text = Localization.IsEnglish ? "Close" : "Schließen", Dock = DockStyle.Right, Width = Px(110), Height = Px(32), Colors = () => palette, BackColor = palette.Panel, ForeColor = palette.Text, DialogResult = DialogResult.Cancel };
            close.FlatAppearance.BorderColor = palette.Border;
            footer.Controls.Add(close);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
            AcceptButton = close;
            CancelButton = close;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            using (var background = new SolidBrush(palette.Input)) e.Graphics.FillRectangle(background, e.Bounds);
            PendingChange change = list.Items[e.Index] as PendingChange;
            if (change == null)
            {
                TextRenderer.DrawText(e.Graphics, Convert.ToString(list.Items[e.Index]), list.Font, Rectangle.Inflate(e.Bounds, -Px(10), 0), palette.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                return;
            }
            string key = change.Section + "  /  " + change.Entry;
            string value = change.Removed
                ? (change.OldValue == null ? "(new) → (removed)" : (change.OldValue ?? String.Empty) + "  →  (" + (Localization.IsEnglish ? "removed" : "entfernt") + ")")
                : (change.OldValue == null ? "(" + (Localization.IsEnglish ? "new" : "neu") + ")  →  " + (change.NewValue ?? String.Empty) : (change.OldValue ?? String.Empty) + "  →  " + (change.NewValue ?? String.Empty));
            int left = e.Bounds.Left + Px(10);
            int width = Math.Max(0, e.Bounds.Width - Px(20));
            TextRenderer.DrawText(e.Graphics, key, list.Font, new Rectangle(left, e.Bounds.Top + Px(5), width, Px(20)), palette.Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics, value, list.Font, new Rectangle(left, e.Bounds.Top + Px(23), width, Px(18)), palette.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            using (var pen = new Pen(Color.FromArgb(70, palette.Border))) e.Graphics.DrawLine(pen, e.Bounds.Left + Px(8), e.Bounds.Bottom - 1, e.Bounds.Right - Px(8), e.Bounds.Bottom - 1);
        }
    }
}
