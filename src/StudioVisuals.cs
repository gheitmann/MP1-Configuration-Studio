using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class StudioIcons
    {
        internal static GraphicsPath Rounded(RectangleF r,float radius)
        {
            var path=new GraphicsPath();float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));
            if(d<=0){path.AddRectangle(r);return path;}
            path.AddArc(r.X,r.Y,d,d,180,90);path.AddArc(r.Right-d,r.Y,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.X,r.Bottom-d,d,d,90,90);path.CloseFigure();return path;
        }
        internal static void Draw(Graphics g,string name,Rectangle bounds,Color color)
        {
            var state=g.Save();g.TranslateTransform(bounds.X,bounds.Y);g.ScaleTransform(bounds.Width/24f,bounds.Height/24f);g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var p=new Pen(color,1.6f)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;
                switch(name){
                case "surface":g.DrawRectangle(p,3,4,18,16);g.DrawLine(p,3,9,21,9);g.DrawLine(p,9,9,9,20);break;
                case "play":g.DrawRectangle(p,3,6,18,15);g.DrawLine(p,3,10,21,10);g.DrawLine(p,6,3,9,6);g.DrawLine(p,12,3,15,6);g.DrawLine(p,18,3,21,6);g.DrawLines(p,new[]{new Point(10,13),new Point(15,16),new Point(10,19),new Point(10,13)});break;
                case "folder":g.DrawLines(p,new[]{new Point(3,8),new Point(3,5),new Point(10,5),new Point(13,8),new Point(21,8),new Point(19,20),new Point(3,20),new Point(3,8),new Point(21,8)});break;
                case "keyboard":g.DrawRectangle(p,2,5,20,14);for(int y=8;y<14;y+=4)for(int x=5;x<21;x+=4)g.DrawLine(p,x,y,x+1,y);g.DrawLine(p,7,16,17,16);break;
                case "plugin":g.DrawLines(p,new[]{new Point(4,6),new Point(9,6),new Point(9,3),new Point(14,3),new Point(14,6),new Point(20,6),new Point(20,11),new Point(23,11),new Point(23,15),new Point(20,15),new Point(20,21),new Point(14,21),new Point(14,18),new Point(9,18),new Point(9,21),new Point(4,21),new Point(4,15),new Point(1,15),new Point(1,11),new Point(4,11),new Point(4,6)});break;
                case "file":g.DrawLines(p,new[]{new Point(5,2),new Point(14,2),new Point(20,8),new Point(20,22),new Point(5,22),new Point(5,2)});g.DrawLine(p,14,2,14,8);g.DrawLine(p,14,8,20,8);g.DrawLine(p,9,12,16,12);g.DrawLine(p,9,16,16,16);break;
                case "search":g.DrawEllipse(p,3,3,12,12);g.DrawLine(p,14,14,21,21);break;
                case "back":g.DrawLines(p,new[]{new Point(14,5),new Point(7,12),new Point(14,19)});break;
                case "forward":g.DrawLines(p,new[]{new Point(10,5),new Point(17,12),new Point(10,19)});break;
                case "help":g.DrawRectangle(p,3,3,18,18);g.DrawLine(p,15,3,15,21);break;
                case "system":g.DrawRectangle(p,3,3,18,13);g.DrawLine(p,8,21,16,21);g.DrawLine(p,12,16,12,21);break;
                case "check":g.DrawLines(p,new[]{new Point(4,12),new Point(9,17),new Point(20,6)});break;
                default:g.DrawLine(p,3,7,21,7);g.DrawLine(p,3,17,21,17);g.DrawEllipse(p,7,4,6,6);g.DrawEllipse(p,13,14,6,6);break;
                }
            }g.Restore(state);
        }
    }
    internal sealed class StudioAction : Button
    {
        public Func<Palette> Colors;public string IconName;public bool Selected;public bool Navigation;public bool Secondary;
        private bool hover;
        public StudioAction(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint|ControlStyles.ResizeRedraw,true);Cursor=Cursors.Hand;}
        protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
        protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
        protected override void OnPaint(PaintEventArgs e)
        {
            var p=Colors==null?Palette.For(true):Colors();e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            if(Selected||hover){using(var shape=StudioIcons.Rounded(new RectangleF(0,0,Width-1,Height-1),6))using(var b=new SolidBrush(hover?p.Hover:p.Selection))e.Graphics.FillPath(b,shape);}
            if(Selected&&Navigation)using(var b=new SolidBrush(p.Accent))e.Graphics.FillRectangle(b,0,7,3,Height-14);
            Color ink=!Enabled?p.Muted:hover?p.Text:Selected?p.Accent:Secondary?p.Muted:p.Text;
            int iconSize=Math.Min(23,Font.Height+2),left=Navigation?15:10;
            if(!String.IsNullOrEmpty(IconName)){StudioIcons.Draw(e.Graphics,IconName,new Rectangle(left,(Height-iconSize)/2,iconSize,iconSize),ink);left+=iconSize+12;}
            var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix;
            if(!Navigation&&String.IsNullOrEmpty(IconName))flags|=TextFormatFlags.HorizontalCenter;
            TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(left,0,Math.Max(0,Width-left-12),Height),ink,flags);
            if(Focused)using(var pen=new Pen(p.Accent))e.Graphics.DrawLine(pen,12,Height-3,Width-12,Height-3);
        }
    }
    internal sealed class StudioLogo : Control
    {
        public Func<Palette> Colors;
        protected override void OnPaint(PaintEventArgs e){var p=Colors();e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var shape=StudioIcons.Rounded(new RectangleF(0,0,Width-1,Height-1),8))using(var b=new SolidBrush(p.Primary))e.Graphics.FillPath(b,shape);TextRenderer.DrawText(e.Graphics,"MP",Font,ClientRectangle,Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
    }
}
