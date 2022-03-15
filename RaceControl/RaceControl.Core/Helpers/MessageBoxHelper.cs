namespace RaceControl.Core.Helpers;

public static class MessageBoxHelper
{
    public static void ShowInfo(string message, string caption = "Info")
    {
        var window = Application.Current.MainWindow;

        if (window != null)
        {
            MessageBox.Show(window, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public static void ShowError(string message, string caption = "Error")
    {
        var window = Application.Current.MainWindow;

        if (window != null)
        {
            MessageBox.Show(window, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static bool AskQuestion(string message, string caption = "Question")
    {
        var window = Application.Current.MainWindow;

        var result = window != null ?
            MessageBox.Show(window, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) :
            MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }
}