# Teams Medic

Teams Medic is a small Windows app that helps fix common Microsoft Teams desktop problems without making you dig through hidden folders, mysterious settings pages, or that one Teams cache path nobody should have to memorize.

It is built for regular people, remote workers, helpdesk folks, and anyone who has ever said:

> "Teams is open, my microphone turns on, but where is the call window?"


## The Short Version

Teams Medic is:

- A local repair toolkit for Microsoft Teams desktop issues
- Focused mainly on New Microsoft Teams on Windows 10 and Windows 11
- Safe by default
- Open source
- Not affiliated with Microsoft
- Not a data collector
- Not a registry-tweaking chaos button

Everything runs on your computer. Nothing is uploaded anywhere.

## Download

Most people should download the ready-to-run Windows app:

[Download Teams Medic v0.1.0 for Windows x64](https://github.com/Henr1ko/teams-medic/releases/download/v0.1.0/TeamsMedic-v0.1.0-win-x64.exe.exe)

You can also browse all releases here:

[Teams Medic releases](https://github.com/Henr1ko/teams-medic/releases)

## What Can It Help With?

Teams Medic includes guided fixes for:

- Teams notifications not showing
- Teams call popups not appearing
- Teams freezing or crashing
- Teams not starting
- Clearing Teams cache safely
- Creating a local report for IT support

It also has a **Quick Repair** button for the classic "Teams is being weird, please stop being weird" situation.

## What Quick Repair Does

Quick Repair is the safest default repair.

It can:

- Detect if Teams is running
- Ask before closing Teams
- Try to close Teams normally first
- Ask again before force-closing anything
- Clear only known Teams cache folders
- Optionally refresh the notification area
- Start Teams again with `msteams:`
- Show a clear log of what happened

It does **not** touch your documents, chats, emails, Office files, browser data, or random folders.

## What It Does Not Do

Teams Medic does not:

- Delete user documents
- Delete emails or chats
- Modify registry settings
- Change firewall rules
- Change antivirus settings
- Disable security software
- Change company policy
- Upload logs
- Upload reports
- Automatically uninstall or reinstall Teams

If a repair needs something risky, Teams Medic should explain that instead of silently doing something surprising.

## Safety Stuff.

Teams cache clearing is limited to these known Teams folders:

```text
%LOCALAPPDATA%\Packages\MSTeams_8wekyb3d8bbwe\LocalCache\Microsoft\MSTeams
%APPDATA%\Microsoft\Teams
```

Before deleting cache contents, Teams Medic checks that the path looks like a real Teams cache path.

In other words: it is not a "delete whatever folder someone typed in" kind of tool. That would be exciting in the bad way.

## Dry Run Mode

Dry run mode lets you see what Teams Medic **would** do without actually deleting cache files or restarting things.

Useful if you are cautious, curious, or currently on a call with someone who would notice if Teams suddenly vanished.

## IT Report

Teams Medic can create a local text report on your Desktop.

The report can include:

- Windows version
- .NET runtime version
- Whether New Teams was detected
- Whether Classic Teams cache exists
- Teams processes currently running
- Cache folder status
- WebView2 Runtime detection
- Audio device names, if available
- Last repair actions performed
- Your selected issue
- Your notes
- Your Teams web test result

The report clearly says:

```text
This report was generated locally by Teams Medic and was not uploaded anywhere.
```

Username and machine name are redacted by default.

## Teams Support Logs

Teams has its own support logs too.

You can collect them manually by pressing:

```text
Ctrl + Alt + Shift + 1
```

You can also collect logs from the Teams system tray icon, depending on your Teams version and company setup.

## For Normal Users

Download the release EXE, run it, and start with **Quick Repair** or the guided fix that matches your problem.

If you are not sure what to pick, try:

1. **Guided Fix: Notifications** if Teams is quiet when it should not be
2. **Guided Fix: Calls Not Showing** if calls activate your mic but no popup appears
3. **Quick Repair** if Teams is generally acting cursed
4. **Create IT Report** if you need to send useful details to support

No administrator rights should be needed for normal repairs.

## For Developers

Teams Medic is a .NET WPF desktop app.

Requirements:

- Windows 10 or Windows 11
- .NET 8 SDK or newer

Build:

```powershell
dotnet build -c Release
```

Publish a self-contained single-file Windows x64 EXE:

```powershell
dotnet publish src/TeamsMedic.App/TeamsMedic.App.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The published EXE will be here:

```text
src\TeamsMedic.App\bin\Release\net8.0-windows\win-x64\publish\
```

## Project Structure

```text
TeamsMedic/
  src/
    TeamsMedic.App/
      Views/
      ViewModels/
      Services/
      Models/
      Repairs/
      Safety/
  README.md
  LICENSE
  .gitignore
```

## Disclaimer

Teams Medic is not affiliated with Microsoft.

Microsoft Teams, Windows, and related names belong to Microsoft. Teams Medic is an independent open-source utility that uses publicly documented Windows and Teams behavior to help with local troubleshooting.

## License

MIT
