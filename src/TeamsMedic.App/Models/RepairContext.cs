namespace TeamsMedic.App.Models;

public sealed class RepairContext
{
    public bool DryRun { get; set; }
    public bool RefreshNotificationArea { get; set; }
    public string? SelectedIssue { get; set; }
    public string? WebTestResult { get; set; }
    public string? UserNotes { get; set; }
    public List<string> LastRepairActions { get; } = [];
}
