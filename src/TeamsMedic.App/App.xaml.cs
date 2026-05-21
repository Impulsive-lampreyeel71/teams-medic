using System.Windows;
using TeamsMedic.App.Repairs;
using TeamsMedic.App.Safety;
using TeamsMedic.App.Services;
using TeamsMedic.App.ViewModels;

namespace TeamsMedic.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = new RepairLogger();
        var prompt = new MessageBoxUserPrompt();
        var adminChecker = new AdminChecker();
        var webView2Checker = new WebView2Checker();
        var processManager = new ProcessManager(logger);
        var teamsDetector = new TeamsDetector(processManager, webView2Checker, adminChecker, logger);
        var cacheCleaner = new CacheCleaner(new SafePathValidator(), logger);
        var notificationHelper = new NotificationHelper(logger);
        var audioDeviceChecker = new AudioDeviceChecker(logger);
        var reportGenerator = new ReportGenerator(teamsDetector, processManager, webView2Checker, audioDeviceChecker, logger);
        var orchestrator = new RepairOrchestrator(teamsDetector, processManager, cacheCleaner, notificationHelper, reportGenerator, logger, prompt);
        var viewModel = new MainViewModel(orchestrator, teamsDetector, notificationHelper, logger, prompt);

        new MainWindow
        {
            DataContext = viewModel
        }.Show();
    }
}
