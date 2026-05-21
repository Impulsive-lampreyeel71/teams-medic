namespace TeamsMedic.App.Services;

public interface IUserPrompt
{
    bool Confirm(string title, string message);
    void ShowInfo(string title, string message);
    void ShowWarning(string title, string message);
}
