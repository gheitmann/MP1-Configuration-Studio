using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal sealed class NavigationChrome : NativeWindow, IDisposable
    {
        [DllImport("user32.dll")] internal static extern IntPtr SendMessage(IntPtr hwnd,int message,IntPtr w,IntPtr l);
        [DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr hwnd,IntPtr rect,IntPtr region,uint flags);
        public void Attach(IntPtr handle)
        {
            if(Handle!=handle){if(Handle!=IntPtr.Zero)ReleaseHandle();if(handle!=IntPtr.Zero)AssignHandle(handle);}
            if(handle!=IntPtr.Zero)SendMessage(handle,0x112c,new IntPtr(4),new IntPtr(4)); // TVS_EX_DOUBLEBUFFER
        }
        protected override void WndProc(ref Message message)
        {
            int kind=message.Msg;base.WndProc(ref message);
            // The top-level composition buffer now contains native scroll blits.
            // Repaint the fixed stripe without hiding/showing the tree per thumb move.
            if(kind==0x0114&&Handle!=IntPtr.Zero)RedrawWindow(Handle,IntPtr.Zero,IntPtr.Zero,0x0001);
        }
        public void Dispose(){if(Handle!=IntPtr.Zero)ReleaseHandle();}
    }
}
