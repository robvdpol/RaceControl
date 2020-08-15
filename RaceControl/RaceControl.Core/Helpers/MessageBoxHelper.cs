using System.Windows;

namespace RaceControl.Core.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowInfo(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool AskQuestion(string message)
        {
            return MessageBox.Show(Application.Current.MainWindow, message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}