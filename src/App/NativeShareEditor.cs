using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeShareEditor : UserControl
    {
        private static readonly string[] KeyPrefixes = { "sharename", "sharepath", "sharescan", "pincode", "sharetype", "shareserver", "sharelogin", "sharepassword", "shareport", "shareremotepath", "shareview", "sharewakeonlan", "hostdetectmethod", "sharedonotfolderjpgifpin", "videothumbscreate", "eachfolderismovie" };
        private readonly StudioConfiguration configuration;
        private readonly string section;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly ThemedListView list;
        private readonly List<ShareEntry> shares = new List<ShareEntry>();
        private readonly bool supportsScan;
        private readonly Button editButton;
        private readonly Button removeButton;

        private sealed class ShareEntry
        {
            public string Name;
            public string Path;
            public bool Scan;
            public Dictionary<string, string> Extra = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public event EventHandler Changed;

        public int PreferredHeight
        {
            get
            {
                int rows = Math.Max(3, shares.Count);
                int rowHeight = list.Font.Height + Px(7);
                return Math.Max(Px(140), Px(26) + rows * rowHeight + Px(46));
            }
        }

        public NativeShareEditor(StudioConfiguration configuration, string section, float scale)
        {
            this.configuration = configuration;
            this.section = section;
            this.scale = scale;
            supportsScan = !String.Equals(section, "pictures", StringComparison.OrdinalIgnoreCase);
            BackColor = palette.Background;
            ForeColor = palette.Text;
            MinimumSize = new Size(Px(320), Px(150));
            var body = Typography.Create(10f * scale, FontStyle.Regular);
            list = new ThemedListView(palette) { Dock = DockStyle.Fill, Font = body, BackColor = palette.Input, ForeColor = palette.Text };
            list.Columns.Add(Localization.IsEnglish ? "Name" : "Name", Px(150));
            list.Columns.Add(Localization.IsEnglish ? "Folder" : "Ordner", Px(260));
            if (supportsScan) list.Columns.Add(Localization.IsEnglish ? "Scan" : "Scannen", Px(80));
            list.DoubleClick += delegate { EditSelected(); };
            list.SelectedIndexChanged += delegate { UpdateButtons(); };
            list.Resize += delegate { ResizeColumns(); };

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Height = Px(38), WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(5), 0, 0) };
            actions.Controls.Add(ButtonFor(Localization.IsEnglish ? "Add folder…" : "Ordner hinzufügen…", delegate { AddShare(); }, body));
            editButton = ButtonFor(Localization.IsEnglish ? "Edit" : "Bearbeiten", delegate { EditSelected(); }, body);
            removeButton = ButtonFor(Localization.IsEnglish ? "Remove" : "Entfernen", delegate { RemoveSelected(); }, body);
            actions.Controls.Add(editButton);
            actions.Controls.Add(removeButton);
            Controls.Add(list);
            Controls.Add(actions);
            LoadShares();
        }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button
            {
                Text = text,
                Font = font,
                Height = Px(30),
                MinimumSize = new Size(Px(92), Px(30)),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(Px(8), 0, Px(8), 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = palette.Panel,
                ForeColor = palette.Text,
                Margin = new Padding(0, 0, Px(6), 0)
            };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void ResizeColumns()
        {
            int width = list.ClientSize.Width;
            if (width <= Px(80) || list.Columns.Count < 2) return;
            int name = (int)(width * (supportsScan ? 0.32 : 0.36));
            int scan = supportsScan ? (int)(width * 0.18) : 0;
            int folder = width - name - scan - Px(2);
            if (folder < Px(80)) folder = Px(80);
            list.Columns[0].Width = name;
            list.Columns[1].Width = folder;
            if (supportsScan) list.Columns[2].Width = scan;
        }

        private void LoadShares()
        {
            shares.Clear();
            for (int index = 0; ; index++)
            {
                var entry = new ShareEntry();
                foreach (string prefix in KeyPrefixes)
                {
                    string value = configuration.Get(section, prefix + index, null);
                    if (value == null) continue;
                    if (prefix == "sharename") entry.Name = value;
                    else if (prefix == "sharepath") entry.Path = value;
                    else if (prefix == "sharescan")
                    {
                        if (supportsScan) entry.Scan = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
                        else entry.Extra[prefix] = value;
                    }
                    else entry.Extra[prefix] = value;
                }
                if (String.IsNullOrEmpty(entry.Name) && String.IsNullOrEmpty(entry.Path)) break;
                shares.Add(entry);
            }
            RefreshList();
        }

        private void RefreshList()
        {
            list.BeginUpdate();
            try
            {
                list.Items.Clear();
                foreach (ShareEntry share in shares)
                {
                    var item = new ListViewItem(share.Name ?? "");
                    item.SubItems.Add(share.Path ?? "");
                    if (supportsScan) item.SubItems.Add(share.Scan ? (Localization.IsEnglish ? "Yes" : "Ja") : (Localization.IsEnglish ? "No" : "Nein"));
                    list.Items.Add(item);
                }
            }
            finally { list.EndUpdate(); }
            ResizeColumns();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool hasSelection = list.SelectedIndices.Count > 0;
            if (editButton != null) editButton.Enabled = hasSelection;
            if (removeButton != null) removeButton.Enabled = hasSelection;
        }

        private void AddShare()
        {
            using (var dialog = new ShareDialog(null, scale, supportsScan))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                shares.Add(new ShareEntry { Name = dialog.ShareName, Path = dialog.SharePath, Scan = dialog.Scan });
                Commit();
                RefreshList();
                if (list.Items.Count > 0) list.Items[list.Items.Count - 1].Selected = true;
            }
        }

        private void EditSelected()
        {
            if (list.SelectedIndices.Count == 0) return;
            int index = list.SelectedIndices[0];
            if (index < 0 || index >= shares.Count) return;
            using (var dialog = new ShareDialog(shares[index], scale, supportsScan))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                shares[index].Name = dialog.ShareName;
                shares[index].Path = dialog.SharePath;
                shares[index].Scan = dialog.Scan;
                Commit();
                RefreshList();
                if (index < list.Items.Count) list.Items[index].Selected = true;
            }
        }

        private void RemoveSelected()
        {
            if (list.SelectedIndices.Count == 0) return;
            int index = list.SelectedIndices[0];
            if (index < 0 || index >= shares.Count) return;
            if (MessageBox.Show(FindForm(), Localization.IsEnglish ? "Remove this folder?" : "Diesen Ordner entfernen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            shares.RemoveAt(index);
            Commit();
            RefreshList();
        }

        private void Commit()
        {
            foreach (string key in configuration.Entries(section).Select(entry => entry.Key).Where(IsShareKey).ToArray())
                configuration.Remove(section, key);
            for (int index = 0; index < shares.Count; index++)
            {
                ShareEntry share = shares[index];
                configuration.Set(section, "sharename" + index, share.Name ?? "");
                configuration.Set(section, "sharepath" + index, share.Path ?? "");
                if (supportsScan) configuration.Set(section, "sharescan" + index, share.Scan ? "yes" : "no");
                foreach (var pair in share.Extra) configuration.Set(section, pair.Key + index, pair.Value ?? "");
            }
            var handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        internal static bool IsShareKey(string key)
        {
            if (String.IsNullOrEmpty(key)) return false;
            int end = key.Length;
            while (end > 0 && Char.IsDigit(key[end - 1])) end--;
            if (end == 0 || end == key.Length) return false;
            string prefix = key.Substring(0, end);
            if (prefix.StartsWith("share", StringComparison.OrdinalIgnoreCase)) return true;
            return String.Equals(prefix, "pincode", StringComparison.OrdinalIgnoreCase)
                || String.Equals(prefix, "hostdetectmethod", StringComparison.OrdinalIgnoreCase)
                || String.Equals(prefix, "videothumbscreate", StringComparison.OrdinalIgnoreCase)
                || String.Equals(prefix, "eachfolderismovie", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ThemedListView : ListView
        {
            private readonly Palette palette;

            public ThemedListView(Palette palette)
            {
                this.palette = palette;
                View = View.Details;
                FullRowSelect = true;
                MultiSelect = false;
                HideSelection = false;
                HeaderStyle = ColumnHeaderStyle.Nonclickable;
                BorderStyle = BorderStyle.None;
                OwnerDraw = true;
                DoubleBuffered = true;
                DrawColumnHeader += DrawHeader;
                DrawItem += delegate(object sender, DrawListViewItemEventArgs e) { e.DrawDefault = false; };
                DrawSubItem += DrawCell;
            }

            private void DrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
            {
                using (var background = new SolidBrush(palette.Panel)) e.Graphics.FillRectangle(background, e.Bounds);
                using (var pen = new Pen(palette.Border))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                    e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top + 5, e.Bounds.Right - 1, e.Bounds.Bottom - 6);
                }
                var textBounds = new Rectangle(e.Bounds.X + 5, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, Font, textBounds, palette.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            private void DrawCell(object sender, DrawListViewSubItemEventArgs e)
            {
                bool selected = e.Item.Selected;
                Color background = selected ? palette.Selection : (e.ItemIndex % 2 == 0 ? palette.Input : palette.Panel);
                using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
                Color ink = selected ? palette.SelectionText : palette.Text;
                var textBounds = new Rectangle(e.Bounds.X + 5, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 10), e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, textBounds, ink, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private sealed class ShareDialog : Form
        {
            private readonly TextBox name = new TextBox();
            private readonly TextBox path = new TextBox();
            private readonly CheckBox scan = new CheckBox { Text = Localization.IsEnglish ? "Scan this folder" : "Diesen Ordner scannen" };
            public string ShareName { get { return name.Text.Trim(); } }
            public string SharePath { get { return path.Text.Trim(); } }
            public bool Scan { get { return scan.Checked; } }

            public ShareDialog(ShareEntry entry, float scale, bool supportsScan)
            {
                var palette = Palette.For(true);
                Text = entry == null ? (Localization.IsEnglish ? "Add media folder" : "Medienordner hinzufügen") : (Localization.IsEnglish ? "Edit media folder" : "Medienordner bearbeiten");
                Icon = StudioIcon.Value;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
                ClientSize = new Size((int)(520 * scale), (int)(220 * scale));
                BackColor = palette.Background; ForeColor = palette.Text;
                Font = Typography.Create(10f * scale, FontStyle.Regular);
                var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding((int)(18 * scale)) };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, (int)(90 * scale)));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)(34 * scale)));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)(34 * scale)));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)(34 * scale)));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                table.Controls.Add(new Label { Text = Localization.IsEnglish ? "Name" : "Name", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
                name.Dock = DockStyle.Fill; name.BackColor = palette.Input; name.ForeColor = palette.Text; name.BorderStyle = BorderStyle.FixedSingle;
                table.Controls.Add(name, 1, 0);
                table.Controls.Add(new Label { Text = Localization.IsEnglish ? "Folder" : "Ordner", ForeColor = palette.Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
                var pathRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
                pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, (int)(36 * scale)));
                path.Dock = DockStyle.Fill; path.BackColor = palette.Input; path.ForeColor = palette.Text; path.BorderStyle = BorderStyle.FixedSingle;
                pathRow.Controls.Add(path, 0, 0);
                var browse = new Button { Text = "…", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text };
                browse.FlatAppearance.BorderColor = palette.Border;
                browse.Click += delegate { using (var picker = new FolderBrowserDialog()) if (picker.ShowDialog(this) == DialogResult.OK) path.Text = picker.SelectedPath; };
                pathRow.Controls.Add(browse, 1, 0);
                table.Controls.Add(pathRow, 1, 1);
                scan.Dock = DockStyle.Fill; scan.ForeColor = palette.Text; scan.BackColor = palette.Background; scan.UseMnemonic = false;
                if (supportsScan) table.Controls.Add(scan, 1, 2);
                var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
                var cancel = new Button { Text = Localization.IsEnglish ? "Cancel" : "Abbrechen", Width = (int)(100 * scale), Height = (int)(30 * scale), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, DialogResult = DialogResult.Cancel };
                cancel.FlatAppearance.BorderColor = palette.Border;
                var ok = new Button { Text = "OK", Width = (int)(100 * scale), Height = (int)(30 * scale), FlatStyle = FlatStyle.Flat, BackColor = palette.Primary, ForeColor = palette.PrimaryText, DialogResult = DialogResult.OK, Margin = new Padding(8, 0, 0, 0) };
                ok.FlatAppearance.BorderSize = 0;
                buttons.Controls.Add(cancel); buttons.Controls.Add(ok);
                table.Controls.Add(buttons, 0, 3); table.SetColumnSpan(buttons, 2);
                Controls.Add(table);
                AcceptButton = ok; CancelButton = cancel;
                if (entry != null) { name.Text = entry.Name; path.Text = entry.Path; scan.Checked = entry.Scan; }
                FormClosing += delegate(object sender, FormClosingEventArgs e) { if (DialogResult == DialogResult.OK && String.IsNullOrWhiteSpace(path.Text)) { e.Cancel = true; MessageBox.Show(this, Localization.IsEnglish ? "Please select a folder." : "Bitte einen Ordner auswählen.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information); } };
                Shown += delegate { name.Focus(); };
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                NativeDarkMode.ApplyWindow(Handle, true);
            }
        }
    }
}
