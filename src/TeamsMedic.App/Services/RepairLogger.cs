using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace TeamsMedic.App.Services;

public sealed class RepairLogger
{
    private readonly StringBuilder _buffer = new();

    public ObservableCollection<string> Entries { get; } = [];

    public string FullLog => _buffer.ToString();

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var suffix = exception is null ? string.Empty : $" ({exception.GetType().Name}: {exception.Message})";
        Write("ERROR", message + suffix);
    }

    private void Write(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        _buffer.AppendLine(line);

        if (Application.Current?.Dispatcher?.CheckAccess() == false)
        {
            Application.Current.Dispatcher.Invoke(() => Entries.Add(line));
        }
        else
        {
            Entries.Add(line);
        }
    }
}
