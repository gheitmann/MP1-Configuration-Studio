using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class StudioNavigation : UserControl
    {
        internal static readonly string[] Groups={"Allgemein","Oberfläche","Wiedergabe","Medienordner","Bedienung","Plugins","System & Diagnose"};
        internal static readonly string[] Icons={"settings","surface","play","folder","keyboard","plugin","system"};
        private readonly TreeView source;private readonly Func<TreeNode,Control> resolve;private readonly Func<TreeNode,bool> navigate;private readonly Func<Palette> colors;private readonly Action<string> tool;
        private readonly FlowLayoutPanel rows=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true};
        private readonly StudioScrollHost scroll;
        private TreeNode[] known=new TreeNode[0];private string active="";private readonly ToolTip tips=new ToolTip();
        public StudioNavigation(TreeView source,Func<TreeNode,Control> resolve,Func<TreeNode,bool> navigate,Func<Palette> colors,Action<string> tool)
        {this.source=source;this.resolve=resolve;this.navigate=navigate;this.colors=colors;this.tool=tool;Name="studioNavigation";Dock=DockStyle.Fill;rows.Padding=new Padding(0,0,0,SystemInformation.HorizontalScrollBarHeight);scroll=new StudioScrollHost(rows,()=>new Size(0,Math.Max(0,rows.GetPreferredSize(new Size(Math.Max(1,Width-18),0)).Height-SystemInformation.HorizontalScrollBarHeight)),colors,false);Controls.Add(scroll);rows.SizeChanged+=delegate{SizeRows();};Disposed+=delegate{tips.Dispose();};}
        private IEnumerable<TreeNode> Flatten(TreeNodeCollection nodes){foreach(TreeNode n in nodes){yield return n;foreach(TreeNode child in Flatten(n.Nodes))yield return child;}}
        internal string Kind(TreeNode node){var c=resolve(node);return c==null||c.GetType().Name=="UserControl"?node.Text:c.GetType().Name;}
        internal string Group(TreeNode node)
        {
            string k=Kind(node);if(k.EndsWith("Shares")||k.StartsWith("Picture"))return Groups[3];
            if(k=="Keys"||k=="GeneralKeyboardControl"||k.StartsWith("Remote"))return Groups[4];
            if(k.StartsWith("Gui")||k=="GUI"||k=="Screensaver"||k=="Blocked")return Groups[1];
            if(k=="GeneralDaemonTools"||k=="GeneralAutoplay"||k.StartsWith("Movie")||k.StartsWith("Music")||k.StartsWith("BD")||k.StartsWith("DVD")||k.StartsWith("TV")||k.StartsWith("Radio")||k.StartsWith("Filter"))return Groups[2];
            if(k.StartsWith("General"))return Groups[0];if(k.StartsWith("Plugin"))return Groups[5];return Groups[6];
        }
        internal string Label(TreeNode node)
        {
            switch(Kind(node)){
            case "Project":return "Über MediaPortal";case "General":return "Allgemein";case "Gui":case "GUI":return "Skin & Oberfläche";case "GuiLanguage":return "Sprache";case "GuiControls":return "Bedienelemente";case "GuiThumbs":return "Vorschaubilder";case "GuiScreensaver":case "Screensaver":return "Bildschirmschoner";case "GuiFileMenu":return "Dateimenü";case "GuiOSD":return "Bildschirmanzeige";case "GuiSkipSteps":return "Sprungweiten";case "Movies":return "Videos";case "MoviePlayer":return "Video-Player";case "MovieShares":return "Videoordner";case "MusicShares":return "Musikordner";case "MovieDatabase":return "Videodatenbank";case "MusicDatabase":return "Musikdatenbank";case "Music":return "Musik";case "Pictures":return "Bilder";case "Remote":return "Fernbedienung & Eingabe";case "FiltersSection":return "Codecs & Renderer";case "GeneralVolume":return "Lautstärke";case "GeneralKeyboardControl":return "Tastaturkürzel";case "Keys":return "Tasten & Klänge";case "GeneralStartupResume":return "Start & Fortsetzen";case "GeneralDynamicRefreshRate":return "Bildwiederholrate";case "GeneralDaemonTools":return "Virtuelles Laufwerk";case "GeneralAutoplay":return "Automatische Wiedergabe";case "PluginsNew":return "Plugins";case "MediaInfo":return "Medieninformationen";case "ThirdPartyChecks":return "Systemprüfung";default:return node.Text;}
        }
        internal string Icon(TreeNode node){return Icons[Array.IndexOf(Groups,Group(node))];}
        public void Sync(TreeNode selected,bool force)
        {
            TreeNode[] all=Flatten(source.Nodes).ToArray();string category=selected==null?Groups[0]:Group(selected);
            if(force||category!=active||!known.SequenceEqual(all))
            {
                using(FrameRendering.Suspend(this)){
                rows.SuspendLayout();foreach(Control c in rows.Controls.Cast<Control>().ToArray())c.Dispose();known=all;active=category;
                AddCaption("KONFIGURATION");
                for(int i=0;i<Groups.Length;i++)
                {
                    string group=Groups[i];TreeNode[] pages=all.Where(n=>Group(n)==group).ToArray();if(pages.Length==0)continue;
                    TreeNode target=pages.FirstOrDefault(n=>n.Parent==null)??pages[0];
                    var button=AddButton(group,Icons[i],delegate{navigate(target);},false);button.Tag=group;button.Selected=group==category;
                    if(group==category)
                    {
                        TreeNode[] shortcuts=pages.Where(n=>n.Parent==null||Group(n.Parent)!=group).ToArray();
                        if(shortcuts.Length>1)foreach(TreeNode page in shortcuts){TreeNode destination=page;var sub=AddButton(Label(page),null,delegate{navigate(destination);},true);sub.Tag=page;sub.Selected=Object.ReferenceEquals(selected,page);}
                    }
                }
                AddCaption("WERKZEUGE");AddButton("Konfigurationsdateien","folder",delegate{tool("config");},false);AddButton("Protokolle","file",delegate{tool("logs");},false);
                rows.ResumeLayout(true);SizeRows();scroll.Arrange();}
            }
            foreach(StudioAction button in rows.Controls.OfType<StudioAction>()){button.BackColor=colors().Panel;button.ForeColor=colors().Text;if(button.Tag is TreeNode)button.Selected=Object.ReferenceEquals(button.Tag,selected);button.Invalidate();}
            BackColor=rows.BackColor=colors().Panel;
        }
        private void AddCaption(string text){rows.Controls.Add(new Label {Name="studioMuted",Text=text,Font=Font,Height=Font.Height+18,Padding=new Padding(14,10,0,0),ForeColor=colors().Muted,BackColor=colors().Panel,Margin=new Padding(0,8,0,4)});}
        private StudioAction AddButton(string text,string icon,Action action,bool sub)
        {
            var button=new StudioAction {Text=text,IconName=icon,Colors=colors,Navigation=true,Secondary=sub,Font=Font,Height=Font.Height+(sub?17:23),Margin=new Padding(sub?27:0,2,0,2),Padding=Padding.Empty,BackColor=colors().Panel,AccessibleName=text};
            button.Click+=delegate{action();};tips.SetToolTip(button,text);rows.Controls.Add(button);return button;
        }
        private void SizeRows(){foreach(Control c in rows.Controls)c.Width=Math.Max(60,rows.ClientSize.Width-8-c.Margin.Left-(rows.VerticalScroll.Visible?SystemInformation.VerticalScrollBarWidth:0));}
    }
}
