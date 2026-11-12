using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeUiSnapshot
    {
        private sealed class ControlState
        {
            public Control Control;
            public Control Parent;
            public int Index;
            public int Depth;
            public Rectangle Bounds;
            public DockStyle Dock;
            public AnchorStyles Anchor;
            public bool Visible;
            public bool Enabled;
            public bool AutoScroll;
            public Padding Padding;
            public Padding Margin;
            public string Text;
            public object Tag;
            public Font Font;
            public Color BackColor;
            public Color ForeColor;
            public Size MinimumSize;
            public AutoScaleMode AutoScaleMode;
            public int ComboItemHeight;
            public Dictionary<ColumnHeader, int> ColumnWidths;
        }

        private readonly Form form;
        private readonly Control[] topLevel;
        private readonly List<ControlState> states = new List<ControlState>();
        private readonly HashSet<Control> seen = new HashSet<Control>();
        private readonly string formText;
        private readonly bool formAutoScroll;
        private readonly Size formMinimumSize;
        private readonly Size formSize;
        private readonly FormStartPosition formStartPosition;
        private readonly IButtonControl acceptButton;
        private readonly IButtonControl cancelButton;
        private readonly bool keyPreview;

        public NativeUiSnapshot(Form form, IEnumerable<Control> sections)
        {
            this.form = form;
            topLevel = form.Controls.Cast<Control>().ToArray();
            formText = form.Text;
            formAutoScroll = form.AutoScroll;
            formMinimumSize = form.MinimumSize;
            formSize = form.Size;
            formStartPosition = form.StartPosition;
            acceptButton = form.AcceptButton;
            cancelButton = form.CancelButton;
            keyPreview = form.KeyPreview;

            foreach (Control child in form.Controls.Cast<Control>().ToArray())
            {
                Capture(child, 0);
            }
            foreach (Control section in sections)
            {
                Capture(section, 0);
            }
        }

        private void Capture(Control control, int depth)
        {
            if (control == null || !seen.Add(control))
            {
                return;
            }

            var state = new ControlState
            {
                Control = control,
                Parent = control.Parent,
                Index = control.Parent == null ? -1 : control.Parent.Controls.GetChildIndex(control),
                Depth = depth,
                Bounds = control.Bounds,
                Dock = control.Dock,
                Anchor = control.Anchor,
                Visible = control.Visible,
                Enabled = control.Enabled,
                Padding = control.Padding,
                Margin = control.Margin,
                Text = control.Text,
                Tag = control.Tag,
                Font = control.Font,
                BackColor = control.BackColor,
                ForeColor = control.ForeColor,
                MinimumSize = control.MinimumSize
            };

            var scrollable = control as ScrollableControl;
            if (scrollable != null)
            {
                state.AutoScroll = scrollable.AutoScroll;
            }

            var container = control as ContainerControl;
            if (container != null)
            {
                state.AutoScaleMode = container.AutoScaleMode;
            }

            var combo = control as ComboBox;
            if (combo != null)
            {
                state.ComboItemHeight = combo.ItemHeight;
            }

            var list = control as ListView;
            if (list != null && list.Columns.Count > 0)
            {
                state.ColumnWidths = new Dictionary<ColumnHeader, int>();
                foreach (ColumnHeader column in list.Columns)
                {
                    state.ColumnWidths[column] = column.Width;
                }
            }

            states.Add(state);

            foreach (Control child in control.Controls.Cast<Control>().ToArray())
            {
                Capture(child, depth + 1);
            }
        }

        public void Restore()
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            form.SuspendLayout();
            try
            {
                foreach (ControlState state in states.OrderBy(item => item.Depth))
                {
                    Control control = state.Control;
                    if (control == null || control.IsDisposed)
                    {
                        continue;
                    }

                    if (state.Parent != null && !state.Parent.IsDisposed)
                    {
                        if (!Object.ReferenceEquals(control.Parent, state.Parent))
                        {
                            state.Parent.Controls.Add(control);
                        }

                        if (state.Index >= 0 && state.Index < state.Parent.Controls.Count)
                        {
                            state.Parent.Controls.SetChildIndex(control, state.Index);
                        }
                    }

                    control.Dock = state.Dock;
                    control.Anchor = state.Anchor;
                    control.Bounds = state.Bounds;
                    control.Visible = state.Visible;
                    control.Enabled = state.Enabled;
                    control.Padding = state.Padding;
                    control.Margin = state.Margin;
                    control.Text = state.Text;
                    control.Tag = state.Tag;
                    control.Font = state.Font;
                    control.BackColor = state.BackColor;
                    control.ForeColor = state.ForeColor;
                    control.MinimumSize = state.MinimumSize;

                    var scrollable = control as ScrollableControl;
                    if (scrollable != null && !(control is ToolStrip))
                    {
                        scrollable.AutoScroll = state.AutoScroll;
                    }

                    var container = control as ContainerControl;
                    if (container != null)
                    {
                        container.AutoScaleMode = state.AutoScaleMode;
                    }

                    var combo = control as ComboBox;
                    if (combo != null)
                    {
                        combo.ItemHeight = state.ComboItemHeight;
                    }

                    var list = control as ListView;
                    if (list != null && state.ColumnWidths != null)
                    {
                        foreach (KeyValuePair<ColumnHeader, int> pair in state.ColumnWidths)
                        {
                            if (pair.Key != null)
                            {
                                pair.Key.Width = pair.Value;
                            }
                        }
                    }
                }

                var originalTopLevel = new HashSet<Control>(topLevel);
                foreach (Control control in form.Controls.Cast<Control>().ToArray())
                {
                    if (!originalTopLevel.Contains(control))
                    {
                        form.Controls.Remove(control);
                        control.Dispose();
                    }
                }

                foreach (Control control in topLevel)
                {
                    if (control != null && !control.IsDisposed && control.Parent == null)
                    {
                        form.Controls.Add(control);
                    }
                }

                form.Text = formText;
                form.AutoScroll = formAutoScroll;
                form.MinimumSize = formMinimumSize;
                form.Size = formSize;
                form.StartPosition = formStartPosition;
                form.AcceptButton = acceptButton;
                form.CancelButton = cancelButton;
                form.KeyPreview = keyPreview;
            }
            finally
            {
                form.ResumeLayout(true);
                form.Invalidate(true);
            }
        }
    }
}
