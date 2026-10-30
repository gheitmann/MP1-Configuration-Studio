using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        private readonly HashSet<Control> initializedNativeDisplays=new HashSet<Control>();
        // MP 1.39 writes the Blu-ray aspect ratio to bdplayerAR but reads the
        // movieplayer key when creating this selector. Correct its displayed
        // initial selection without replacing the native save handler.
        private void CorrectStoredSettingDisplay(Control page,bool afterNativeReload=false)
        {
            if(page==null||page.GetType().Name!="BDZoom"||(!afterNativeReload&&initializedNativeDisplays.Contains(page)))return;
            var combo=Descendants(page).OfType<ComboBox>().FirstOrDefault(c=>c.Name=="defaultZoomModeComboBox");if(combo==null||combo.Items.Count==0)return;
            Type settingsType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("MediaPortal.Profile.MPSettings",false)).FirstOrDefault(t=>t!=null);if(settingsType==null)return;
            object settings=Activator.CreateInstance(settingsType);
            bool wasBuilding=building;
            try{building=true;string saved=Convert.ToString(settingsType.GetMethod("GetValueAsString",new[]{typeof(string),typeof(string),typeof(string)}).Invoke(settings,new object[]{"bdplayerAR","defaultar",combo.Text}));int index=combo.FindStringExact(saved);if(index>=0&&combo.SelectedIndex!=index)combo.SelectedIndex=index;if(!afterNativeReload){baselines[combo]=Value(combo);edits.Remove(combo);}initializedNativeDisplays.Add(page);}
            finally{building=wasBuilding;var disposable=settings as IDisposable;if(disposable!=null)disposable.Dispose();}
        }
    }
}
