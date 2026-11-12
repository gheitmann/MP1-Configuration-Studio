using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    public sealed class Theme
    {
        private readonly HashSet<Control> watched = new HashSet<Control>();
        private readonly HashSet<TabControl> drawnTabs = new HashSet<TabControl>();
        private readonly HashSet<ListView> drawnLists = new HashSet<ListView>();
        private readonly Dictionary<Control, Color> originalBack = new Dictionary<Control, Color>();
        private readonly Dictionary<ListViewItem.ListViewSubItem, Color> originalListFore = new Dictionary<ListViewItem.ListViewSubItem, Color>();
        private readonly HashSet<GroupBox> flatGroups=new HashSet<GroupBox>();
        private readonly HashSet<ComboBox> drawnCombos=new HashSet<ComboBox>();
        private readonly Dictionary<Control,ControlChrome> chrome=new Dictionary<Control,ControlChrome>();
        private readonly Dictionary<ListView,ControlChrome> headers=new Dictionary<ListView,ControlChrome>();
        private readonly Dictionary<Control,IntPtr> themedHandles=new Dictionary<Control,IntPtr>();
        private readonly Dictionary<Control,bool> themedDark=new Dictionary<Control,bool>();
        private readonly Dictionary<ListView,IntPtr> themedHeaders=new Dictionary<ListView,IntPtr>();
        private readonly Dictionary<ListView,bool> headerDark=new Dictionary<ListView,bool>();
        private readonly Dictionary<TreeView,NavigationChrome> navigation=new Dictionary<TreeView,NavigationChrome>();
        public Palette Colors { get; private set; }
        public bool Dark { get; private set; }
        public Theme(bool dark) { Set(dark); }
        public void Set(bool dark) { Dark = dark; Colors = Palette.For(dark); }
        [DllImport("uxtheme.dll", CharSet=CharSet.Unicode)] private static extern int SetWindowTheme(IntPtr h, string app, string id);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr h, int attribute, ref int value, int size);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
        // Navigation reuses existing controls. Do not reset native themes or redraw an
        // entire page again merely because the original holder reparents it.
        public void Prepare(Control c)
        {
            if(c==null||c.IsDisposed)return;
            if(!watched.Contains(c)){Apply(c);return;}
            var list=c as ListView;if(list!=null)ColorItems(list);
            foreach(Control child in c.Controls)Prepare(child);
        }
        private static void BufferManagedPainting(Control c)
        {
            var setStyle=typeof(Control).GetMethod("SetStyle",BindingFlags.Instance|BindingFlags.NonPublic);
            setStyle.Invoke(c,new object[]{ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true});
            typeof(Control).GetProperty("DoubleBuffered",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(c,true,null);
        }
        public void Apply(Control c)
        {
            if(c==null||c.IsDisposed)return;
            var states=AllControls(c).Where(control=>control is ComboBox||control is TrackBar).OrderBy(control=>control is TrackBar?1:0).Select(control=>new NativePresentationState(control)).ToArray();
            try{ApplyCore(c);}finally{foreach(var state in states)state.Dispose();}
        }
        private static IEnumerable<Control> AllControls(Control c){yield return c;foreach(Control child in c.Controls)foreach(Control descendant in AllControls(child))yield return descendant;}
        private void ApplyCore(Control c)
        {
            if (c == null || c.IsDisposed) return;
            if (!originalBack.ContainsKey(c)) originalBack[c] = c.BackColor;
            if (watched.Add(c))
            {
                c.ControlAdded += delegate(object sender, ControlEventArgs e) { Prepare(e.Control); };
                c.HandleCreated += delegate { ApplyNative(c); };
                c.HandleDestroyed += delegate {themedHandles.Remove(c);themedDark.Remove(c);var listControl=c as ListView;if(listControl!=null){themedHeaders.Remove(listControl);headerDark.Remove(listControl);}};
                c.Disposed += delegate { watched.Remove(c); originalBack.Remove(c);ControlChrome decoration;if(chrome.TryGetValue(c,out decoration)){decoration.Dispose();chrome.Remove(c);}var listControl=c as ListView;if(listControl!=null&&headers.TryGetValue(listControl,out decoration)){decoration.Dispose();headers.Remove(listControl);}var treeControl=c as TreeView;NavigationChrome treeChrome;if(treeControl!=null&&navigation.TryGetValue(treeControl,out treeChrome)){treeChrome.Dispose();navigation.Remove(treeControl);} };
                if(c is Form||c is GroupBox||c is Panel||c is UserControl||c is TabControl||c is ListView)BufferManagedPainting(c);
            }
            var p = Colors;
            Color nativeBackground=c.BackColor;
            bool semanticInput=c is TextBoxBase&&!nativeBackground.IsSystemColor&&nativeBackground.GetSaturation()>.35f&&!IsPaletteSurface(nativeBackground);
            c.ForeColor = p.Text;
            // Preserve swatches, artwork and semantic custom colors in native/plugin pages.
            Color original = originalBack[c];
            bool swatch = (c is PictureBox) || (c is Panel && c.Name.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0 && original != SystemColors.Control);
            Color surface=IsNativePage(c)||IsStudioContent(c)?p.Background:p.Panel;
            if (!swatch&&!semanticInput) c.BackColor = c is TextBoxBase || c is ComboBox || c is ListBox || c is ListView || c is NumericUpDown ? p.Input : surface;
            if(semanticInput)c.ForeColor=ReadableForeground(p.Text,nativeBackground,Color.Black);
            var group=c as GroupBox;
            if(group!=null&&flatGroups.Add(group))
            {
                group.FlatStyle=FlatStyle.Flat;
                group.Paint+=delegate(object sender,PaintEventArgs e){
                    e.Graphics.Clear(group.BackColor);
                    if(!String.IsNullOrEmpty(group.Text))TextRenderer.DrawText(e.Graphics,group.Text,group.Font,new Rectangle(7,0,Math.Max(0,group.Width-14),group.Font.Height+4),group.Enabled?Colors.Text:Colors.Muted,TextFormatFlags.Top|TextFormatFlags.EndEllipsis);
                    using(var pen=new Pen(Colors.Border))e.Graphics.DrawLine(pen,7,group.Height-1,Math.Max(7,group.Width-8),group.Height-1);
                };
            }
            var textBox=c as TextBox;
            if(textBox!=null){int height=textBox.Height;if(!textBox.Multiline)textBox.AutoSize=false;textBox.BorderStyle=BorderStyle.None;textBox.Height=height;}
            var rich=c as RichTextBox;if(rich!=null)rich.BorderStyle=BorderStyle.None;
            var listBox=c as ListBox;if(listBox!=null)listBox.BorderStyle=BorderStyle.None;
            var panel=c as Panel;if(panel!=null&&!swatch)panel.BorderStyle=BorderStyle.None;
            var picture=c as PictureBox;if(picture!=null)picture.BorderStyle=BorderStyle.None;
            var numeric=c as NumericUpDown;if(numeric!=null)numeric.BorderStyle=BorderStyle.None;
            var combo=c as ComboBox;
            if(combo!=null||c is TextBoxBase)SetProperty(c,"BorderColor",Color.Empty);
            if(combo!=null)combo.FlatStyle=FlatStyle.Flat;
            if(combo!=null&&(drawnCombos.Contains(combo)||combo.DrawMode==DrawMode.Normal))
            {
                combo.FlatStyle=FlatStyle.Flat;combo.DrawMode=DrawMode.OwnerDrawFixed;
                if(drawnCombos.Add(combo))combo.DrawItem+=delegate(object sender,DrawItemEventArgs e){
                    Color background=(e.State&DrawItemState.Selected)!=0?Colors.Hover:Colors.Input;
                    using(var brush=new SolidBrush(background))e.Graphics.FillRectangle(brush,e.Bounds);
                    string text=e.Index>=0&&e.Index<combo.Items.Count?combo.GetItemText(combo.Items[e.Index]):combo.Text;
                    TextRenderer.DrawText(e.Graphics,text,combo.Font,e.Bounds,combo.Enabled?Colors.Text:Colors.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
                    if((e.State&DrawItemState.Focus)!=0)e.DrawFocusRectangle();
                };
            }
            var form = c as Form;
            if (form != null) form.BackColor = p.Background;
            var button = c as Button;
            if (button != null)
            {
                button.UseVisualStyleBackColor = false; button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize=0;
                button.FlatAppearance.BorderColor = p.Border; button.FlatAppearance.MouseOverBackColor = p.Hover;
                button.FlatAppearance.MouseDownBackColor = p.Selection;
                if (Convert.ToString(button.Tag) == "studio-primary") { button.BackColor = p.Primary; button.ForeColor = Color.White; }
            }
            var checkbox = c as CheckBox;
            if (checkbox != null) checkbox.UseVisualStyleBackColor = false;
            var radio = c as RadioButton;
            if (radio != null) radio.UseVisualStyleBackColor = false;
            var link = c as LinkLabel;
            if (link != null) { link.LinkColor = p.Accent; link.ActiveLinkColor = p.Accent; link.VisitedLinkColor = p.Accent; }
            var tree = c as TreeView;
            if (tree != null) { tree.LineColor = p.Border; tree.BorderStyle = BorderStyle.None; }
            var strip = c as ToolStrip;
            if (strip != null)
            {
                strip.Renderer = new ToolStripProfessionalRenderer(new StudioColorTable(p));
                foreach (ToolStripItem item in strip.Items) ThemeItem(item, p);
            }
            var grid = c as DataGridView;
            if (grid != null)
            {
                grid.EnableHeadersVisualStyles = false; grid.BackgroundColor = p.Input; grid.GridColor = p.Border;
                grid.BorderStyle=BorderStyle.None;grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.RowHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
                grid.DefaultCellStyle.BackColor = p.Input; grid.DefaultCellStyle.ForeColor = p.Text;
                grid.DefaultCellStyle.SelectionBackColor = p.Selection; grid.DefaultCellStyle.SelectionForeColor = p.Text;
                grid.ColumnHeadersDefaultCellStyle.BackColor = p.Panel; grid.ColumnHeadersDefaultCellStyle.ForeColor = p.Text;
                grid.RowHeadersDefaultCellStyle.BackColor = p.Panel; grid.RowHeadersDefaultCellStyle.ForeColor = p.Text;
            }
            var tabs = c as TabControl;
            if (tabs != null && (drawnTabs.Contains(tabs) || tabs.DrawMode == TabDrawMode.Normal))
            {
                tabs.Padding=new Point(Math.Max(14,tabs.Font.Height),8);
                tabs.ItemSize=new Size(tabs.ItemSize.Width,Math.Max(34,tabs.Font.Height+16));
                tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
                if (drawnTabs.Add(tabs)) tabs.DrawItem += delegate(object sender, DrawItemEventArgs e) {
                    if (e.Index < 0 || e.Index >= tabs.TabPages.Count) return;
                    bool active = tabs.SelectedIndex == e.Index;
                    using (var brush = new SolidBrush(active ? Colors.Selection : Colors.Panel)) e.Graphics.FillRectangle(brush, e.Bounds);
                    TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, e.Bounds, active ? Colors.Accent : Colors.Text, TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter);
                };
            }
            var list = c as ListView;
            if (list != null)
            {
                list.BorderStyle = BorderStyle.None;
                ColorItems(list);
                // Leave custom drawing to plugin authors; only own-draw standard details headers.
                if (drawnLists.Contains(list) || (!list.OwnerDraw && list.View == View.Details))
                {
                    list.OwnerDraw = true;
                    if (drawnLists.Add(list))
                    {
                        list.DrawColumnHeader += delegate(object sender, DrawListViewColumnHeaderEventArgs e) {
                            using(var b = new SolidBrush(Colors.Panel)) e.Graphics.FillRectangle(b,e.Bounds);
                            TextRenderer.DrawText(e.Graphics,e.Header.Text,list.Font,e.Bounds,Colors.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
                        };
                        list.DrawItem += delegate(object sender, DrawListViewItemEventArgs e) { e.DrawDefault = list.View != View.Details; };
                        list.DrawSubItem += delegate(object sender, DrawListViewSubItemEventArgs e) { e.DrawDefault=false;PaintDetailsCell(e.Graphics,list,e.Item,e.SubItem,e.ColumnIndex,e.Bounds,Colors,e.Item.Selected||(e.ItemState&ListViewItemStates.Selected)!=0,(e.ItemState&ListViewItemStates.Hot)!=0); };
                    }
                }
            }
            if (c.GetType().Name == "MPGradientLabel")
            {
                SetProperty(c,"FirstColor",p.Panel); SetProperty(c,"LastColor",p.Panel); SetProperty(c,"TextColor",p.Text);
            }
            ApplyNative(c);
            foreach (Control child in c.Controls) ApplyCore(child);
            c.Invalidate();
        }
        private static bool IsPaletteSurface(Color color)
        {
            foreach(bool dark in new[]{false,true}){Palette p=Palette.For(dark);if(color.ToArgb()==p.Input.ToArgb()||color.ToArgb()==p.Panel.ToArgb()||color.ToArgb()==p.Background.ToArgb())return true;}
            return false;
        }
        private void ApplyNative(Control c)
        {
            if (!c.IsHandleCreated || c.IsDisposed) return;
            IntPtr oldHandle;bool oldDark;
            bool changed=!themedHandles.TryGetValue(c,out oldHandle)||oldHandle!=c.Handle||!themedDark.TryGetValue(c,out oldDark)||oldDark!=Dark;
            if (changed&&(c is TreeView || c is ListView || c is TextBoxBase || c is ListBox))
                try { SetWindowTheme(c.Handle, Dark ? "DarkMode_Explorer" : "Explorer", null); } catch (DllNotFoundException) { }
            themedHandles[c]=c.Handle;themedDark[c]=Dark;
            if(c is TreeView)
            {
                var tree=(TreeView)c;NavigationChrome decoration;
                if(!navigation.TryGetValue(tree,out decoration)){decoration=new NavigationChrome();navigation[tree]=decoration;}
                decoration.Attach(tree.Handle);
            }
            if(c is ListView)
            {
                SendMessage(c.Handle,0x1036,new IntPtr(0x10000),new IntPtr(0x10000)); // LVS_EX_DOUBLEBUFFER
                IntPtr header=SendMessage(c.Handle,0x101f,IntPtr.Zero,IntPtr.Zero);
                var list=(ListView)c;
                if(header!=IntPtr.Zero&&(!themedHeaders.TryGetValue(list,out oldHandle)||oldHandle!=header||!headerDark.TryGetValue(list,out oldDark)||oldDark!=Dark))
                {try{SetWindowTheme(header,Dark?"DarkMode_Explorer":"Explorer",null);}catch(DllNotFoundException){}themedHeaders[list]=header;headerDark[list]=Dark;}
                if(header!=IntPtr.Zero&&drawnLists.Contains(list))
                {
                    ControlChrome decoration;if(!headers.TryGetValue(list,out decoration)){decoration=new ControlChrome(list,()=>Colors,ControlChrome.ChromeKind.Header);headers[list]=decoration;}decoration.Attach(header);
                }
            }
            if(c is ComboBox||c is TabControl||c is TrackBar)
            {
                ControlChrome decoration;if(!chrome.TryGetValue(c,out decoration)){decoration=new ControlChrome(c,()=>Colors,c is ComboBox?ControlChrome.ChromeKind.Combo:c is TrackBar?ControlChrome.ChromeKind.Slider:ControlChrome.ChromeKind.Tabs);chrome[c]=decoration;}
                if(c is ComboBox)decoration.OwnComboContent=drawnCombos.Contains((ComboBox)c)&&((ComboBox)c).DropDownStyle!=ComboBoxStyle.Simple;
                decoration.Attach(c.Handle);
            }
            if (changed&&c is Form)
            {
                FrameRendering.EnableComposition(c);
                int v = Dark ? 1 : 0;
                try { DwmSetWindowAttribute(c.Handle,20,ref v,4); } catch (DllNotFoundException) { }
            }
        }
        internal static void PaintSelectedCell(Graphics g,ListView list,ListViewItem item,ListViewItem.ListViewSubItem cell,int column,Rectangle bounds,Palette p)
        {PaintDetailsCell(g,list,item,cell,column,bounds,p,true);}
        private static void PaintDetailsCell(Graphics g,ListView list,ListViewItem item,ListViewItem.ListViewSubItem cell,int column,Rectangle bounds,Palette p,bool selected,bool hover=false)
        {
            using(var brush=new SolidBrush(hover?p.Hover:selected?p.Selection:p.Input))g.FillRectangle(brush,bounds);
            Rectangle text=Rectangle.Inflate(bounds,-5,0);
            if(column==0)
            {
                if(list.CheckBoxes){Size size=CheckBoxRenderer.GetGlyphSize(g,System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);CheckBoxRenderer.DrawCheckBox(g,new Point(text.Left,text.Top+(text.Height-size.Height)/2),item.Checked?System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal:System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);text.X+=size.Width+5;text.Width-=size.Width+5;}
                if(list.SmallImageList!=null){int image=item.ImageIndex;if(!String.IsNullOrEmpty(item.ImageKey))image=list.SmallImageList.Images.IndexOfKey(item.ImageKey);if(image>=0&&image<list.SmallImageList.Images.Count){list.SmallImageList.Draw(g,text.Left,text.Top+(text.Height-list.SmallImageList.ImageSize.Height)/2,image);int offset=list.SmallImageList.ImageSize.Width+5;text.X+=offset;text.Width-=offset;}}
                if(selected)using(var brush=new SolidBrush(p.Accent))g.FillRectangle(brush,bounds.Left,bounds.Top,3,bounds.Height);
            }
            TextFormatFlags flags=TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix|TextFormatFlags.SingleLine;
            if(column<list.Columns.Count){if(list.Columns[column].TextAlign==HorizontalAlignment.Right)flags|=TextFormatFlags.Right;else if(list.Columns[column].TextAlign==HorizontalAlignment.Center)flags|=TextFormatFlags.HorizontalCenter;}
            TextRenderer.DrawText(g,cell.Text,list.Font,text,selected||hover?p.Text:ReadableForeground(cell.ForeColor,p.Input,p.Text),flags);
        }
        private void ColorItems(ListView list)
        {
            var p=Colors;
            foreach(ListViewItem item in list.Items)
            {
                if(item.BackColor!=p.Input)item.BackColor=p.Input;
                foreach(ListViewItem.ListViewSubItem cell in item.SubItems)
                {
                    if(!originalListFore.ContainsKey(cell))originalListFore[cell]=cell.ForeColor;
                    Color originalText=originalListFore[cell];
                    Color foreground=originalText.IsSystemColor||originalText.GetSaturation()<.2f?p.Text:ReadableForeground(originalText,p.Input,p.Text);
                    if(cell.BackColor!=p.Input)cell.BackColor=p.Input;
                    if(cell.ForeColor!=foreground)cell.ForeColor=foreground;
                }
            }
        }
        internal static double Contrast(Color first,Color second)
        {
            Func<Color,double> luminance=delegate(Color c){Func<byte,double> channel=v=>{double n=v/255d;return n<=.04045?n/12.92:Math.Pow((n+.055)/1.055,2.4);};return .2126*channel(c.R)+.7152*channel(c.G)+.0722*channel(c.B);};
            double a=luminance(first),b=luminance(second);return (Math.Max(a,b)+.05)/(Math.Min(a,b)+.05);
        }
        internal static Color ReadableForeground(Color original,Color background,Color fallback)
        {
            if(Contrast(original,background)>=4.5)return original;
            for(int step=1;step<=20;step++){double mix=step/20d;Color candidate=Color.FromArgb((int)Math.Round(original.R*(1-mix)+fallback.R*mix),(int)Math.Round(original.G*(1-mix)+fallback.G*mix),(int)Math.Round(original.B*(1-mix)+fallback.B*mix));if(Contrast(candidate,background)>=4.5)return candidate;}
            return fallback;
        }
        private static bool IsNativePage(Control c)
        {
            for(Control parent=c;parent!=null;parent=parent.Parent)
                for(Type t=parent.GetType();t!=null;t=t.BaseType)
                    if(t.FullName=="MediaPortal.Configuration.SectionSettings")return true;
            return false;
        }
        private static bool IsStudioContent(Control c){for(Control p=c;p!=null;p=p.Parent)if(p.Name=="studioContent")return true;return false;}
        private static void SetProperty(object obj,string name,object value)
        {
            var p = obj.GetType().GetProperty(name); if (p != null && p.CanWrite) p.SetValue(obj,value,null);
        }
        private static void ThemeItem(ToolStripItem item,Palette p)
        {
            item.BackColor=p.Panel; item.ForeColor=p.Text;
            var dropdown=item as ToolStripDropDownItem;
            if(dropdown!=null) foreach(ToolStripItem child in dropdown.DropDownItems) ThemeItem(child,p);
        }
    }
    internal sealed class StudioColorTable : ProfessionalColorTable
    {
        private readonly Palette p; public StudioColorTable(Palette colors){p=colors;UseSystemColors=false;}
        public override Color ToolStripGradientBegin{get{return p.Panel;}}
        public override Color ToolStripGradientMiddle{get{return p.Panel;}}
        public override Color ToolStripGradientEnd{get{return p.Panel;}}
        public override Color ToolStripBorder{get{return p.Border;}}
        public override Color MenuItemSelected{get{return p.Hover;}}
        public override Color MenuItemBorder{get{return p.Border;}}
        public override Color MenuItemSelectedGradientBegin{get{return p.Hover;}}
        public override Color MenuItemSelectedGradientEnd{get{return p.Hover;}}
        public override Color MenuBorder{get{return p.Border;}}
        public override Color ToolStripDropDownBackground{get{return p.Panel;}}
        public override Color ImageMarginGradientBegin{get{return p.Panel;}}
        public override Color ImageMarginGradientMiddle{get{return p.Panel;}}
        public override Color ImageMarginGradientEnd{get{return p.Panel;}}
        public override Color ButtonSelectedGradientBegin{get{return p.Selection;}}
        public override Color ButtonSelectedGradientMiddle{get{return p.Selection;}}
        public override Color ButtonSelectedGradientEnd{get{return p.Selection;}}
        public override Color ButtonCheckedGradientBegin{get{return p.Selection;}}
        public override Color ButtonCheckedGradientMiddle{get{return p.Selection;}}
        public override Color ButtonCheckedGradientEnd{get{return p.Selection;}}
    }
}
