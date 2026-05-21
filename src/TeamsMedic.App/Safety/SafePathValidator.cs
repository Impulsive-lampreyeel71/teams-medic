using System.IO;

namespace TeamsMedic.App.Safety;

public sealed class SafePathValidator
{
    private const string NewTeamsPackageFamily = "MSTeams_8wekyb3d8bbwe";
    private static readonly string ClassicTeamsEnding = Path.Combine("Microsoft", "Teams");

    public bool IsSafeTeamsCachePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return fullPath.Contains(NewTeamsPackageFamily, StringComparison.OrdinalIgnoreCase)
                   || fullPath.EndsWith(ClassicTeamsEnding, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
