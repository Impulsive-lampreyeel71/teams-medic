using System.Diagnostics;
using TeamsMedic.App.Models;

namespace TeamsMedic.App.Services;

public sealed class ProcessManager(RepairLogger logger)
{
    private static readonly string[] ProcessNames = ["ms-teams", "msteams", "Teams", "Update"];

    public IReadOnlyList<TeamsProcessInfo> GetRunningTeamsProcesses()
    {
        var results = new List<TeamsProcessInfo>();

        foreach (var name in ProcessNames)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    using (process)
                    {
                        results.Add(new TeamsProcessInfo(process.Id, process.ProcessName, SafeGetTitle(process)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Could not inspect process '{name}'.", ex);
            }
        }

        return results.OrderBy(p => p.ProcessName).ThenBy(p => p.Id).ToList();
    }

    public async Task<bool> CloseTeamsGracefullyAsync(CancellationToken cancellationToken)
    {
        var anyFailed = false;
        foreach (var info in GetRunningTeamsProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var process = Process.GetProcessById(info.Id);
                logger.Info($"Requesting graceful close for {info.ProcessName} (PID {info.Id}).");
                if (!process.CloseMainWindow())
                {
                    logger.Warn($"{info.ProcessName} (PID {info.Id}) has no main window to close.");
                    anyFailed = true;
                    continue;
                }

                var exited = await Task.Run(() => process.WaitForExit(5000), cancellationToken);
                if (!exited)
                {
                    logger.Warn($"{info.ProcessName} (PID {info.Id}) did not exit after graceful close.");
                    anyFailed = true;
                }
                else
                {
                    logger.Info($"{info.ProcessName} (PID {info.Id}) closed.");
                }
            }
            catch (ArgumentException)
            {
                logger.Info($"{info.ProcessName} (PID {info.Id}) already exited.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to close {info.ProcessName} (PID {info.Id}).", ex);
                anyFailed = true;
            }
        }

        return !anyFailed;
    }

    public void ForceKillTeamsProcesses()
    {
        foreach (var info in GetRunningTeamsProcesses())
        {
            try
            {
                using var process = Process.GetProcessById(info.Id);
                logger.Warn($"Force killing {info.ProcessName} (PID {info.Id}).");
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            catch (ArgumentException)
            {
                logger.Info($"{info.ProcessName} (PID {info.Id}) already exited.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to force kill {info.ProcessName} (PID {info.Id}).", ex);
            }
        }
    }

    private static string? SafeGetTitle(Process process)
    {
        try
        {
            return string.IsNullOrWhiteSpace(process.MainWindowTitle) ? null : process.MainWindowTitle;
        }
        catch
        {
            return null;
        }
    }
}
