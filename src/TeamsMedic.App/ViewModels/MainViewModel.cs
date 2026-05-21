using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TeamsMedic.App.Models;
using TeamsMedic.App.Repairs;
using TeamsMedic.App.Services;

namespace TeamsMedic.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RepairOrchestrator _orchestrator;
    private readonly TeamsDetector _teamsDetector;
    private readonly NotificationHelper _notificationHelper;
    private readonly RepairLogger _logger;
    private readonly IUserPrompt _prompt;
    private readonly RepairContext _context = new();
    private string _newTeamsStatus = "Checking...";
    private string _classicTeamsStatus = "Checking...";
    private string _webView2Status = "Checking...";
    private string _adminStatus = "Checking...";
    private string _statusMessage = "Ready";
    private string _userNotes = string.Empty;
    private string _webTestResult = "Not entered";
    private bool _dryRun;
    private bool _refreshNotificationArea;
    private bool _showAdvancedWarning;

    public MainViewModel(
        RepairOrchestrator orchestrator,
        TeamsDetector teamsDetector,
        NotificationHelper notificationHelper,
        RepairLogger logger,
        IUserPrompt prompt)
    {
        _orchestrator = orchestrator;
        _teamsDetector = teamsDetector;
        _notificationHelper = notificationHelper;
        _logger = logger;
        _prompt = prompt;

        Logs = logger.Entries;
        QuickRepairCommand = Command(_ => RunAsync("Quick Repair", ct => _orchestrator.QuickRepairAsync(SyncContext(), ct)));
        CallsNotShowingCommand = Command(_ => RunAsync("Calls Not Showing", ct => _orchestrator.CallsNotShowingAsync(SyncContext(), ct)));
        NotificationsCommand = Command(_ => RunAsync("Notifications Not Working", ct =>
        {
            var runCacheReset = _prompt.Confirm("Run cache reset?", "Open notification settings first, then run a Teams cache reset and restart Teams?");
            return _orchestrator.NotificationsNotWorkingAsync(SyncContext(), runCacheReset, ct);
        }));
        FreezesCommand = Command(_ => RunAsync("Teams Freezes or Crashes", ct => _orchestrator.FreezesOrCrashesAsync(SyncContext(), ct)));
        WillNotStartCommand = Command(_ => RunAsync("Teams Will Not Start", ct => _orchestrator.TeamsWillNotStartAsync(SyncContext(), ct)));
        CreateReportCommand = Command(_ => RunAsync("Create IT Report", async ct =>
        {
            var path = await _orchestrator.CreateReportAsync(SyncContext(), ct);
            _prompt.ShowInfo("Report Created", $"The IT report was created on your Desktop:\n\n{path}");
        }));
        CopyLogCommand = Command(_ =>
        {
            Clipboard.SetText(_logger.FullLog);
            StatusMessage = "Log copied to clipboard.";
            return Task.CompletedTask;
        });
        RefreshDetectionCommand = Command(_ => RefreshDetectionAsync());
        RestartExplorerCommand = Command(_ =>
        {
            _notificationHelper.RestartExplorer(DryRun);
            return Task.CompletedTask;
        });
        OpenNewCacheCommand = Command(_ =>
        {
            _notificationHelper.OpenFolder(_teamsDetector.NewTeamsCachePath);
            return Task.CompletedTask;
        });
        OpenClassicCacheCommand = Command(_ =>
        {
            _notificationHelper.OpenFolder(_teamsDetector.ClassicTeamsCachePath);
            return Task.CompletedTask;
        });
        OpenNotificationSettingsCommand = Command(_ =>
        {
            _notificationHelper.OpenNotificationSettings();
            return Task.CompletedTask;
        });
        OpenAppsSettingsCommand = Command(_ =>
        {
            _notificationHelper.OpenAppsSettings();
            return Task.CompletedTask;
        });
        FullCacheResetCommand = Command(_ => RunAsync("Clear Teams Cache Only", ct =>
        {
            if (!_prompt.Confirm("Advanced Tool", "Clear Teams cache only deletes contents from known Teams cache folders. It does not close or restart Teams. Continue?"))
            {
                return Task.CompletedTask;
            }

            _orchestrator.FullCacheReset(SyncContext());
            return Task.CompletedTask;
        }));
        ShowAdvancedCommand = Command(_ =>
        {
            ShowAdvancedWarning = true;
            _prompt.ShowWarning("Advanced Tools", "Advanced tools can close apps, clear Teams caches, or restart Explorer. They do not edit registry, firewall, antivirus, or policy settings.");
            return Task.CompletedTask;
        });

        _ = RefreshDetectionAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Logs { get; }

    public ICommand QuickRepairCommand { get; }
    public ICommand CallsNotShowingCommand { get; }
    public ICommand NotificationsCommand { get; }
    public ICommand FreezesCommand { get; }
    public ICommand WillNotStartCommand { get; }
    public ICommand CreateReportCommand { get; }
    public ICommand CopyLogCommand { get; }
    public ICommand RefreshDetectionCommand { get; }
    public ICommand RestartExplorerCommand { get; }
    public ICommand OpenNewCacheCommand { get; }
    public ICommand OpenClassicCacheCommand { get; }
    public ICommand OpenNotificationSettingsCommand { get; }
    public ICommand OpenAppsSettingsCommand { get; }
    public ICommand FullCacheResetCommand { get; }
    public ICommand ShowAdvancedCommand { get; }

    public string NewTeamsStatus { get => _newTeamsStatus; private set => SetField(ref _newTeamsStatus, value); }
    public string ClassicTeamsStatus { get => _classicTeamsStatus; private set => SetField(ref _classicTeamsStatus, value); }
    public string WebView2Status { get => _webView2Status; private set => SetField(ref _webView2Status, value); }
    public string AdminStatus { get => _adminStatus; private set => SetField(ref _adminStatus, value); }
    public string StatusMessage { get => _statusMessage; private set => SetField(ref _statusMessage, value); }

    public bool DryRun
    {
        get => _dryRun;
        set
        {
            if (SetField(ref _dryRun, value))
            {
                _context.DryRun = value;
                _logger.Info($"Dry run mode set to: {value}");
            }
        }
    }

    public bool RefreshNotificationArea
    {
        get => _refreshNotificationArea;
        set
        {
            if (SetField(ref _refreshNotificationArea, value))
            {
                _context.RefreshNotificationArea = value;
            }
        }
    }

    public string UserNotes
    {
        get => _userNotes;
        set
        {
            if (SetField(ref _userNotes, value))
            {
                _context.UserNotes = value;
            }
        }
    }

    public string WebTestResult
    {
        get => _webTestResult;
        set
        {
            if (SetField(ref _webTestResult, value))
            {
                _context.WebTestResult = value;
            }
        }
    }

    public bool ShowAdvancedWarning
    {
        get => _showAdvancedWarning;
        set => SetField(ref _showAdvancedWarning, value);
    }

    private RepairContext SyncContext()
    {
        _context.DryRun = DryRun;
        _context.RefreshNotificationArea = RefreshNotificationArea;
        _context.UserNotes = string.IsNullOrWhiteSpace(UserNotes) ? null : UserNotes;
        _context.WebTestResult = WebTestResult;
        return _context;
    }

    private async Task RunAsync(string name, Func<CancellationToken, Task> action)
    {
        try
        {
            StatusMessage = $"{name} running...";
            await action(CancellationToken.None);
            await RefreshDetectionAsync();
            StatusMessage = $"{name} complete.";
        }
        catch (Exception ex)
        {
            _logger.Error($"{name} failed.", ex);
            StatusMessage = $"{name} failed. See log.";
        }
    }

    private async Task RefreshDetectionAsync()
    {
        try
        {
            var snapshot = await _teamsDetector.GetSnapshotAsync();
            NewTeamsStatus = snapshot.NewTeamsPackageDetected || snapshot.NewTeamsCacheExists ? "New Teams detected" : "New Teams not detected";
            ClassicTeamsStatus = snapshot.ClassicTeamsCacheExists ? "Classic Teams leftovers detected" : "No Classic Teams cache";
            WebView2Status = snapshot.WebView2Detected ? "WebView2 detected" : "WebView2 not detected";
            AdminStatus = snapshot.IsAdmin ? "Admin mode: yes" : "Admin mode: no";
            _logger.Info("Detection status refreshed.");
        }
        catch (Exception ex)
        {
            _logger.Error("Detection refresh failed.", ex);
        }
    }

    private static RelayCommand Command(Func<object?, Task> execute) => new(execute);

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
