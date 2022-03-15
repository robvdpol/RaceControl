namespace RaceControl.Core.Converters;

public class BooleanToWindowStateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? WindowState.Maximized : WindowState.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WindowState state)
        {
            return state == WindowState.Maximized;
        }

        return false;
    }
}