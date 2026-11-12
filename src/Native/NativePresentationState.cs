using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    // Some native pages configure trackbar ranges directly through TBM_*.
    // WinForms otherwise recreates that HWND using its stale default range.
    internal sealed class NativePresentationState : IDisposable
    {
        private readonly TrackBar slider;private readonly ComboBox combo;
        private readonly int minimum,maximum,value,index,itemCount;private readonly object selected;private readonly string text;
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle,int message,IntPtr w,IntPtr l);
        [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] private static extern IntPtr SendText(IntPtr handle,int message,IntPtr w,string text);
        internal static void SetPlaceholder(ComboBox combo,string text){SendText(combo.Handle,0x000c,IntPtr.Zero,text);}
        public NativePresentationState(Control control)
        {
            slider=control as TrackBar;combo=control as ComboBox;
            if(slider!=null)
            {
                minimum=slider.IsHandleCreated?SendMessage(slider.Handle,0x0401,IntPtr.Zero,IntPtr.Zero).ToInt32():slider.Minimum;
                maximum=slider.IsHandleCreated?SendMessage(slider.Handle,0x0402,IntPtr.Zero,IntPtr.Zero).ToInt32():slider.Maximum;
                value=slider.Value;
                minimum=Math.Min(minimum,Math.Min(slider.Minimum,value));maximum=Math.Max(maximum,Math.Max(slider.Maximum,value));
                slider.SetRange(minimum,maximum);
            }
            if(combo!=null){index=combo.SelectedIndex;selected=combo.SelectedItem;text=combo.Text;itemCount=combo.Items.Count;}
        }
        public void Dispose()
        {
            if(slider!=null&&!slider.IsDisposed)
            {
                slider.SetRange(minimum,maximum);
                if(slider.IsHandleCreated){SendMessage(slider.Handle,0x0407,IntPtr.Zero,new IntPtr(minimum));SendMessage(slider.Handle,0x0408,IntPtr.Zero,new IntPtr(maximum));SendMessage(slider.Handle,0x0405,new IntPtr(1),new IntPtr(value));}
                if(slider.Value!=value)slider.Value=Math.Max(minimum,Math.Min(maximum,value));
            }
            if(combo!=null&&!combo.IsDisposed&&combo.Items.Count==itemCount)
            {
                if(selected!=null&&combo.Items.Contains(selected)){if(!Object.Equals(combo.SelectedItem,selected))combo.SelectedItem=selected;}
                else if(index<0){if(combo.SelectedIndex!=-1)combo.SelectedIndex=-1;if(combo.DropDownStyle!=ComboBoxStyle.DropDownList&&combo.Text!=text)combo.Text=text;}
                if(combo.DropDownStyle!=ComboBoxStyle.DropDownList&&combo.Text!=text)combo.Text=text;
                if(index<0&&combo.DropDownStyle==ComboBoxStyle.DropDownList&&combo.Text!=text)SetPlaceholder(combo,text);
            }
        }
    }
}
