using System.IO;
using TeamsMedic.App.Safety;

namespace TeamsMedic.App.Services;

public sealed class CacheCleaner(SafePathValidator safePathValidator, RepairLogger logger)
{
    public void ClearCacheFolder(string path, bool dryRun)
    {
        logger.Info($"Preparing to clear cache path: {path}");

        if (!safePathValidator.IsSafeTeamsCachePath(path))
        {
            logger.Warn($"Refusing to clear unsafe path: {path}");
            return;
        }

        if (!Directory.Exists(path))
        {
            logger.Info($"Cache path does not exist: {path}");
            return;
        }

        if (dryRun)
        {
            logger.Info($"Dry run: would delete contents of {path}");
            return;
        }

        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                TryDeleteFile(file);
            }

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                TryDeleteDirectory(directory);
            }

            logger.Info($"Finished clearing cache contents: {path}");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error("Permission denied while clearing cache. Close Teams and try again.", ex);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed while clearing cache path: {path}", ex);
        }
    }

    private void TryDeleteFile(string file)
    {
        try
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
            logger.Info($"Deleted file: {file}");
        }
        catch (Exception ex)
        {
            logger.Error($"Could not delete file: {file}", ex);
        }
    }

    private void TryDeleteDirectory(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
            logger.Info($"Deleted folder: {directory}");
        }
        catch (Exception ex)
        {
            logger.Error($"Could not delete folder: {directory}", ex);
        }
    }
}
