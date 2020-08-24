using System.Windows;

namespace RaceControl.Core.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowInfo(string message, string caption = "Info")
        {
            MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool AskQuestion(string message, string caption = "Question")
        {
            return MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}