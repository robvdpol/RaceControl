namespace RaceControl.Extensions;

public static class CommandExtensions
{
    public static bool TryExecute(this ICommand command, object parameter = null)
    {
        if (command.CanExecute(parameter))
        {
            command.Execute(parameter);

            return true;
        }

        return false;
    }
}