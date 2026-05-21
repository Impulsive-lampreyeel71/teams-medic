namespace TeamsMedic.App.Models;

public sealed record DetectionSnapshot(
    bool NewTeamsPackageDetected,
    bool NewTeamsCacheExists,
    bool ClassicTeamsCacheExists,
    bool WebView2Detected,
    string WindowsVersion,
    bool IsAdmin,
    IReadOnlyList<TeamsProcessInfo> RunningTeamsProcesses);
