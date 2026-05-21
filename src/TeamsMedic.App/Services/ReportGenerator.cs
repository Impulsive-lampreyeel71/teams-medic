using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TeamsMedic.App.Models;

namespace TeamsMedic.App.Services;

public sealed class ReportGenerator(
    TeamsDetector teamsDetector,
    ProcessManager processManager,
    WebView2Checker webView2Checker,
    AudioDeviceChecker audioDeviceChecker,
    RepairLogger logger)
{
    public async Task<string> GenerateAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        logger.Info("Generating local IT report.");
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var reportPath = Path.Combine(desktop, $"TeamsMedic-Report-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        var snapshot = await teamsDetector.GetSnapshotAsync(cancellationToken);
        var audioDevices = await audioDeviceChecker.GetAudioDeviceNamesAsync(cancellationToken);
        var processes = processManager.GetRunningTeamsProcesses();

        var report = new StringBuilder();
        report.AppendLine("Teams Medic IT Report");
        report.AppendLine("=====================");
        report.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}");
        report.AppendLine($"Windows version: {RuntimeInformation.OSDescription}");
        report.AppendLine($".NET runtime version: {Environment.Version}");
        report.AppendLine("Username: [redacted]");
        report.AppendLine("Machine name: [redacted]");
        report.AppendLine($"New Teams package detected: {snapshot.NewTeamsPackageDetected}");
        report.AppendLine($"Classic Teams folder detected: {snapshot.ClassicTeamsCacheExists}");
        report.AppendLine($"New Teams cache path: {teamsDetector.NewTeamsCachePath}");
        report.AppendLine($"New Teams cache exists: {snapshot.NewTeamsCacheExists}");
        report.AppendLine($"Classic Teams cache path: {teamsDetector.ClassicTeamsCachePath}");
        report.AppendLine($"Classic Teams cache exists: {snapshot.ClassicTeamsCacheExists}");
        report.AppendLine($"WebView2 Runtime detected: {webView2Checker.IsWebView2RuntimePresent()}");
        report.AppendLine($"Admin mode: {snapshot.IsAdmin}");
        report.AppendLine($"User-selected issue: {context.SelectedIssue ?? "[not provided]"}");
        report.AppendLine($"Teams web test result: {context.WebTestResult ?? "[not provided]"}");
        report.AppendLine($"User notes: {context.UserNotes ?? "[not provided]"}");
        report.AppendLine();
        report.AppendLine("Teams processes currently running:");
        AppendList(report, processes.Select(p => $"{p.ProcessName} (PID {p.Id})"));
        report.AppendLine();
        report.AppendLine("Audio devices:");
        AppendList(report, audioDevices);
        report.AppendLine();
        report.AppendLine("Last repair actions performed:");
        AppendList(report, context.LastRepairActions);
        report.AppendLine();
        report.AppendLine("Recent log:");
        report.AppendLine(logger.FullLog);
        report.AppendLine("This report was generated locally by Teams Medic and was not uploaded anywhere.");

        await File.WriteAllTextAsync(reportPath, report.ToString(), cancellationToken);
        logger.Info($"Report written to: {reportPath}");
        return reportPath;
    }

    private static void AppendList(StringBuilder builder, IEnumerable<string> items)
    {
        var any = false;
        foreach (var item in items)
        {
            builder.AppendLine($"- {item}");
            any = true;
        }

        if (!any)
        {
            builder.AppendLine("- [none]");
        }
    }
}
