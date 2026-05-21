using System.Diagnostics;

namespace TeamsMedic.App.Services;

public sealed class AudioDeviceChecker(RepairLogger logger)
{
    public async Task<IReadOnlyList<string>> GetAudioDeviceNamesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-CimInstance Win32_SoundDevice | Select-Object -ExpandProperty Name\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        }
        catch (Exception ex)
        {
            logger.Error("Audio device detection failed.", ex);
            return [];
        }
    }
}
