using System.Diagnostics;
using System.IO;

namespace TeamsMedic.App.Services;

public sealed class NotificationHelper(RepairLogger logger)
{
    public void OpenNotificationSettings() => StartUri("ms-settings:notifications", "Windows notification settings");

    public void OpenAppsSettings() => StartUri("ms-settings:appsfeatures", "Windows apps settings");

    public void StartTeams() => StartUri("msteams:", "Microsoft Teams");

    public void OpenWebTest() => StartUri("https://teams.microsoft.com", "Teams on the web");

    public void OpenFolder(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                logger.Warn($"Folder does not exist: {path}");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            logger.Info($"Opened folder: {path}");
        }
        catch (Exception ex)
        {
            logger.Error($"Could not open folder: {path}", ex);
        }
    }

    public void RestartExplorer(bool dryRun)
    {
        logger.Warn("Restarting Windows Explorer refreshes the taskbar and notification area.");
        if (dryRun)
        {
            logger.Info("Dry run: would restart Windows Explorer.");
            return;
        }

        try
        {
            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                using (process)
                {
                    process.Kill();
                }
            }

            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
            logger.Info("Windows Explorer restarted.");
        }
        catch (Exception ex)
        {
            logger.Error("Could not restart Windows Explorer.", ex);
        }
    }

    private void StartUri(string uri, string description)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            logger.Info($"Opened {description}: {uri}");
        }
        catch (Exception ex)
        {
            logger.Error($"Could not open {description}.", ex);
        }
    }
}
