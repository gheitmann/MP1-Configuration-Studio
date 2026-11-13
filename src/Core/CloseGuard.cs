using System;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class CloseGuard
    {
        public static bool ShouldPrompt(CloseReason reason,int unsavedChanges,bool restarting,bool suppressed)
        {
            return !suppressed&&!restarting&&reason==CloseReason.UserClosing&&unsavedChanges>0;
        }

        public static string Prompt(int changes,bool english)
        {
            if(english)
                return changes==1
                    ?"One change has not been saved. Close Studio and discard it?"
                    :"There are "+changes+" unsaved changes. Close Studio and discard them?";
            return changes==1
                ?"Eine Änderung ist noch nicht gespeichert. Studio schließen und die Änderung verwerfen?"
                :"Es sind "+changes+" Änderungen noch nicht gespeichert. Studio schließen und alle Änderungen verwerfen?";
        }

        public static string RestartPrompt(int changes,bool english)
        {
            if(changes<=0)
                return english?"Restart Studio now?":"Studio jetzt neu starten?";
            if(english)
                return changes==1
                    ?"Restart Studio now? One unsaved change will be lost."
                    :"Restart Studio now? "+changes+" unsaved changes will be lost.";
            return changes==1
                ?"Studio jetzt neu starten? Eine ungespeicherte Änderung geht verloren."
                :"Studio jetzt neu starten? "+changes+" ungespeicherte Änderungen gehen verloren.";
        }
    }
}
