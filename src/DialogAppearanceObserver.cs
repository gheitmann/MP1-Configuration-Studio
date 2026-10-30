using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    // In-process UI-thread hook only. Observe a Form's show request before its
    // first visible paint; Idle is too late for modal plugin configuration.
    internal sealed class DialogAppearanceObserver : IDisposable, IMessageFilter
    {
        private readonly Action<Form> prepare;
        private readonly HashSet<Form> prepared=new HashSet<Form>();
        private readonly HookProc callback;
        private IntPtr hook;
        private bool disposed;
        private delegate IntPtr HookProc(int code,IntPtr wParam,IntPtr lParam);
        [StructLayout(LayoutKind.Sequential)] private struct SentMessage
        {public IntPtr LParam,WParam;public uint Message;public IntPtr Window;}
        [StructLayout(LayoutKind.Sequential)] private struct WindowPosition
        {public IntPtr Window,After;public int X,Y,Width,Height;public uint Flags;}
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll",SetLastError=true)] private static extern IntPtr SetWindowsHookEx(int kind,HookProc proc,IntPtr module,uint thread);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr w,IntPtr l);
        public DialogAppearanceObserver(Action<Form> prepare)
        {
            this.prepare=prepare;callback=Observe;
            hook=SetWindowsHookEx(4,callback,IntPtr.Zero,GetCurrentThreadId());
            if(hook==IntPtr.Zero)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            Application.AddMessageFilter(this);
        }
        private void PrepareWindow(IntPtr handle)
        {
            var form=Control.FromHandle(handle) as Form;
            if(form==null||form.IsDisposed||form.Disposing||!prepared.Add(form))return;
            form.FormClosed+=delegate{prepared.Remove(form);};
            try{prepare(form);}
            catch(Exception ex){prepared.Remove(form);Trace.TraceError("Dialog appearance: {0}",ex);}
        }
        private IntPtr Observe(int code,IntPtr w,IntPtr l)
        {
            if(code>=0&&!disposed)
            {
                var message=(SentMessage)Marshal.PtrToStructure(l,typeof(SentMessage));
                bool show=message.Message==0x0018&&message.WParam!=IntPtr.Zero;
                if(message.Message==0x0046&&message.LParam!=IntPtr.Zero)
                    show=(((WindowPosition)Marshal.PtrToStructure(message.LParam,typeof(WindowPosition))).Flags&0x0040)!=0;
                if(show)PrepareWindow(message.Window);
            }
            return CallNextHookEx(hook,code,w,l);
        }
        public bool PreFilterMessage(ref Message message)
        {
            if(message.Msg==0x000f&&!disposed)
            {
                Control control=Control.FromChildHandle(message.HWnd);Form form=control==null?null:control.FindForm();
                if(form!=null)PrepareWindow(form.Handle);
            }
            return false;
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;Application.RemoveMessageFilter(this);
            if(hook!=IntPtr.Zero){UnhookWindowsHookEx(hook);hook=IntPtr.Zero;}prepared.Clear();
        }
    }
}
