using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    // Retain original controls and values. Scale their geometry together with
    // text, rather than increasing fonts inside unchanged native rectangles.
    internal sealed class PageZoom : IDisposable
    {
        private readonly Dictionary<Control,Font> originals=new Dictionary<Control,Font>();
        private readonly Dictionary<Control,float> scales=new Dictionary<Control,float>();
        private readonly Dictionary<Control,Size> minima=new Dictionary<Control,Size>();
        private readonly Dictionary<string,Font> fonts=new Dictionary<string,Font>();
        private readonly HashSet<Control> captured=new HashSet<Control>();
        private readonly Dictionary<Control,Rectangle> bounds=new Dictionary<Control,Rectangle>();
        private readonly Dictionary<ColumnHeader,int> columns=new Dictionary<ColumnHeader,int>();
        private readonly Dictionary<Control,Size> originalMinima=new Dictionary<Control,Size>();
        public void Capture(Control page)
        {
            if(!scales.ContainsKey(page)){scales[page]=1;minima[page]=page.MinimumSize;}
            CaptureControls(page);
        }
        private void CaptureControls(Control c)
        {
            if(captured.Add(c)){Font original=c.Font;Font parentFont;if(c.Parent!=null&&originals.TryGetValue(c.Parent,out parentFont)&&!TypeDescriptor.GetProperties(c)["Font"].ShouldSerializeValue(c))original=parentFont;originals[c]=original;bounds[c]=c.Bounds;originalMinima[c]=c.MinimumSize;var container=c as ContainerControl;if(container!=null)container.AutoScaleMode=AutoScaleMode.None;var list=c as ListView;if(list!=null)foreach(ColumnHeader column in list.Columns)columns[column]=column.Width;}
            foreach(Control child in c.Controls)CaptureControls(child);
        }
        public void Apply(Control page,float factor)
        {
            int before=captured.Count;Capture(page);float previous=scales[page];if(Math.Abs(previous-factor)<.001f&&before==captured.Count)return;
            var controls=All(page).ToArray();foreach(Control c in controls)c.SuspendLayout();
            var anchors=controls.ToDictionary(c=>c,c=>c.Anchor);var docks=controls.ToDictionary(c=>c,c=>c.Dock);
            var nativeStates=controls.Where(c=>c is TrackBar||c is ComboBox).OrderBy(c=>c is TrackBar?1:0).Select(c=>new NativePresentationState(c)).ToArray();
            try
            {
                // Anchor layout against a changing viewport must not become the
                // baseline for the next zoom. Otherwise repeated round trips
                // progressively squeeze groups while their fields grow.
                foreach(Control c in controls){c.Dock=DockStyle.None;c.Anchor=AnchorStyles.Top|AnchorStyles.Left;}
                page.MinimumSize=Size.Empty;
                foreach(Control c in controls)
                {
                    Size originalMinimum=originalMinima[c];c.MinimumSize=new Size((int)Math.Ceiling(originalMinimum.Width*factor),(int)Math.Ceiling(originalMinimum.Height*factor));
                    Rectangle originalBounds=bounds[c];c.Bounds=new Rectangle((int)Math.Round(originalBounds.X*factor),(int)Math.Round(originalBounds.Y*factor),(int)Math.Round(originalBounds.Width*factor),(int)Math.Round(originalBounds.Height*factor));
                    Font original=originals[c];string family=original.Name;if(family=="Microsoft Sans Serif"||family=="MS Sans Serif"||family=="Tahoma"||family=="Arial")family="Segoe UI";
                    string key=family+"|"+original.SizeInPoints+"|"+original.Style+"|"+factor;Font font;
                    if(!fonts.TryGetValue(key,out font)){font=new Font(family,original.SizeInPoints*factor,original.Style,GraphicsUnit.Point);fonts[key]=font;}c.Font=font;
                    var combo=c as ComboBox;if(combo!=null&&combo.DrawMode==DrawMode.OwnerDrawFixed)combo.ItemHeight=font.Height+3;
                    var list=c as ListView;if(list!=null)foreach(ColumnHeader column in list.Columns){int width;if(!columns.TryGetValue(column,out width)){width=column.Width;columns[column]=width;}if(width>0)column.Width=(int)Math.Round(width*factor);}
                }
                var minimum=minima[page];page.MinimumSize=new Size((int)Math.Ceiling(minimum.Width*factor),(int)Math.Ceiling(minimum.Height*factor));scales[page]=factor;
            }
            finally{foreach(Control c in controls){c.Anchor=anchors[c];c.Dock=docks[c];}foreach(Control c in controls.Reverse())c.ResumeLayout(true);foreach(var state in nativeStates)state.Dispose();}
        }
        public void EnsureContentFits(Control page)
        {
            var controls=All(page).Where(c=>c.Visible).ToArray();var anchors=controls.ToDictionary(c=>c,c=>c.Anchor);
            foreach(Control c in controls){c.SuspendLayout();c.Anchor=AnchorStyles.Top|AnchorStyles.Left;}
            try
            {
                foreach(Control c in controls.Reverse())
                {
                    if(!(c is GroupBox||c is Panel||c is UserControl||c is TabControl||c is TabPage)||c is FlowLayoutPanel||c is TableLayoutPanel)continue;
                    var scrolling=c as ScrollableControl;if(scrolling!=null&&scrolling.AutoScroll)continue;
                    int width=c.ClientSize.Width,height=c.ClientSize.Height;
                    foreach(Control child in c.Controls)
                    {
                        if(!child.Visible)continue;
                        int right=child.Left+Math.Max(child.Width,child.MinimumSize.Width),bottom=child.Top+Math.Max(child.Height,child.MinimumSize.Height);
                        if(right>width)width=right+Math.Max(4,c.Padding.Right);if(bottom>height)height=bottom+Math.Max(4,c.Padding.Bottom);
                    }
                    if(c is TabControl)
                    {
                        var tabs=(TabControl)c;if(tabs.SelectedTab==null)continue;Rectangle display=tabs.DisplayRectangle;
                        width=Math.Max(c.Width,tabs.SelectedTab.MinimumSize.Width+c.Width-display.Width);height=Math.Max(c.Height,tabs.SelectedTab.MinimumSize.Height+c.Height-display.Height);
                    }
                    else{width+=c.Width-c.ClientSize.Width;height+=c.Height-c.ClientSize.Height;}
                    if(width>c.Width+3||height>c.Height+3){c.MinimumSize=new Size(Math.Max(c.MinimumSize.Width,width),Math.Max(c.MinimumSize.Height,height));if(!(c is TabPage))c.Size=c.MinimumSize;}
                }
            }
            finally{foreach(Control c in controls)c.Anchor=anchors[c];foreach(Control c in controls.Reverse())c.ResumeLayout(true);}
        }
        private IEnumerable<Control> All(Control c){yield return c;foreach(Control child in c.Controls)foreach(Control value in All(child))yield return value;}
        public void Dispose(){foreach(Font font in fonts.Values)font.Dispose();fonts.Clear();}
    }
}
