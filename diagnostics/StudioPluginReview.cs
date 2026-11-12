using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        // Audited original 1.39 forms only. No arbitrary installed plugin
        // constructors or setup handlers are executed by this smoke test.
        private void RunPluginDialogChecks(XmlDocument report,XmlElement root,string isolatedRoot)
        {
            foreach(string descriptor in new[]{"GUIHome|MediaPortal.GUI.Home.GUIHomeSetupForm","GUITopbar|MediaPortal.Topbar.TopBarSetupForm","GUIRSSFeed|GUIRSSFeed.SetupForm","ProcessPlugins|ProcessPlugins.ViewModeSwitcher.ViewModeSwitcherConfig","ProcessPlugins|MediaPortal.ProcessPlugins.MusicDBReorg.MusicDBReorgSettings"})
            {
                string[] parts=descriptor.Split('|');var result=report.CreateElement("pluginDialog");root.AppendChild(result);result.SetAttribute("type",parts[1]);
                try
                {
                    Type type=Assembly.Load(parts[0]).GetType(parts[1],true);
                    using(var dialog=(Form)Activator.CreateInstance(type))
                    {
                        var before=ConfigHashes(isolatedRoot);
                        dialog.Shown+=delegate{dialog.BeginInvoke((Action)delegate{
                            try
                            {
                                before=ConfigHashes(isolatedRoot);
                                result.SetAttribute("preparedBeforeShown",dialogs.Contains(dialog).ToString());result.SetAttribute("composition",FrameRendering.IsComposited(dialog).ToString());
                                var values=Descendants(dialog).Where(c=>Value(c)!=null&&!(c is TextBoxBase&&((TextBoxBase)c).ReadOnly)).ToDictionary(c=>c,Value);
                                bool originalDark=appearance.Dark;SetDark(false);SetDark(true);SetDark(originalDark);
                                result.SetAttribute("valuesPreserved",values.All(pair=>!pair.Key.IsDisposed&&Value(pair.Key)==pair.Value).ToString());
                                result.SetAttribute("status","opened and checked");
                            }
                            catch(Exception ex){result.SetAttribute("status","failed");result.SetAttribute("error",ex.GetBaseException().Message);}
                            finally
                            {
                                var cancel=dialog.CancelButton;
                                if(cancel==null)cancel=Descendants(dialog).OfType<Button>().FirstOrDefault(button=>button.Name.IndexOf("cancel",StringComparison.OrdinalIgnoreCase)>=0);
                                if(cancel!=null){result.SetAttribute("nativeCancelInvoked","True");cancel.PerformClick();}
                                if(!dialog.IsDisposed&&dialog.Visible){dialog.DialogResult=DialogResult.Cancel;dialog.Close();}
                            }
                        });};
                        dialog.ShowDialog(form);
                        RecordChangedFiles(report,result,before,ConfigHashes(isolatedRoot));result.SetAttribute("cancelKeptConfigFiles",(result.SelectNodes("changedFile").Count==0).ToString());
                    }
                }
                catch(Exception ex){result.SetAttribute("status","failed");result.SetAttribute("error",ex.GetBaseException().Message);}
            }
        }
    }
}
