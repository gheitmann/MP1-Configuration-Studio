using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Lists pending configuration entries and offers Save/Discard when closing.</summary>
    internal sealed class PendingChangesDialog : Form
    {
        private readonly Palette palette = Palette.For(true);
        private readonly ListBox list = new ListBox();
        private readonly float scale;

        public bool SaveRequested { get; private set; }
        public bool DiscardRequested { get; private set; }

        public PendingChangesDialog(IEnumerable<PendingChange> changes, int fileEditors, float scale) : this(changes, fileEditors, scale, false) { }

        public PendingChangesDialog(IEnumerable<PendingChange> changes, int fileEditors, float scale, bool confirmClose)
        {
            this.scale = scale;
            List<PendingChange> items = changes == null ? new List<PendingChange>() : changes.ToList();
            Text = confirmClose
                ? (Localization.IsEnglish ? "Unsaved changes" : "Ungespeicherte Änderungen")
                : (Localization.IsEnglish ? "Pending changes" : "Ungespeicherte Änderungen");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = palette.Panel;
            ForeColor = palette.Text;
            ClientSize = new Size(Px(820), Px(confirmClose ? 560 : 520));
            MinimumSize = new Size(Px(560), Px(360));
            Font = Typography.Create(10f * scale, FontStyle.Regular);

            string title = confirmClose
                ? (Localization.IsEnglish ? "Save your changes before leaving?" : "Änderungen vor dem Verlassen speichern?")
                : (Localization.IsEnglish ? "Pending configuration changes" : "Ausstehende Konfigurationsänderungen");
            var header = new Label
            {
                Text = title + " · " + items.Count + (fileEditors > 0 ? (Localization.IsEnglish ? " · " + fileEditors + " XML editor(s)" : " · " + fileEditors + " XML-Editor(en)") : String.Empty),
                Dock = DockStyle.Top,
                Height = Px(confirmClose ? 62 : 38),
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

            var footer = new Panel { Dock = DockStyle.Bottom, Height = Px(56), Padding = new Padding(Px(16), Px(10), Px(16), Px(10)), BackColor = palette.Panel };
            var actions = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, AutoSize = true, BackColor = palette.Panel, Width = Px(confirmClose ? 390 : 140) };
            var close = new StudioButton
            {
                Text = confirmClose ? (Localization.IsEnglish ? "Cancel" : "Abbrechen") : (Localization.IsEnglish ? "Close" : "Schließen"),
                Colors = () => palette,
                BackColor = palette.Panel,
                ForeColor = palette.Text,
                Width = Px(110),
                Height = Px(32),
                DialogResult = confirmClose ? DialogResult.Cancel : DialogResult.Cancel,
                Margin = new Padding(Px(8), 0, 0, 0)
            };
            close.FlatAppearance.BorderColor = palette.Border;
            actions.Controls.Add(close);
            if (confirmClose)
            {
                var save = new StudioButton { Text = Localization.IsEnglish ? "Save" : "Speichern", Primary = true, Colors = () => palette, BackColor = palette.Primary, ForeColor = palette.PrimaryText, Width = Px(120), Height = Px(32), Margin = new Padding(Px(8), 0, 0, 0) };
                save.Click += delegate { SaveRequested = true; DialogResult = DialogResult.OK; Close(); };
                actions.Controls.Add(save);
                var discard = new StudioButton { Text = Localization.IsEnglish ? "Don't save" : "Nicht speichern", Colors = () => palette, BackColor = palette.Panel, ForeColor = palette.Text, Width = Px(130), Height = Px(32), Margin = new Padding(Px(8), 0, 0, 0) };
                discard.FlatAppearance.BorderColor = palette.Border;
                discard.Click += delegate { DiscardRequested = true; DialogResult = DialogResult.OK; Close(); };
                actions.Controls.Add(discard);
            }
            footer.Controls.Add(actions);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
            AcceptButton = close;
            CancelButton = close;
            StudioDialog.Apply(this);
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
