using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    // Keep native AutoScroll semantics and original control identity. A clipped
    // viewport excludes the OS scrollbars; slim, themed controls operate the
    // same AutoScrollPosition instead of replacing page/input behavior.
    internal sealed class StudioScrollHost : UserControl
    {
        private readonly ScrollableControl content;
        private readonly Func<Size> required;
        private readonly Panel viewport=new Panel();
        private readonly StudioScrollThumb vertical,horizontal;
        private readonly bool horizontalAllowed;
        private bool arranging;
        public StudioScrollHost(ScrollableControl content,Func<Size> required,Func<Palette> colors,bool horizontalAllowed)
        {
            this.content=content;this.required=required;this.horizontalAllowed=horizontalAllowed;Dock=DockStyle.Fill;Margin=Padding.Empty;
            vertical=new StudioScrollThumb {Colors=colors,Vertical=true,AccessibleName="Inhalt vertikal scrollen"};horizontal=new StudioScrollThumb {Colors=colors,Vertical=false,AccessibleName="Inhalt horizontal scrollen"};
            Controls.Add(viewport);Controls.Add(vertical);Controls.Add(horizontal);content.Parent=viewport;content.Dock=DockStyle.None;content.Location=Point.Empty;
            vertical.Changed+=delegate{content.AutoScrollPosition=new Point(-content.AutoScrollPosition.X,vertical.Value);};horizontal.Changed+=delegate{content.AutoScrollPosition=new Point(horizontal.Value,-content.AutoScrollPosition.Y);};
            content.Layout+=delegate{Arrange();};content.Scroll+=delegate{Sync();};SizeChanged+=delegate{Arrange();};
        }
        public void Arrange()
        {
            if(arranging||Width<1||Height<1)return;arranging=true;
            try
            {
                for(int pass=0;pass<3;pass++)
                {
                Size size=required();bool v=size.Height>Height;bool h=horizontalAllowed&&size.Width>Width-(v?13:0);v=size.Height>Height-(h?13:0);h=horizontalAllowed&&size.Width>Width-(v?13:0);
                viewport.SetBounds(0,0,Math.Max(1,Width-(v?13:0)),Math.Max(1,Height-(h?13:0)));
                vertical.Visible=v;horizontal.Visible=h;vertical.SetBounds(viewport.Right+2,0,11,viewport.Height);horizontal.SetBounds(0,viewport.Bottom+2,viewport.Width,11);
                content.SetBounds(0,0,viewport.Width+(v?SystemInformation.VerticalScrollBarWidth:0),viewport.Height+(h||!horizontalAllowed?SystemInformation.HorizontalScrollBarHeight:0));
                vertical.Maximum=Math.Max(0,size.Height-viewport.Height);vertical.Viewport=viewport.Height;horizontal.Maximum=Math.Max(0,size.Width-viewport.Width);horizontal.Viewport=viewport.Width;Sync();
                if(required()==size)break;
                }
            }
            finally{arranging=false;}
        }
        private void Sync(){vertical.Value=-content.AutoScrollPosition.Y;horizontal.Value=-content.AutoScrollPosition.X;vertical.Invalidate();horizontal.Invalidate();}
    }
    internal sealed class StudioScrollThumb : Control
    {
        public Func<Palette> Colors;public bool Vertical;public int Maximum,Viewport,Value;public event EventHandler Changed;
        private int drag=-1;
        public StudioScrollThumb(){SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint,true);Cursor=Cursors.Hand;TabStop=false;}
        private Rectangle Thumb()
        {
            int length=Vertical?Height:Width;int size=Math.Min(length,Math.Max(30,(int)((long)length*Math.Max(1,Viewport)/Math.Max(1,Viewport+Maximum))));
            int pos=Maximum<=0?0:(int)((long)Math.Max(0,Math.Min(Value,Maximum))*(length-size)/Maximum);
            return Vertical?new Rectangle(3,pos,5,size):new Rectangle(pos,3,size,5);
        }
        protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);if(Maximum<=0)return;using(var shape=StudioIcons.Rounded(Thumb(),3))using(var brush=new SolidBrush(Color.FromArgb(drag>=0?230:125,Colors().Muted)))e.Graphics.FillPath(brush,shape);}
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Rectangle thumb=Thumb();int point=Vertical?e.Y:e.X;drag=thumb.Contains(e.Location)?point-(Vertical?thumb.Top:thumb.Left):(Vertical?thumb.Height:thumb.Width)/2;Capture=true;MoveThumb(point);}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(drag>=0)MoveThumb(Vertical?e.Y:e.X);}
        protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);drag=-1;Capture=false;Invalidate();}
        private void MoveThumb(int point){Rectangle thumb=Thumb();int travel=(Vertical?Height-thumb.Height:Width-thumb.Width);Value=travel<=0?0:Math.Max(0,Math.Min(Maximum,(int)((long)(point-drag)*Maximum/travel)));if(Changed!=null)Changed(this,EventArgs.Empty);Invalidate();}
        protected override void OnMouseWheel(MouseEventArgs e){Value=Math.Max(0,Math.Min(Maximum,Value-e.Delta*3*Font.Height/120));if(Changed!=null)Changed(this,EventArgs.Empty);Invalidate();base.OnMouseWheel(e);}
    }
}
