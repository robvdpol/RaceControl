namespace RaceControl.Core.Converters;

[ValueConversion(typeof(long), typeof(TimeSpan))]
public class TicksToTimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long longValue)
        {
            return TimeSpan.FromTicks(longValue);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}