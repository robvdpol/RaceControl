namespace RaceControl.Core.Converters;

[ValueConversion(typeof(DateTime), typeof(bool))]
public class DatePassedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            if (parameter is not int days)
            {
                days = 0;
            }

            return DateTime.UtcNow >= date.AddDays(days);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public bool HasPassed(DateTime? date, int offset = 0)
    {
        return Convert(date, null, offset, CultureInfo.InvariantCulture) is true;
    }
}