using System.Windows;

namespace RaceControl.Core.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}