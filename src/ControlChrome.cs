using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    // Paint-only adapters: native objects, hit testing, events and accessibility stay intact.
    internal sealed class ControlChrome : NativeWindow, IDisposable
    {
        internal enum ChromeKind { Combo, Header, Tabs, Slider }
        public bool OwnComboContent;
        private readonly Control control;
        private readonly Func<Palette> colors;
        private readonly ChromeKind kind;
        public ControlChrome(Control control, Func<Palette> colors, ChromeKind kind)
        { this.control=control;this.colors=colors;this.kind=kind; }
        public void Attach(IntPtr handle)
        {
            if(Handle==handle)return;
            if(Handle!=IntPtr.Zero)ReleaseHandle();
            if(handle!=IntPtr.Zero)AssignHandle(handle);
        }
        public void Dispose(){if(Handle!=IntPtr.Zero)ReleaseHandle();}
        protected override void WndProc(ref Message m)
        {
            int message=m.Msg;IntPtr printDC=m.WParam;
            if(!control.IsDisposed&&Handle!=IntPtr.Zero&&message==0x0014)
            {m.Result=new IntPtr(1);return;} // Erase into the same buffer as the final chrome.
            if(!control.IsDisposed&&Handle!=IntPtr.Zero&&message==0x000f)
            {
                PaintBuffered();m.Result=IntPtr.Zero;return;
            }
            base.WndProc(ref m);
            if(control.IsDisposed||Handle==IntPtr.Zero)return;
            // WM_PRINT already targets a caller-owned DC (e.g. printing), never the screen.
            if(message==0x0317||message==0x0318)
            {
                if(printDC==IntPtr.Zero)return;
                using(Graphics g=Graphics.FromHdc(printDC))Paint(g);
            }
        }
        [StructLayout(LayoutKind.Sequential)] private struct PaintState
        {
            public IntPtr DC;public int Erase;public Rect Bounds;public int Restore,Update;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=32)] public byte[] Reserved;
        }
        [DllImport("user32.dll")] private static extern IntPtr BeginPaint(IntPtr hwnd,out PaintState state);
        [DllImport("user32.dll")] private static extern bool EndPaint(IntPtr hwnd,ref PaintState state);
        private void PaintBuffered()
        {
            IntPtr hwnd=Handle;PaintState state;IntPtr screen=BeginPaint(hwnd,out state);
            try
            {
                Rect client;if(screen==IntPtr.Zero||!GetClientRect(hwnd,out client)||client.Right<1||client.Bottom<1)return;
                using(var bitmap=new Bitmap(client.Right,client.Bottom))
                using(var buffer=Graphics.FromImage(bitmap))
                {
                    buffer.Clear(kind==ChromeKind.Combo?colors().Input:control.BackColor);
                    if(kind==ChromeKind.Tabs||(kind==ChromeKind.Combo&&!OwnComboContent))
                    {
                        IntPtr memory=buffer.GetHdc();
                        try
                        {
                            // Ask the original control to render to memory. The old frame
                            // must never reach the visible window before our final drawing.
                            Message print=Message.Create(hwnd,0x0318,memory,new IntPtr(0x0c));
                            base.WndProc(ref print);
                        }
                        finally{buffer.ReleaseHdc(memory);}
                    }
                    Paint(buffer);
                    using(var target=Graphics.FromHdc(screen))target.DrawImageUnscaled(bitmap,0,0);
                }
            }
            finally{EndPaint(hwnd,ref state);}
        }
        private void Paint(Graphics g)
        {
            if(kind==ChromeKind.Combo)PaintCombo(g,(ComboBox)control);
            else if(kind==ChromeKind.Header)PaintHeader(g,(ListView)control);
            else if(kind==ChromeKind.Slider)PaintSlider(g,(TrackBar)control);
            else PaintTabs(g,(TabControl)control);
        }
        private void PaintCombo(Graphics g,ComboBox combo)
        {
            Rect client;if(!GetClientRect(Handle,out client))return;
            int width=client.Right,height=client.Bottom;
            if(combo.DropDownStyle==ComboBoxStyle.Simple||width<8||height<8)return;
            Palette p=colors();int arrowWidth=SystemInformation.VerticalScrollBarWidth+3;
            bool rtl=combo.RightToLeft==RightToLeft.Yes;
            if(OwnComboContent)
            {
                g.Clear(p.Input);
                if(combo.DropDownStyle==ComboBoxStyle.DropDownList)
                {
                    var textBounds=new Rectangle(rtl?arrowWidth+3:4,1,Math.Max(0,width-arrowWidth-8),height-2);
                    string text=combo.SelectedIndex>=0?combo.GetItemText(combo.SelectedItem):combo.Text;
                    var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix;
                    if(rtl)flags|=TextFormatFlags.Right|TextFormatFlags.RightToLeft;
                    TextRenderer.DrawText(g,text,combo.Font,textBounds,combo.Enabled?p.Text:p.Muted,flags);
                }
            }
            using(var surface=new SolidBrush(p.Input))
            {
                g.FillRectangle(surface,0,0,width,2);g.FillRectangle(surface,0,height-2,width,2);
                g.FillRectangle(surface,0,0,2,height);g.FillRectangle(surface,width-2,0,2,height);
            }
            var button=new Rectangle(rtl?1:Math.Max(1,width-arrowWidth),1,arrowWidth-1,Math.Max(0,height-2));
            using(var b=new SolidBrush(p.Input))g.FillRectangle(b,button);
            Color ink=combo.Enabled?p.Muted:p.Border;
            int x=button.Left+button.Width/2,y=height/2;
            using(var pen=new Pen(ink,1.4f)){g.DrawLine(pen,x-3,y-1,x,y+2);g.DrawLine(pen,x,y+2,x+3,y-1);}
            using(var pen=new Pen(combo.Focused?p.Accent:p.Border))g.DrawRectangle(pen,0,0,width-1,height-1);
        }
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd,int message,IntPtr w,IntPtr l);
        private void PaintSlider(Graphics g,TrackBar slider)
        {
            var p=colors();g.Clear(slider.BackColor);
            Rect channel=new Rect(),thumb=new Rect();
            SendMessage(Handle,0x041a,IntPtr.Zero,ref channel);SendMessage(Handle,0x0419,IntPtr.Zero,ref thumb);
            bool vertical=slider.Orientation==Orientation.Vertical;
            int center=vertical?(channel.Left+channel.Right)/2:(channel.Top+channel.Bottom)/2;
            using(var rail=new Pen(p.Border,3))
            {
                if(vertical)g.DrawLine(rail,center,channel.Top,center,channel.Bottom);
                else g.DrawLine(rail,channel.Left,center,channel.Right,center);
            }
            if(slider.TickStyle!=TickStyle.None)
            {
                int ticks=SendMessage(Handle,0x0410,IntPtr.Zero,IntPtr.Zero).ToInt32();
                using(var pen=new Pen(p.Muted))
                    for(int i=-1;i<ticks-1;i+=Math.Max(1,ticks/1000))
                    {
                        int pos=i==-1?(vertical?channel.Top:channel.Left):i==ticks-2?(vertical?channel.Bottom:channel.Right):SendMessage(Handle,0x040f,new IntPtr(i),IntPtr.Zero).ToInt32();
                        if(pos<0)continue;
                        if(slider.TickStyle==TickStyle.TopLeft||slider.TickStyle==TickStyle.Both)
                        {if(vertical)g.DrawLine(pen,thumb.Left-5,pos,thumb.Left-2,pos);else g.DrawLine(pen,pos,thumb.Top-5,pos,thumb.Top-2);}
                        if(slider.TickStyle==TickStyle.BottomRight||slider.TickStyle==TickStyle.Both)
                        {if(vertical)g.DrawLine(pen,thumb.Right+2,pos,thumb.Right+5,pos);else g.DrawLine(pen,pos,thumb.Bottom+2,pos,thumb.Bottom+5);}
                    }
            }
            if(thumb.Right>thumb.Left&&thumb.Bottom>thumb.Top)
            {
                using(var brush=new SolidBrush(slider.Enabled?p.Accent:p.Muted))g.FillRectangle(brush,thumb.Left,thumb.Top,thumb.Right-thumb.Left,thumb.Bottom-thumb.Top);
                if(slider.Focused)using(var pen=new Pen(p.Text))g.DrawRectangle(pen,thumb.Left,thumb.Top,thumb.Right-thumb.Left-1,thumb.Bottom-thumb.Top-1);
            }
        }
        [StructLayout(LayoutKind.Sequential)] private struct Rect {public int Left,Top,Right,Bottom;}
        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] private struct HeaderItem
        {public uint Mask;public int Width;public IntPtr Text,Bitmap;public int TextMax,Format;public IntPtr Parameter;public int Image,Order;public uint Type;public IntPtr Filter;public uint State;}
        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hwnd,out Rect bounds);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd,int message,IntPtr w,ref Rect bounds);
        [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr hwnd,int message,IntPtr w,ref HeaderItem item);
        private void PaintHeader(Graphics g,ListView list)
        {
            Palette p=colors();Rect client;if(!GetClientRect(Handle,out client))return;
            using(var b=new SolidBrush(p.Input))g.FillRectangle(b,0,0,client.Right,client.Bottom);
            for(int i=0;i<list.Columns.Count;i++)
            {
                Rect r=new Rect();SendMessage(Handle,0x1207,new IntPtr(i),ref r); // HDM_GETITEMRECT preserves scrolling/order.
                if(r.Right-r.Left<16||r.Bottom<=r.Top)continue;
                var bounds=Rectangle.FromLTRB(r.Left+8,r.Top,r.Right-6,r.Bottom-1);
                if(list.SmallImageList!=null&&list.Columns[i].ImageIndex>=0&&list.Columns[i].ImageIndex<list.SmallImageList.Images.Count)
                {list.SmallImageList.Draw(g,bounds.Left,bounds.Top+(bounds.Height-list.SmallImageList.ImageSize.Height)/2,list.Columns[i].ImageIndex);bounds.X+=list.SmallImageList.ImageSize.Width+5;bounds.Width-=list.SmallImageList.ImageSize.Width+5;}
                var state=new HeaderItem {Mask=4};SendMessage(Handle,0x120b,new IntPtr(i),ref state);
                if((state.Format&0x600)!=0)
                {
                    int x=r.Right-11,y=(r.Top+r.Bottom)/2;bool up=(state.Format&0x400)!=0;
                    using(var pen=new Pen(p.Muted,1.2f)){g.DrawLine(pen,x-3,y+(up?1:-1),x,y+(up?-2:2));g.DrawLine(pen,x,y+(up?-2:2),x+3,y+(up?1:-1));}
                    bounds.Width=Math.Max(0,bounds.Width-15);
                }
                var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix;
                if(list.Columns[i].TextAlign==HorizontalAlignment.Right)flags|=TextFormatFlags.Right;
                else if(list.Columns[i].TextAlign==HorizontalAlignment.Center)flags|=TextFormatFlags.HorizontalCenter;
                TextRenderer.DrawText(g,list.Columns[i].Text,list.Font,bounds,p.Muted,flags);
            }
            using(var pen=new Pen(p.Border))g.DrawLine(pen,0,client.Bottom-1,client.Right,client.Bottom-1);
        }
        private void PaintTabs(Graphics g,TabControl tabs)
        {
            Palette p=colors();Rectangle page=tabs.DisplayRectangle;
            using(var pen=new Pen(p.Panel,5))
                if(page.Width>0&&page.Height>0)g.DrawRectangle(pen,page.Left-2,page.Top-2,page.Width+3,page.Height+3);
            if(tabs.Alignment!=TabAlignment.Top)return;
            using(var b=new SolidBrush(p.Panel))g.FillRectangle(b,0,0,tabs.ClientSize.Width,Math.Max(0,page.Top-3));
            for(int i=0;i<tabs.TabCount;i++)
            {
                Rectangle r=tabs.GetTabRect(i);bool active=tabs.SelectedIndex==i;
                if(active)using(var shape=StudioIcons.Rounded(Rectangle.Inflate(r,-3,-2),5))using(var brush=new SolidBrush(p.Selection))g.FillPath(brush,shape);
                TextRenderer.DrawText(g,tabs.TabPages[i].Text,tabs.Font,r,p.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.EndEllipsis);
                if(active)using(var pen=new Pen(p.Accent,2))g.DrawLine(pen,r.Left+7,r.Bottom-2,r.Right-7,r.Bottom-2);
            }
        }
    }

    internal sealed class StudioTabButton : Button
    {
        public Func<Palette> Colors;
        public bool Selected;
        private bool hover;
        public StudioTabButton(){SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);FlatStyle=FlatStyle.Flat;}
        protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);hover=true;Invalidate();}
        protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=false;Invalidate();}
        protected override void OnPaint(PaintEventArgs e)
        {
            Palette p=Colors==null?Palette.For(true):Colors();
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using(var shape=StudioIcons.Rounded(new Rectangle(0,1,Width-1,Height-3),7))
            using(var brush=new SolidBrush(hover?p.Hover:Selected?p.Selection:p.Panel))e.Graphics.FillPath(brush,shape);
            using(var textFont=new Font(Font,FontStyle.Regular))
                TextRenderer.DrawText(e.Graphics,Text,textFont,Rectangle.Inflate(ClientRectangle,-8,0),Selected&&!hover?p.Accent:p.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
            if(Selected)using(var pen=new Pen(p.Accent,3))e.Graphics.DrawLine(pen,12,Height-3,Width-12,Height-3);
            if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-4,-4),p.Accent,BackColor);
        }
    }
}

