using System;
using System.Globalization;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class CloseGuard
    {
        public static bool ShouldPrompt(CloseReason reason,int unsavedChanges,bool restarting,bool suppressed)
        {
            return !suppressed&&!restarting&&reason==CloseReason.UserClosing&&unsavedChanges>0;
        }

        public static string Prompt(int changes)
        {
            return changes==1
                ?Localization.T2("Eine Änderung ist noch nicht gespeichert. Studio schließen und die Änderung verwerfen?","One change has not been saved. Close Studio and discard it?")
                :String.Format(CultureInfo.CurrentCulture,Localization.T2("Es sind {0} Änderungen noch nicht gespeichert. Studio schließen und alle Änderungen verwerfen?","There are {0} unsaved changes. Close Studio and discard them?"),changes);
        }

        public static string RestartPrompt(int changes)
        {
            if(changes<=0)
                return Localization.T2("Studio jetzt neu starten?","Restart Studio now?");
            return changes==1
                ?Localization.T2("Studio jetzt neu starten? Eine ungespeicherte Änderung geht verloren.","Restart Studio now? One unsaved change will be lost.")
                :String.Format(CultureInfo.CurrentCulture,Localization.T2("Studio jetzt neu starten? {0} ungespeicherte Änderungen gehen verloren.","Restart Studio now? {0} unsaved changes will be lost."),changes);
        }
    }
}
