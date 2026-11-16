using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeGroupedSectionEditor : UserControl
    {
        private readonly StudioConfiguration configuration;
        private readonly string section;
        private readonly Func<string, string> groupFor;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly TextBox search = new TextBox { BorderStyle = BorderStyle.FixedSingle };
        private readonly Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
        private readonly Label status = new Label();
        private readonly List<KeyValuePair<string, string>> entries;
        private bool building;

        public event EventHandler Changed;

        public NativeGroupedSectionEditor(StudioConfiguration configuration, string section, Func<string, string> groupFor, float scale)
        {
            this.configuration = configuration;
            this.section = section;
            this.groupFor = groupFor;
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            entries = configuration.Entries(section).ToList();
            var body = Typography.Create(10f * scale, FontStyle.Regular);

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = Px(38), ColumnCount = 2, BackColor = palette.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(260)));
            search.Dock = DockStyle.Fill;
            search.Font = body;
            search.BackColor = palette.Input;
            search.ForeColor = palette.Muted;
            search.Margin = new Padding(0, Px(4), Px(8), Px(4));
            search.Text = Localization.T("Einträge filtern …");
            search.GotFocus += delegate { if (search.ForeColor == palette.Muted) { search.Text = string.Empty; search.ForeColor = palette.Text; } };
            search.LostFocus += delegate { if (search.Text.Length == 0) { search.ForeColor = palette.Muted; search.Text = Localization.T("Einträge filtern …"); } };
            search.TextChanged += delegate { if (!building) Build(); };
            top.Controls.Add(search, 0, 0);
            status.AutoSize = false;
            status.Dock = DockStyle.Fill;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            top.Controls.Add(status, 1, 0);

            table.Padding = new Padding(0);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            scroll.Controls.Add(table);
            scroll.Resize += delegate { LayoutTable(); };
            Controls.Add(scroll);
            Controls.Add(top);
            Build();
        }

        public static string TraktGroup(string key)
        {
            if (key == null) return L("Weitere", "Other");
            if (key == "SettingsVersion" || key == "Username" || key == "UserRefreshToken" || key.StartsWith("UserAccessToken", StringComparison.OrdinalIgnoreCase))
                return L("Konto", "Account");
            if (key.EndsWith("DefaultLayout", StringComparison.OrdinalIgnoreCase) || key.StartsWith("DefaultTVCalendar", StringComparison.OrdinalIgnoreCase) || key.StartsWith("DefaultMovieCalendar", StringComparison.OrdinalIgnoreCase))
                return L("Layouts & Kalender", "Layouts & calendar");
            if (key.StartsWith("SortBy", StringComparison.OrdinalIgnoreCase)) return L("Sortierung", "Sorting");
            if (key.StartsWith("MovieRecommendation", StringComparison.OrdinalIgnoreCase) || key.StartsWith("ShowRecommendation", StringComparison.OrdinalIgnoreCase))
                return L("Empfehlungen", "Recommendations");
            if (key.StartsWith("Dashboard", StringComparison.OrdinalIgnoreCase)) return L("Dashboard", "Dashboard");
            if (key.StartsWith("Sync", StringComparison.OrdinalIgnoreCase) || key == "KeepLibraryClean" || key.StartsWith("Blocked", StringComparison.OrdinalIgnoreCase))
                return L("Synchronisierung", "Synchronization");
            if (key.StartsWith("Download", StringComparison.OrdinalIgnoreCase) || key.StartsWith("WebRequest", StringComparison.OrdinalIgnoreCase))
                return L("Netzwerk & Fanart", "Network & fanart");
            if (key.StartsWith("Hide", StringComparison.OrdinalIgnoreCase) || key.StartsWith("GetFriend", StringComparison.OrdinalIgnoreCase) || key.StartsWith("MovingPictures", StringComparison.OrdinalIgnoreCase)
                || key == "SyncRatings" || key == "ShowRateDialogOnWatched" || key.StartsWith("Calendar", StringComparison.OrdinalIgnoreCase))
                return L("Verhalten & Datenschutz", "Behavior & privacy");
            if (key == "TVSeries" || key == "MyVideos" || key == "MyFilms" || key == "OnlineVideos" || key == "MyTVRecordings" || key == "MyTVLive"
                || key == "ArgusRecordings" || key == "ArgusTVLive")
                return L("Scrobbling / Plugins", "Scrobbling / plugins");
            return L("Weitere", "Other");
        }

        public static string OnlineVideosGroup(string key)
        {
            if (key == "searchHistory" || key == "lastFirstRun") return L("Verlauf", "History");
            return L("Ansichten", "Views");
        }

        private static string L(string de, string en) { return Localization.T2(de, en); }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private void LayoutTable()
        {
            table.Width = Math.Max(Px(420), scroll.ClientSize.Width - scroll.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - Px(4));
        }

        private void Build()
        {
            string query = search.ForeColor == palette.Muted ? string.Empty : search.Text.Trim();
            var groups = new List<string>();
            var grouped = new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (query.Length > 0 && entry.Key.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0
                    && (entry.Value ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0) continue;
                string group = groupFor(entry.Key);
                if (!grouped.ContainsKey(group))
                {
                    grouped[group] = new List<KeyValuePair<string, string>>();
                    groups.Add(group);
                }
                grouped[group].Add(entry);
            }

            table.SuspendLayout();
            building = true;
            try
            {
                table.Controls.Clear();
                table.RowStyles.Clear();
                table.RowCount = 0;
                foreach (string group in groups)
                {
                    var title = new Label { Text = group, Font = Typography.Create(11f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, Margin = new Padding(0, Px(8), 0, Px(4)) };
                    table.Controls.Add(title, 0, table.RowCount);
                    table.SetColumnSpan(title, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                    foreach (var entry in grouped[group])
                    {
                        string key = entry.Key;
                        string value = entry.Value ?? string.Empty;
                        bool boolValue = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                            || String.Equals(value, "no", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
                        if (boolValue)
                        {
                            var check = new CheckBox { Text = key, Checked = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) || String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), AutoSize = true, Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Text, BackColor = palette.Background, FlatStyle = FlatStyle.Flat, UseMnemonic = false, Margin = new Padding(0, Px(2), 0, Px(2)) };
                            check.CheckedChanged += delegate { if (!building) { configuration.SetBool(section, key, check.Checked); RaiseChanged(); } };
                            table.Controls.Add(check, 0, table.RowCount);
                            table.SetColumnSpan(check, 2);
                            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            table.RowCount++;
                        }
                        else
                        {
                            var label = new Label { Text = key, Font = Typography.Create(9.5f * scale, FontStyle.Regular), ForeColor = palette.Text, AutoSize = false, Dock = DockStyle.Fill, Height = Px(30), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Margin = new Padding(0, 0, Px(10), 0) };
                            var text = new TextBox { Text = value, Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Margin = new Padding(0) };
                            text.TextChanged += delegate { if (!building) { configuration.Set(section, key, text.Text); RaiseChanged(); } };
                            table.Controls.Add(label, 0, table.RowCount);
                            table.Controls.Add(text, 1, table.RowCount);
                            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                            table.RowCount++;
                        }
                    }
                }
                if (groups.Count == 0)
                {
                    var empty = new Label { Text = L("Keine Einträge gefunden.", "No entries found."), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, Px(6), 0, Px(6)) };
                    table.Controls.Add(empty, 0, table.RowCount);
                    table.SetColumnSpan(empty, 2);
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount++;
                }
                status.Text = entries.Count + (Localization.T(" Einträge"));
            }
            finally
            {
                building = false;
                table.ResumeLayout(true);
            }
            LayoutTable();
            scroll.AutoScrollPosition = Point.Empty;
        }

        private void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
