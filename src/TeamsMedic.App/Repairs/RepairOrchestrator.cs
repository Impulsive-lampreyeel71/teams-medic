using TeamsMedic.App.Models;
using TeamsMedic.App.Services;

namespace TeamsMedic.App.Repairs;

public sealed class RepairOrchestrator(
    TeamsDetector teamsDetector,
    ProcessManager processManager,
    CacheCleaner cacheCleaner,
    NotificationHelper notificationHelper,
    ReportGenerator reportGenerator,
    RepairLogger logger,
    IUserPrompt prompt)
{
    public async Task QuickRepairAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        context.SelectedIssue = "Quick Repair";
        context.LastRepairActions.Clear();
        logger.Info("Starting Quick Repair.");

        var running = processManager.GetRunningTeamsProcesses();
        if (running.Count > 0)
        {
            var close = prompt.Confirm("Close Teams?", "Teams appears to be running. Close Teams before clearing cache?");
            if (close)
            {
                context.LastRepairActions.Add("Attempted graceful Teams close.");
                var graceful = await processManager.CloseTeamsGracefullyAsync(cancellationToken);
                if (!graceful && prompt.Confirm("Force close Teams?", "Some Teams processes did not close. Force close them before cache repair?"))
                {
                    processManager.ForceKillTeamsProcesses();
                    context.LastRepairActions.Add("Force closed remaining Teams processes with user confirmation.");
                }
            }
            else
            {
                logger.Warn("User skipped closing Teams. Cache files in use may not be cleared.");
                context.LastRepairActions.Add("Skipped closing Teams.");
            }
        }
        else
        {
            logger.Info("No Teams processes detected.");
        }

        ClearKnownCaches(context);

        if (context.RefreshNotificationArea)
        {
            notificationHelper.RestartExplorer(context.DryRun);
            context.LastRepairActions.Add("Refreshed notification area by restarting Explorer.");
        }

        if (!context.DryRun)
        {
            notificationHelper.StartTeams();
            context.LastRepairActions.Add("Started Teams using msteams:.");
        }
        else
        {
            logger.Info("Dry run: would start Teams using msteams:.");
        }

        logger.Info("Quick Repair finished.");
    }

    public async Task CallsNotShowingAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        context.SelectedIssue = "Calls Not Showing";
        logger.Info("Calls checklist shown before opening Windows notification settings.");
        prompt.ShowInfo("Calls Not Showing Checklist",
            "Teams Medic will open Windows notification settings after this message.\n\nIn Settings > System > Notifications:\n\n1. Make sure Notifications is turned On.\n2. Make sure Do not disturb is turned Off.\n3. Scroll down to Notifications from apps and other senders.\n4. Find Microsoft Teams or Teams.\n5. Open it and make sure notifications are On.\n6. Make sure Show notification banners is On.\n7. Make sure Play a sound when a notification arrives is On.\n\nAlso check:\n\n- If you use multiple monitors, the call window may be opening off-screen.\n- Test once without a docking station or external monitor.\n\nWeb test: Open https://teams.microsoft.com, sign in, and ask someone to call you.\n\nIf web calls work, this is probably a desktop app or local Windows issue. If web calls fail, it is likely account, tenant, Teams policy, or company configuration.");
        notificationHelper.OpenNotificationSettings();
        notificationHelper.OpenWebTest();
        context.LastRepairActions.Add("Opened notification settings and web Teams test guidance.");

        if (prompt.Confirm("Run Quick Repair?", "Run Quick Repair too? This can help local desktop notification and call popup issues by closing Teams, clearing Teams cache, and starting Teams again."))
        {
            await QuickRepairAsync(context, cancellationToken);
            context.SelectedIssue = "Calls Not Showing";
        }
    }

    public async Task NotificationsNotWorkingAsync(RepairContext context, bool runCacheReset, CancellationToken cancellationToken = default)
    {
        context.SelectedIssue = "Notifications Not Working";
        logger.Info("Starting Notifications Not Working flow.");
        logger.Info("Notifications checklist shown before opening Windows notification settings.");
        prompt.ShowInfo("Notifications Checklist",
            "Teams Medic will open Windows notification settings after this message.\n\nIn Settings > System > Notifications:\n\n1. Make sure Notifications is turned On.\n2. Make sure Do not disturb is turned Off.\n3. Scroll down to Notifications from apps and other senders.\n4. Find Microsoft Teams or Teams.\n5. Open it and make sure notifications are On.\n6. Make sure Show notification banners is On.\n7. Make sure Play a sound when a notification arrives is On.\n\nIf Teams is not listed there, start Teams once, then come back to this settings page.");
        notificationHelper.OpenNotificationSettings();
        context.LastRepairActions.Add("Showed checklist and opened Windows notification settings.");

        if (runCacheReset)
        {
            await QuickRepairAsync(context, cancellationToken);
            context.SelectedIssue = "Notifications Not Working";
        }
        else if (!context.DryRun)
        {
            notificationHelper.StartTeams();
            context.LastRepairActions.Add("Started Teams using msteams:.");
        }

        logger.Info("Notifications flow finished.");
    }

    public async Task FreezesOrCrashesAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        context.SelectedIssue = "Teams Freezes or Crashes";
        if (prompt.Confirm("Run Quick Repair?", "Run Quick Repair first? This closes Teams, clears Teams cache, and starts Teams again."))
        {
            await QuickRepairAsync(context, cancellationToken);
            context.SelectedIssue = "Teams Freezes or Crashes";
        }

        var snapshot = await teamsDetector.GetSnapshotAsync(cancellationToken);
        logger.Info($"Windows version: {snapshot.WindowsVersion}");
        logger.Info($"WebView2 detected: {snapshot.WebView2Detected}");
        logger.Info($"New Teams package detected: {snapshot.NewTeamsPackageDetected}");
        logger.Info("Suggestion: update Teams and Windows. To collect Teams support logs manually, press Ctrl + Alt + Shift + 1 in Teams or use the Teams system tray icon.");
        context.LastRepairActions.Add("Checked Windows, WebView2, and Teams package status.");
        await reportGenerator.GenerateAsync(context, cancellationToken);
    }

    public async Task TeamsWillNotStartAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        context.SelectedIssue = "Teams Will Not Start";
        logger.Info("Starting Teams Will Not Start flow.");
        var packageDetected = await teamsDetector.IsNewTeamsPackageInstalledAsync(cancellationToken);
        logger.Info($"New Teams package detected: {packageDetected}");
        ClearKnownCaches(context);

        if (!context.DryRun)
        {
            notificationHelper.StartTeams();
            context.LastRepairActions.Add("Tried launching Teams using msteams:.");
        }
        else
        {
            logger.Info("Dry run: would launch Teams using msteams:.");
        }

        prompt.ShowInfo("If Teams Still Will Not Start",
            "Open Windows Settings > Apps > Installed Apps > Microsoft Teams > Advanced Options, then try Repair. If Repair does not help, consider Reset.\n\nTeams Medic does not automatically uninstall or reinstall Teams in v1.");
        logger.Info("Teams Will Not Start flow finished.");
    }

    public void FullCacheReset(RepairContext context)
    {
        context.SelectedIssue = "Advanced Tools - Full Cache Reset";
        ClearKnownCaches(context);
        logger.Info("Full cache reset finished.");
    }

    public Task<string> CreateReportAsync(RepairContext context, CancellationToken cancellationToken = default)
    {
        return reportGenerator.GenerateAsync(context, cancellationToken);
    }

    private void ClearKnownCaches(RepairContext context)
    {
        cacheCleaner.ClearCacheFolder(teamsDetector.NewTeamsCachePath, context.DryRun);
        cacheCleaner.ClearCacheFolder(teamsDetector.ClassicTeamsCachePath, context.DryRun);
        context.LastRepairActions.Add("Cleared known Teams cache folders if present.");
    }
}
