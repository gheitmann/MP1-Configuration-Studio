using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    /// <summary>Dark themed dropdown that replaces the native white framed combo box.</summary>
    internal sealed class StudioChoice : Control
    {
        private readonly List<ChoiceOption> items = new List<ChoiceOption>();
        private readonly float scale;
        private int selectedIndex = -1;
        private bool hover;
        private bool open;
        private ToolStripDropDown drop;
        public Func<Palette> Colors;

        public StudioChoice(float scale)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Height = S(32);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.ComboBox;
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        public int Count { get { return items.Count; } }
        public IList<ChoiceOption> Options { get { return items; } }
        public bool DroppedDown { get { return open; } }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetIndex(value, true); }
        }

        public string SelectedValue
        {
            get { return selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex].Value : String.Empty; }
        }

        public bool SelectValue(string value)
        {
            int index = items.FindIndex(option => String.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;
            SetIndex(index, true);
            return true;
        }

        public void SetOptions(IEnumerable<ChoiceOption> options, string currentValue)
        {
            items.Clear();
            items.AddRange(options);
            int index = items.FindIndex(option => String.Equals(option.Value, currentValue, StringComparison.OrdinalIgnoreCase));
            if (index < 0 && !String.IsNullOrWhiteSpace(currentValue))
            {
                items.Add(new ChoiceOption(currentValue, currentValue.Trim()));
                index = items.Count - 1;
            }
            selectedIndex = index >= 0 ? index : (items.Count > 0 ? 0 : -1);
            Invalidate();
        }

        private void SetIndex(int value, bool raise)
        {
            if (items.Count == 0) return;
            value = Math.Max(0, Math.Min(items.Count - 1, value));
            if (value == selectedIndex) { Invalidate(); return; }
            selectedIndex = value;
            Invalidate();
            if (raise && SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
        }

        public event EventHandler SelectedIndexChanged;

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            Color fill = StudioChoice.Blend(palette.Input, palette.Hover, hover || open ? 0.22f : 0f);
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), S(5)))
            {
                using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
                Color border = open || Focused ? palette.Accent : (hover ? Blend(palette.Border, palette.Accent, 0.55f) : palette.Border);
                using (var pen = new Pen(border)) e.Graphics.DrawPath(pen, path);
            }
            string text = selectedIndex >= 0 && selectedIndex < items.Count ? items[selectedIndex].Label : String.Empty;
            TextRenderer.DrawText(e.Graphics, text, Font, new Rectangle(S(10), 0, Math.Max(0, Width - S(38)), Height), palette.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            DrawChevron(e.Graphics, palette, open || hover ? palette.Accent : palette.Muted);
            if (Focused && !open)
                using (var pen = new Pen(Color.FromArgb(90, palette.Accent)))
                    e.Graphics.DrawLine(pen, S(10), Height - S(4), Width - S(10), Height - S(4));
        }

        private void DrawChevron(Graphics g, Palette palette, Color color)
        {
            int x = Width - S(19);
            int y = Height / 2 - S(2);
            using (var pen = new Pen(color, S(2)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, x - S(4), y, x, y + S(4));
                g.DrawLine(pen, x, y + S(4), x + S(4), y);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Focus();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)) OpenDropDown();
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Right)
            {
                if (open) return;
                SetIndex(selectedIndex + 1, true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Left)
            {
                if (open) return;
                SetIndex(selectedIndex - 1, true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.F4)
            {
                OpenDropDown();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && open)
            {
                CloseDropDown();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                case Keys.Up:
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        private void OpenDropDown()
        {
            if (items.Count == 0 || open) return;
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            int itemHeight = S(30);
            int width = Math.Max(Width, Math.Min(S(620), MeasureWidth() + S(54)));
            int visible = Math.Min(items.Count, 12);
            var list = new StudioChoiceList(items, selectedIndex, palette, Font, itemHeight, scale);
            list.Chosen += delegate(int index)
            {
                CloseDropDown();
                SetIndex(index, true);
            };
            list.CloseRequested += delegate { CloseDropDown(); };
            var host = new ToolStripControlHost(list) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = false, Size = new Size(width, visible * itemHeight + S(2)) };
            drop = new ToolStripDropDown { Padding = Padding.Empty, Margin = Padding.Empty, AutoSize = false, DropShadowEnabled = true, BackColor = palette.Panel, Size = new Size(width, visible * itemHeight + S(2)) };
            drop.Items.Add(host);
            drop.Closed += delegate(object sender, ToolStripDropDownClosedEventArgs args)
            {
                open = false;
                Invalidate();
            };
            open = true;
            try
            {
                drop.Show(this, new Point(0, Height + S(2)));
                list.Focus();
                list.EnsureIndexVisible(selectedIndex);
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Choice dropdown could not be shown");
                CloseDropDown();
            }
            Invalidate();
        }

        private int MeasureWidth()
        {
            int width = S(120);
            foreach (ChoiceOption option in items)
                width = Math.Max(width, TextRenderer.MeasureText(option.Label ?? String.Empty, Font).Width);
            return width;
        }

        private void CloseDropDown()
        {
            if (drop == null) return;
            ToolStripDropDown closing = drop;
            drop = null;
            try { closing.Close(); } catch { }
            try { closing.Dispose(); } catch { }
            open = false;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) CloseDropDown();
            base.Dispose(disposing);
        }

        internal static Color Blend(Color first, Color second, float amount)
        {
            return Color.FromArgb(
                (int)(first.R + (second.R - first.R) * amount),
                (int)(first.G + (second.G - first.G) * amount),
                (int)(first.B + (second.B - first.B) * amount));
        }
    }

    internal sealed class StudioChoiceList : ListBox
    {
        private readonly Palette palette;
        private readonly float scale;
        private readonly int itemHeight;
        private int hoverIndex = -1;

        public event Action<int> Chosen;
        public event Action CloseRequested;

        public StudioChoiceList(List<ChoiceOption> options, int selected, Palette palette, Font font, int itemHeight, float scale)
        {
            this.palette = palette;
            this.scale = scale;
            this.itemHeight = itemHeight;
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;
            ItemHeight = itemHeight;
            Font = font;
            BackColor = palette.Panel;
            ForeColor = palette.Text;
            TabStop = true;
            DoubleBuffered = true;
            foreach (ChoiceOption option in options) Items.Add(option);
            if (selected >= 0 && selected < Items.Count) SelectedIndex = selected;
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeDarkMode.Apply(Handle);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool hovered = e.Index == hoverIndex;
            Color fill = selected ? palette.Selection : (hovered ? palette.Hover : palette.Panel);
            using (var brush = new SolidBrush(fill)) e.Graphics.FillRectangle(brush, e.Bounds);
            if (selected)
                using (var brush = new SolidBrush(palette.Accent)) e.Graphics.FillRectangle(brush, e.Bounds.Left, e.Bounds.Top + S(3), S(3), e.Bounds.Height - S(6));
            ChoiceOption option = (ChoiceOption)Items[e.Index];
            Color text = selected ? palette.SelectionText : palette.Text;
            TextRenderer.DrawText(e.Graphics, option.Label ?? String.Empty, Font,
                new Rectangle(e.Bounds.Left + S(12), e.Bounds.Top, Math.Max(0, e.Bounds.Width - S(44)), e.Bounds.Height), text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (selected)
                StudioIcons.Draw(e.Graphics, "check", new Rectangle(e.Bounds.Right - S(30), e.Bounds.Top + (itemHeight - S(14)) / 2, S(14), S(14)), palette.Accent);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = IndexFromPoint(e.Location);
            if (index != hoverIndex)
            {
                hoverIndex = index;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoverIndex != -1) { hoverIndex = -1; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            int index = IndexFromPoint(e.Location);
            if (index >= 0 && index < Items.Count && Chosen != null) Chosen(index);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                int index = SelectedIndex;
                if (index < 0) index = hoverIndex;
                if (index >= 0 && index < Items.Count && Chosen != null) Chosen(index);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Tab)
            {
                if (CloseRequested != null) CloseRequested();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        public void EnsureIndexVisible(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            int visible = Math.Max(1, ClientSize.Height / itemHeight);
            if (index < TopIndex) TopIndex = index;
            else if (index >= TopIndex + visible) TopIndex = Math.Max(0, index - visible + 1);
        }
    }

    /// <summary>Editable combo box with the same dark frame as the other Studio controls.</summary>
    internal sealed class StudioComboInput : Control
    {
        private readonly float scale;
        private readonly TextBox inner;
        private readonly List<string> items = new List<string>();
        private bool hover;
        private bool open;
        private ToolStripDropDown drop;
        public Func<Palette> Colors;

        public StudioComboInput(float scale, Font font, Palette palette)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = S(32);
            AccessibleRole = AccessibleRole.ComboBox;
            inner = new TextBox { BorderStyle = BorderStyle.None, Font = font, BackColor = palette.Input, ForeColor = palette.Text };
            Controls.Add(inner);
            inner.TextChanged += delegate { OnTextChanged(EventArgs.Empty); };
            inner.GotFocus += delegate { Invalidate(); };
            inner.LostFocus += delegate { Invalidate(); };
            inner.Leave += delegate { OnLeave(EventArgs.Empty); };
            inner.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.F4 || (e.Alt && e.KeyCode == Keys.Down))
                {
                    OpenDropDown();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            inner.SetBounds(S(9), Math.Max(1, (Height - inner.PreferredHeight) / 2), Math.Max(60, Width - S(40)), inner.PreferredHeight);
        }

        private int S(float value) { return (int)Math.Round(value * scale); }

        public TextBox Box { get { return inner; } }
        public List<string> Items { get { return items; } }

        public override string Text
        {
            get { return inner.Text; }
            set { inner.Text = value; }
        }

        public string SelectedItem
        {
            get { return inner.Text; }
            set { inner.Text = value ?? String.Empty; }
        }

        public event EventHandler SelectedIndexChanged;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (inner == null) return;
            int inset = S(9);
            inner.SetBounds(inset, Math.Max(1, (Height - inner.PreferredHeight) / 2), Math.Max(60, Width - inset - S(34)), inner.PreferredHeight);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.X >= Width - S(34)) OpenDropDown();
            else inner.Focus();
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), S(5)))
            {
                using (var brush = new SolidBrush(palette.Input)) e.Graphics.FillPath(brush, path);
                Color border = open || inner.Focused ? palette.Accent : (hover ? StudioChoice.Blend(palette.Border, palette.Accent, 0.55f) : palette.Border);
                using (var pen = new Pen(border)) e.Graphics.DrawPath(pen, path);
            }
            int x = Width - S(18);
            int y = Height / 2 - S(2);
            using (var pen = new Pen(open || hover ? palette.Accent : palette.Muted, S(2)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLine(pen, x - S(4), y, x, y + S(4));
                e.Graphics.DrawLine(pen, x, y + S(4), x + S(4), y);
            }
        }

        private void OpenDropDown()
        {
            if (open) return;
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            var options = new List<ChoiceOption>();
            foreach (string item in items)
                if (!String.IsNullOrEmpty(item)) options.Add(new ChoiceOption(item, item));
            string current = inner.Text;
            if (!String.IsNullOrWhiteSpace(current) && options.FindIndex(option => String.Equals(option.Value, current, StringComparison.OrdinalIgnoreCase)) < 0)
                options.Add(new ChoiceOption(current, current));
            if (options.Count == 0) return;

            int selected = options.FindIndex(option => String.Equals(option.Value, current, StringComparison.OrdinalIgnoreCase));
            int itemHeight = S(30);
            int width = Math.Max(Width, Math.Min(S(620), MeasureWidth(options) + S(54)));
            int visible = Math.Min(options.Count, 12);
            var list = new StudioChoiceList(options, selected, palette, Font, itemHeight, scale);
            list.Chosen += delegate(int index)
            {
                CloseDropDown();
                if (index < 0 || index >= options.Count) return;
                inner.Text = options[index].Value;
                if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
            };
            list.CloseRequested += delegate { CloseDropDown(); };
            var host = new ToolStripControlHost(list) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = false, Size = new Size(width, visible * itemHeight + S(2)) };
            drop = new ToolStripDropDown { Padding = Padding.Empty, Margin = Padding.Empty, AutoSize = false, DropShadowEnabled = true, BackColor = palette.Panel, Size = new Size(width, visible * itemHeight + S(2)) };
            drop.Items.Add(host);
            drop.Closed += delegate { open = false; Invalidate(); };
            open = true;
            try
            {
                drop.Show(this, new Point(0, Height + S(2)));
                list.Focus();
                list.EnsureIndexVisible(selected);
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Editable combo dropdown could not be shown");
                CloseDropDown();
            }
            Invalidate();
        }

        private int MeasureWidth(IEnumerable<ChoiceOption> options)
        {
            int width = S(120);
            foreach (ChoiceOption option in options)
                width = Math.Max(width, TextRenderer.MeasureText(option.Label ?? String.Empty, Font).Width);
            return width;
        }

        private void CloseDropDown()
        {
            if (drop == null) return;
            ToolStripDropDown closing = drop;
            drop = null;
            try { closing.Close(); } catch { }
            try { closing.Dispose(); } catch { }
            open = false;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) CloseDropDown();
            base.Dispose(disposing);
        }
    }

    /// <summary>Large owner drawn check box with hover, pressed and focus feedback.</summary>
    internal sealed class StudioCheck : CheckBox
    {
        private readonly float scale;
        private bool hover;
        private bool pressed;
        public Func<Palette> Colors;
        public Color InkOverride = Color.Empty;

        public StudioCheck(float scale)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            AutoSize = false;
            Height = S(30);
            Cursor = Cursors.Hand;
            UseMnemonic = false;
            AccessibleRole = AccessibleRole.CheckButton;
        }

        private int S(float value) { return (int)Math.Round(value * scale); }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (String.IsNullOrEmpty(AccessibleName)) AccessibleName = Text;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(BackColor);
            if (hover || Focused)
                using (var wash = new SolidBrush(Color.FromArgb(Focused ? 26 : 16, palette.Accent)))
                    e.Graphics.FillRectangle(wash, ClientRectangle);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int size = S(19);
            int left = S(2);
            int top = (Height - size) / 2;
            Rectangle box = new Rectangle(left, top, size, size);
            Color fill = Checked
                ? (pressed ? StudioChoice.Blend(palette.Primary, Color.Black, 0.18f) : (hover ? StudioChoice.Blend(palette.Primary, Color.White, 0.10f) : palette.Primary))
                : (pressed ? palette.Selection : (hover ? StudioChoice.Blend(palette.Input, palette.Hover, 0.45f) : palette.Input));
            Color border = Checked
                ? (hover ? Color.White : StudioChoice.Blend(palette.Primary, Color.White, 0.25f))
                : (hover || Focused ? palette.Accent : palette.Border);
            using (var path = StudioIcons.Rounded(new RectangleF(box.X, box.Y, box.Width - 1, box.Height - 1), S(4)))
            {
                using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
                using (var pen = new Pen(border, Math.Max(1, S(1.4f)))) e.Graphics.DrawPath(pen, path);
            }
            if (Checked)
            {
                using (var pen = new Pen(Color.White, Math.Max(1.6f, S(2.1f))))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    Point[] check =
                    {
                        new Point(box.Left + S(5), box.Top + S(10)),
                        new Point(box.Left + S(8), box.Top + S(13)),
                        new Point(box.Left + S(14), box.Top + S(6))
                    };
                    e.Graphics.DrawLines(pen, check);
                }
            }
            int textLeft = box.Right + S(10);
            TextRenderer.DrawText(e.Graphics, Text, Font,
                new Rectangle(textLeft, 0, Math.Max(0, Width - textLeft - S(12)), Height), InkOverride.IsEmpty ? palette.Text : InkOverride,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (Focused)
            {
                int textWidth = Math.Min(Math.Max(0, Width - textLeft - S(12)), TextRenderer.MeasureText(Text, Font).Width + S(4));
                using (var pen = new Pen(Color.FromArgb(140, palette.Accent)))
                    e.Graphics.DrawRectangle(pen, textLeft - S(4), S(2), Math.Max(1, textWidth + S(8)), Height - S(5));
            }
        }
    }

    /// <summary>Text box with a flat rounded frame instead of the white classic border.</summary>
    internal sealed class StudioTextInput : Control
    {
        private readonly float scale;
        private readonly TextBox inner;
        private bool hover;
        public Func<Palette> Colors;
        public string IconName;
        public string HintText;

        public StudioTextInput(float scale, Font font, Palette palette)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = S(32);
            AccessibleRole = AccessibleRole.Text;
            inner = new TextBox { BorderStyle = BorderStyle.None, Font = font, BackColor = palette.Input, ForeColor = palette.Text };
            Controls.Add(inner);
            inner.GotFocus += delegate { Invalidate(); };
            inner.LostFocus += delegate { Invalidate(); };
            inner.MouseEnter += delegate { hover = true; Invalidate(); };
            inner.MouseLeave += delegate { hover = false; Invalidate(); };
            inner.TextChanged += delegate { OnTextChanged(EventArgs.Empty); };
            ApplyInnerBounds();
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        public TextBox Box { get { return inner; } }

        public void ConfigureIcon(string icon, string hint)
        {
            IconName = icon;
            HintText = hint;
            ApplyInnerBounds();
            Invalidate();
        }

        private void ApplyInnerBounds()
        {
            if (inner == null) return;
            int left = String.IsNullOrEmpty(IconName) ? S(9) : S(30);
            int right = String.IsNullOrEmpty(HintText) ? S(9) : S(58);
            int height = inner.PreferredHeight;
            inner.SetBounds(left, Math.Max(1, (Height - height) / 2), Math.Max(1, Width - left - right), height);
        }

        public override string Text
        {
            get { return inner.Text; }
            set { inner.Text = value; }
        }

        public void SelectAll() { inner.SelectAll(); }
        public void Clear() { inner.Clear(); }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyInnerBounds();
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { inner.Focus(); base.OnMouseDown(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), S(5)))
            {
                using (var brush = new SolidBrush(palette.Input)) e.Graphics.FillPath(brush, path);
                Color border = inner.Focused ? palette.Accent : (hover ? StudioChoice.Blend(palette.Border, palette.Accent, 0.55f) : palette.Border);
                using (var pen = new Pen(border)) e.Graphics.DrawPath(pen, path);
            }
            if (!String.IsNullOrEmpty(IconName))
                StudioIcons.Draw(e.Graphics, IconName, new Rectangle(S(8), (Height - S(16)) / 2, S(16), S(16)), inner.Focused ? palette.Accent : palette.Muted);
            if (!String.IsNullOrEmpty(HintText) && String.IsNullOrEmpty(inner.Text))
                TextRenderer.DrawText(e.Graphics, HintText, Font, new Rectangle(Math.Max(0, Width - S(56)), 0, S(48), Height), palette.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Flicker-free host for page content and subsection swaps.</summary>
    internal sealed class StudioBufferedPanel : Panel
    {
        public StudioBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }

    internal sealed class StudioBufferedTable : TableLayoutPanel
    {
        public StudioBufferedTable()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }

    /// <summary>Panel that wraps the 1.9 style subsection tabs into rows itself.</summary>
    internal sealed class StudioTabStrip : Panel
    {
        private readonly List<Control> items = new List<Control>();
        private readonly int gap;

        public StudioTabStrip(int gap)
        {
            this.gap = gap;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Margin = Padding.Empty;
            Padding = Padding.Empty;
        }

        public void Add(Control control)
        {
            items.Add(control);
            Controls.Add(control);
        }

        public int Reflow()
        {
            int available = Math.Max(80, ClientSize.Width);
            int x = 0, y = 0, rowHeight = 0;
            foreach (Control item in items)
            {
                if (x > 0 && x + item.Width > available)
                {
                    x = 0;
                    y += rowHeight + gap;
                    rowHeight = 0;
                }
                item.SetBounds(x, y, item.Width, item.Height);
                x += item.Width + gap;
                rowHeight = Math.Max(rowHeight, item.Height);
            }
            int height = y + rowHeight;
            if (Height != height) Height = height;
            return height;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Reflow();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(BackColor)) e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }

    /// <summary>Compact dark slider used by the DSP editor.</summary>
    internal sealed class StudioTrackBar : Control
    {
        private readonly float scale;
        private bool hover;
        private bool dragging;
        private int minimum;
        private int maximum = 10;
        private int value;
        private int tickFrequency = 1;
        public Func<Palette> Colors;

        public StudioTrackBar(float scale)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Height = S(30);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.Slider;
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        public int Minimum { get { return minimum; } set { minimum = value; if (this.value < minimum) this.value = minimum; Invalidate(); } }
        public int Maximum { get { return maximum; } set { maximum = Math.Max(value, minimum); if (this.value > maximum) this.value = maximum; Invalidate(); } }
        public int TickFrequency { get { return tickFrequency; } set { tickFrequency = Math.Max(1, value); Invalidate(); } }
        public int SmallChange { get; set; }
        public int LargeChange { get; set; }

        public int Value
        {
            get { return value; }
            set
            {
                int next = Math.Max(minimum, Math.Min(maximum, value));
                if (next == this.value) return;
                this.value = next;
                Invalidate();
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; dragging = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { Focus(); dragging = true; SetFromPoint(e.X); } base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { if (dragging) SetFromPoint(e.X); base.OnMouseMove(e); }
        protected override void OnMouseUp(MouseEventArgs e) { dragging = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        private void SetFromPoint(int x)
        {
            int radius = S(8);
            int travel = Math.Max(1, Width - radius * 2);
            int position = Math.Max(0, Math.Min(travel, x - radius));
            Value = minimum + (int)Math.Round((double)position / travel * (maximum - minimum));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            int step = e.Shift ? Math.Max(1, LargeChange) : Math.Max(1, SmallChange);
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down) { Value -= step; e.Handled = true; }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up) { Value += step; e.Handled = true; }
            else if (e.KeyCode == Keys.Home) { Value = minimum; e.Handled = true; }
            else if (e.KeyCode == Keys.End) { Value = maximum; e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(Parent != null ? Parent.BackColor : palette.Panel);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int radius = S(8);
            int mid = Height / 2;
            int left = radius;
            int right = Width - radius;
            Color groove = Enabled ? palette.Border : StudioChoice.Blend(palette.Border, palette.Background, 0.5f);
            using (var pen = new Pen(groove, S(4)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLine(pen, left, mid, right, mid);
            }
            if (tickFrequency > 0 && maximum > minimum)
            {
                using (var pen = new Pen(Color.FromArgb(120, palette.Muted), S(1)))
                {
                    for (int tick = minimum; tick <= maximum; tick += tickFrequency)
                    {
                        double ratio = (double)(tick - minimum) / (maximum - minimum);
                        int x = left + (int)Math.Round(ratio * (right - left));
                        e.Graphics.DrawLine(pen, x, mid + S(5), x, mid + S(8));
                    }
                }
            }
            double position = maximum == minimum ? 0 : (double)(value - minimum) / (maximum - minimum);
            int thumbX = left + (int)Math.Round(position * (right - left));
            Color fill = !Enabled ? palette.Muted : (dragging || hover ? StudioChoice.Blend(palette.Accent, Color.White, 0.18f) : palette.Accent);
            using (var brush = new SolidBrush(fill)) e.Graphics.FillEllipse(brush, thumbX - radius, mid - radius, radius * 2, radius * 2);
            if (Focused)
                using (var pen = new Pen(Color.FromArgb(150, palette.Accent))) e.Graphics.DrawEllipse(pen, thumbX - radius - S(2), mid - radius - S(2), radius * 2 + S(4), radius * 2 + S(4));
        }
    }

    /// <summary>Rounded dark button used by the structured editors.</summary>
    internal sealed class StudioButton : Button
    {
        private bool hover;
        public Func<Palette> Colors;
        public bool Primary;

        public StudioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(Parent != null ? Parent.BackColor : palette.Background);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            Color baseColor = Primary ? palette.Primary : (BackColor.A == 255 ? BackColor : palette.Panel);
            Color fill = Enabled ? (hover ? StudioChoice.Blend(baseColor, Color.White, 0.10f) : baseColor) : StudioChoice.Blend(baseColor, palette.Background, 0.45f);
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), 6))
            {
                using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
                if (!Primary)
                    using (var pen = new Pen(hover ? palette.Accent : palette.Border)) e.Graphics.DrawPath(pen, path);
            }
            Color ink = !Enabled ? palette.Muted : (Primary ? palette.PrimaryText : ForeColor);
            TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (Focused)
                using (var pen = new Pen(Color.FromArgb(140, palette.Accent))) e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
        }
    }

    /// <summary>Applies the Studio palette and dark title bar to custom dialogs.</summary>
    internal static class StudioDialog
    {
        public static void Apply(Form form)
        {
            if (form == null) return;
            Palette palette = Palette.For(true);
            form.Icon = StudioIcon.Value;
            form.BackColor = palette.Panel;
            form.ForeColor = palette.Text;
            form.HandleCreated += delegate { NativeDarkMode.ApplyWindow(form.Handle, !Palette.UseLight); };
            Style(form, palette);
        }

        private static void Style(Control parent, Palette palette)
        {
            foreach (Control child in parent.Controls)
            {
                var text = child as TextBox;
                if (text != null)
                {
                    text.BackColor = palette.Input;
                    text.ForeColor = palette.Text;
                    if (!text.Multiline) text.BorderStyle = BorderStyle.None;
                }
                var list = child as ListBox;
                if (list != null)
                {
                    list.BackColor = palette.Input;
                    list.ForeColor = palette.Text;
                }
                var button = child as StudioButton;
                if (button != null)
                {
                    button.Colors = () => palette;
                    button.BackColor = palette.Panel;
                    button.ForeColor = palette.Text;
                }
                Style(child, palette);
            }
        }
    }

    /// <summary>Rounded subsection tab, ported from the 1.9 interface.</summary>
    internal sealed class StudioSubTab : Button
    {
        private readonly float scale;
        private bool hover;
        public bool Selected;
        public Func<Palette> Colors;

        public StudioSubTab(float scale)
        {
            this.scale = scale;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            AutoSize = false;
            Height = S(34);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.PageTab;
        }

        private int S(int value) { return (int)Math.Round(value * scale); }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Palette palette = Colors != null ? Colors() : Palette.For(true);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            using (var path = StudioIcons.Rounded(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), S(6)))
            {
                Color fill = Selected ? palette.Selection : (hover ? palette.Hover : Color.FromArgb(12, 255, 255, 255));
                using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, path);
                Color border = Selected ? palette.Accent : (hover ? palette.Border : Color.FromArgb(70, palette.Border));
                using (var pen = new Pen(border)) e.Graphics.DrawPath(pen, path);
            }
            Color ink = Selected ? (Palette.HighContrastActive ? SystemColors.HighlightText : palette.Accent) : ((hover || Focused) ? (Palette.HighContrastActive ? SystemColors.HighlightText : palette.Text) : palette.Muted);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(S(6), 0, Math.Max(0, Width - S(12)), Height), ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (Focused && !Selected)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -S(4), -S(4)), palette.Accent, BackColor);
        }
    }
}
