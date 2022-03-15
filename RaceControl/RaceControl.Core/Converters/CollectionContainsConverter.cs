namespace RaceControl.Core.Converters;

public class CollectionContainsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is string identifier && values[1] is ICollection collection)
        {
            return collection.Cast<string>().Any(item => item == identifier) || string.IsNullOrWhiteSpace(identifier);
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}