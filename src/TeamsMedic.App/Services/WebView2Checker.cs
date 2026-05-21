using Microsoft.Win32;

namespace TeamsMedic.App.Services;

public sealed class WebView2Checker
{
    public bool IsWebView2RuntimePresent()
    {
        return HasWebView2Key(Registry.CurrentUser)
               || HasWebView2Key(Registry.LocalMachine);
    }

    private static bool HasWebView2Key(RegistryKey hive)
    {
        try
        {
            using var key = hive.OpenSubKey(@"Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            return key is not null;
        }
        catch
        {
            return false;
        }
    }
}
