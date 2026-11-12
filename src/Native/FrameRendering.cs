using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class FrameRendering
    {
        private const long Composited=0x02000000;
        [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] private static extern IntPtr GetLong64(IntPtr h,int index);
        [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW")] private static extern IntPtr SetLong64(IntPtr h,int index,IntPtr value);
        [DllImport("user32.dll",EntryPoint="GetWindowLongW")] private static extern int GetLong32(IntPtr h,int index);
        [DllImport("user32.dll",EntryPoint="SetWindowLongW")] private static extern int SetLong32(IntPtr h,int index,int value);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
        [DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr h,IntPtr rect,IntPtr region,uint flags);
        [ThreadStatic] private static Dictionary<Control,State> suspended;
        private sealed class State {public IntPtr Handle;public int Depth;public bool WasVisible;}
        internal static bool IsVisible(Control control){return control.IsHandleCreated&&IsWindowVisible(control.Handle);}
        internal static bool IsComposited(Control control)
        {
            if(!control.IsHandleCreated)return false;
            long style=IntPtr.Size==8?GetLong64(control.Handle,-20).ToInt64():GetLong32(control.Handle,-20);
            return (style&Composited)!=0;
        }
        internal static void EnableComposition(Control control)
        {
            if(!control.IsHandleCreated||IsComposited(control))return;
            long style=IntPtr.Size==8?GetLong64(control.Handle,-20).ToInt64():GetLong32(control.Handle,-20);
            if(IntPtr.Size==8)SetLong64(control.Handle,-20,new IntPtr(style|Composited));
            else SetLong32(control.Handle,-20,unchecked((int)(style|Composited)));
        }
        internal static IDisposable Suspend(Control control)
        {
            if(suspended==null)suspended=new Dictionary<Control,State>();
            State state;
            if(!suspended.TryGetValue(control,out state))
            {
                state=new State();suspended[control]=state;
                if(control.IsHandleCreated&&!control.IsDisposed)
                {
                    state.Handle=control.Handle;state.WasVisible=IsWindowVisible(state.Handle);
                    if(state.WasVisible)SendMessage(state.Handle,0x000b,IntPtr.Zero,IntPtr.Zero);
                }
            }
            state.Depth++;return new Scope(control,state);
        }
        private sealed class Scope : IDisposable
        {
            private Control control;private readonly State state;
            public Scope(Control control,State state){this.control=control;this.state=state;}
            public void Dispose()
            {
                if(control==null)return;Control owner=control;control=null;
                if(--state.Depth!=0)return;suspended.Remove(owner);
                // Never enable redraw on an originally hidden window: WM_SETREDRAW
                // would otherwise give it WS_VISIBLE and expose an unfinished form.
                if(state.WasVisible&&!owner.IsDisposed&&owner.IsHandleCreated&&owner.Handle==state.Handle)
                {
                    SendMessage(state.Handle,0x000b,new IntPtr(1),IntPtr.Zero);
                    RedrawWindow(state.Handle,IntPtr.Zero,IntPtr.Zero,0x0485);
                }
            }
        }
    }
}
