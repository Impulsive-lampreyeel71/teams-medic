using System.Diagnostics;
using System.IO;
using TeamsMedic.App.Models;

namespace TeamsMedic.App.Services;

public sealed class TeamsDetector(ProcessManager processManager, WebView2Checker webView2Checker, AdminChecker adminChecker, RepairLogger logger)
{
    public string NewTeamsCachePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "MSTeams_8wekyb3d8bbwe", "LocalCache", "Microsoft", "MSTeams");

    public string ClassicTeamsCachePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Teams");

    public async Task<DetectionSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var packageDetected = await IsNewTeamsPackageInstalledAsync(cancellationToken);
        var running = processManager.GetRunningTeamsProcesses();

        return new DetectionSnapshot(
            packageDetected,
            Directory.Exists(NewTeamsCachePath),
            Directory.Exists(ClassicTeamsCachePath),
            webView2Checker.IsWebView2RuntimePresent(),
            Environment.OSVersion.VersionString,
            adminChecker.IsRunningAsAdministrator(),
            running);
    }

    public async Task<bool> IsNewTeamsPackageInstalledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"if (Get-AppxPackage MSTeams -ErrorAction SilentlyContinue) { 'true' } else { 'false' }\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = (await outputTask).Trim();
            return output.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.Error("PowerShell package detection failed. Continuing without package status.", ex);
            return false;
        }
    }
}
