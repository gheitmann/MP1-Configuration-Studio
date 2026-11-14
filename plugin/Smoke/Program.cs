using System;
using MP1.ConfigurationStudio.Plugin;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var plugin = new StudioSettingsPlugin();
            string text, image, focus, picture;
            bool home = plugin.GetHome(out text, out image, out focus, out picture);
            Console.WriteLine("name=" + plugin.PluginName());
            Console.WriteLine("id=" + plugin.GetWindowId());
            Console.WriteLine("home=" + home + " text=" + text);
            Console.WriteLine("window=" + plugin.GetID);
            Console.WriteLine("description=" + plugin.Description());
            return plugin.GetWindowId() == StudioSettingsPlugin.WindowId ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }
}
